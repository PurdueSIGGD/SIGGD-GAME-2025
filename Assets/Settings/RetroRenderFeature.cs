using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Scripting;

/// <summary>
/// A URP ScriptableRendererFeature that simulates a low-resolution rendering effect.
/// It renders the scene to a small offscreen render texture (e.g. 640x360) and then upscales
/// it back to the screen, creating a pixelated, retro look. The target resolution and filter
/// mode can be configured in the inspector.
/// </summary>
[Preserve]
public class RetroRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class RetroSettings
    {
        public int targetWidth = 640;
        public int targetHeight = 360;
        public FilterMode filterMode = FilterMode.Point;
        public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingTransparents;

        [Tooltip("Assign the 'Hidden/Universal Render Pipeline/Blit' shader here to ensure it's included in the build.")]
        public Shader blitShader;
    }

    public RetroSettings settings = new RetroSettings();
    private RetroRenderPass pass;

    public override void Create()
    {
        pass = new RetroRenderPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing)
    {
        pass?.Cleanup();
        base.Dispose(disposing);
    }
}

/// <summary>
/// Custom render pass that handles the downscaling and upscaling of the rendered image using the Render Graph API.
/// </summary>
public class RetroRenderPass : ScriptableRenderPass
{
    private RetroRenderFeature.RetroSettings settings;
    private Material blitMaterial;

    private class PassData
    {
        internal TextureHandle Source;
        internal Material Material;
    }

    public RetroRenderPass(RetroRenderFeature.RetroSettings retroSettings)
    {
        settings = retroSettings;
        renderPassEvent = settings.passEvent;

        if (settings.blitShader != null)
        {
            blitMaterial = CoreUtils.CreateEngineMaterial(settings.blitShader);
        }
        else
        {
            blitMaterial = CoreUtils.CreateEngineMaterial("Hidden/Universal Render Pipeline/Blit");
        }
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (blitMaterial == null)
            return;

        var resourceData = frameData.Get<UniversalResourceData>();

        TextureHandle src = resourceData.activeColorTexture;

        var lowResDesc = new RenderTextureDescriptor(settings.targetWidth, settings.targetHeight, RenderTextureFormat.Default, 0);

        TextureHandle lowResHandle = UniversalRenderer.CreateRenderGraphTexture(
            renderGraph,
            lowResDesc,
            "_RetroLowResRT",
            false,
            settings.filterMode
        );

        // Pass 1: downscale scene color into the low-res buffer
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Retro Downscale", out var passData))
        {
            passData.Source = src;
            passData.Material = blitMaterial;

            builder.UseTexture(src);
            builder.SetRenderAttachment(lowResHandle, 0);
            builder.AllowPassCulling(false);

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
            {
                RTHandle source = data.Source;
                Blitter.BlitTexture(ctx.cmd, source, new Vector4(1, 1, 0, 0), data.Material, 0);
            });
        }

        // Pass 2: upscale the low-res buffer back into the camera color target
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("Retro Upscale", out var passData))
        {
            passData.Source = lowResHandle;
            passData.Material = blitMaterial;

            builder.UseTexture(lowResHandle);
            builder.SetRenderAttachment(src, 0);
            builder.AllowPassCulling(false);

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
            {
                RTHandle source = data.Source;
                Blitter.BlitTexture(ctx.cmd, source, new Vector4(1, 1, 0, 0), data.Material, 0);
            });
        }
    }

    public void Cleanup()
    {
        if (blitMaterial != null)
            CoreUtils.Destroy(blitMaterial);
    }
}

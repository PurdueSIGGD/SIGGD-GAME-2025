using System;
using System.Collections;
using UnityEngine;

public class Effects : MonoBehaviour
{
    public Material vignetteMat;
    public float vignetteIntensity = 4f;
	private Coroutine vignetteTask;

	private static Effects instance;
	
	private int _vignettePowerID = Shader.PropertyToID("_VignettePower");
	
	Func<float, float> easeOutQuad = t => 1 - (1 - t) * (1 - t);
	Func<float, float> easeInQuad = t => t * t;
 
	private void Awake()
	{
		if(instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
		
		vignetteMat?.SetFloat(_vignettePowerID, 10f);
	}

	private float LerpByFunction(float start, float end, float t, Func<float, float> lerpFunction)
	{
		return Mathf.Lerp(start, end, lerpFunction(t));
	}

	private void VignetteEffect(float intensity, float duration = 1f) 
	{
		if(vignetteTask != null)
			StopCoroutine(vignetteTask);
		vignetteTask = StartCoroutine(vignette(intensity, duration));
	}
	private IEnumerator vignette(float intensity, float duration)
    {
        var startRadius = 10f;
        var targetRadius = intensity;
        
        vignetteMat.SetFloat(_vignettePowerID, startRadius);
        
        float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / duration;
			float currentRadius = LerpByFunction(startRadius, targetRadius, t, easeInQuad);
			vignetteMat.SetFloat(_vignettePowerID, currentRadius);
			yield return null;
		}

		elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / duration;
			float currentRadius = LerpByFunction(targetRadius, startRadius, t, easeOutQuad);
			vignetteMat.SetFloat(_vignettePowerID, currentRadius);
			yield return null;
		}
		
		vignetteMat.SetFloat(_vignettePowerID, startRadius);
		vignetteTask = null;

    }

	public static class SpecialEffects
	{
		public static void VignetteEffect(float intensity, float duration) => instance.VignetteEffect(intensity, duration);
	}
}
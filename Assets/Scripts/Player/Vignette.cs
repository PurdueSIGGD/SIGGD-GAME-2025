using System.Collections;
using UnityEngine;

public class Effects : MonoBehaviour
{
    public Material vignetteMat;
    public float vignetteIntensity = 4f;
	private Coroutine vignetteTask;

	private static Effects instance;
 
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
	}

	private void VignetteEffect(float intensity) 
	{
		if(vignetteTask != null)
			StopCoroutine(vignetteTask);
		vignetteTask = StartCoroutine(vignette(intensity));
	}
	private IEnumerator vignette(float intensity)
    {
        var targetRadius = 10f;
        var startRadius = intensity;
        var curRadius = startRadius;

		for (float t = 0; curRadius != targetRadius; t += Time.deltaTime)
		{
			curRadius = Mathf.Clamp(Mathf.Lerp(startRadius, targetRadius, t), 1, targetRadius);
			vignetteMat.SetFloat("_VignettePower", curRadius);
			yield return null;
		}
		for (float t = 0; curRadius < startRadius; t += Time.deltaTime)
		{
			curRadius = Mathf.Lerp(targetRadius, startRadius, t);
			vignetteMat.SetFloat("_VignettePower", curRadius);
			yield return null;
		}
		
	}

	public static class SpecialEffects
	{
		public static void VignetteEffect(float intensity) => instance.VignetteEffect(intensity);
	}
}
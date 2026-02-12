using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GenericLingeringVignette : MonoBehaviour
{
	[SerializeField, Min(0.05f)] private float lerpDuration = 0.5f;

	[SerializeField] public Material vignetteMaterial;
	private Coroutine vignetteTask;

	private int StrengthPropertyID = Shader.PropertyToID("_Strength");

	void Awake()
	{
		ApplyStrength(0);
	}

    public void SetStrength(float targetStrength)
	{
		if (vignetteTask != null)
			StopCoroutine(vignetteTask);
		vignetteTask = StartCoroutine(SmoothStrength(targetStrength));
	}

	private IEnumerator SmoothStrength(float targetStrength)
    {
		float startStrength = vignetteMaterial.GetFloat(StrengthPropertyID);
		float elapsed = 0f;
		while (elapsed < lerpDuration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / lerpDuration);
			float currentStrength = Mathf.Lerp(startStrength, targetStrength, t);
			vignetteMaterial.SetFloat(StrengthPropertyID, currentStrength);
			yield return null;
		}
		vignetteMaterial.SetFloat(StrengthPropertyID, targetStrength);
		vignetteTask = null;
    }

	public void ApplyStrength(float strength)
	{
		vignetteMaterial.SetFloat(StrengthPropertyID, strength);
	}
}

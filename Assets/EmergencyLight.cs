using System.Collections;
using UnityEngine;

public class EmergencyLight : MonoBehaviour
{

    [SerializeField] private Light light1;
    [SerializeField] private float light1Intensity = 0.6f;
    [SerializeField] private Light light2;
    [SerializeField] private float light2Intensity = 1f;
    [SerializeField] private float startDelay;
    [SerializeField] private float fadeInTime = 0.4f;
    [SerializeField] private float fadeOutTime = 2.5f;
    [SerializeField] private float fadeDelay = 0.05f;

    [SerializeField] private bool isFadingIn;
    [SerializeField] private float fadeInProgress = 0f;
    [SerializeField] private bool isFadingOut;
    [SerializeField] private float fadeOutProgress = 0f;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        light1 = GetComponent<Light>();
        light2 = transform.GetChild(0).GetComponent<Light>();
        light1.color = Color.red;
        light2.color = Color.red;
        light1.intensity = 0f;
        light2.intensity = 0f;
        StartCoroutine(LateStart());
    }

    private IEnumerator LateStart()
    {
        yield return new WaitForSeconds(startDelay);
        fadeInProgress = 0f;
        isFadingIn = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isFadingOut)
        {
            fadeOutProgress += Time.deltaTime;
            float fadeOutPercent = fadeOutProgress / fadeOutTime;

            if (fadeOutPercent > 1f)
            {
                isFadingOut = false;
                //isFadingIn = true;
                fadeOutProgress = 0f;
                light1.intensity = 0f;
                light2.intensity = 0f;
                StartCoroutine(FadeDelay(true));
            }

            light1.intensity = Mathf.Lerp(0f, light1Intensity, (1f - fadeOutPercent));
            light2.intensity = Mathf.Lerp(0f, light2Intensity, (1f - fadeOutPercent));
        }

        if (isFadingIn)
        {
            fadeInProgress += Time.deltaTime;
            float fadeInPercent = fadeInProgress / fadeInTime;

            if (fadeInPercent > 1f)
            {
                isFadingIn = false;
                //isFadingOut = true;
                fadeInProgress = 0f;
                light1.intensity = light1Intensity;
                light2.intensity = light2Intensity;
                StartCoroutine(FadeDelay(false));
            }

            light1.intensity = Mathf.Lerp(0f, light1Intensity, fadeInPercent);
            light2.intensity = Mathf.Lerp(0f, light2Intensity, fadeInPercent);
        }
    }

    private IEnumerator FadeDelay(bool fadeIn)
    {
        yield return new WaitForSeconds(fadeDelay);
        if (fadeIn) isFadingIn = true;
        else isFadingOut = true;
    }
}

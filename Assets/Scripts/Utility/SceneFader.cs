using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : Singleton<SceneFader>
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private bool isTransitioning;

    void Start()
    {
        SetAlpha(0f);
    }

    public void FadeToScene(string sceneName)
    {
        Debug.Log("Fading to scene " + sceneName);
        if (!isTransitioning)
            StartCoroutine(TransitionRoutine(sceneName));
    }

    IEnumerator TransitionRoutine(string sceneName)
    {
        isTransitioning = true;

        // Fade OUT (to black)
        yield return StartCoroutine(Fade(1f));

        // Load scene async
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        // Wait until almost loaded
        while (op.progress < 0.9f)
        {
            yield return null;
        }

        // Optional: small pause for polish
        yield return new WaitForSeconds(0.2f);

        // Activate scene
        op.allowSceneActivation = true;

        // Wait one frame so scene swaps cleanly
        yield return null;

        // Fade IN (from black)
        yield return StartCoroutine(Fade(0f));

        isTransitioning = false;
    }

    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeImage.canvasRenderer.GetAlpha();
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            float t = time / fadeDuration;

            // Smooth easing (ease-in-out)
            t = t * t * (3f - 2f * t);

            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            SetAlpha(alpha);

            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    void SetAlpha(float a)
    {
        fadeImage.canvasRenderer.SetAlpha(a);
    }
}
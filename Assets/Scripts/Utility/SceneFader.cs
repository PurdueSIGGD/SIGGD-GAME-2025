using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : Singleton<SceneFader>
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private bool isTransitioning;

    override protected void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SetAlpha(0f);
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Pressed K " + isTransitioning);
            if (!isTransitioning) {
                StartCoroutine(Fade(1f - fadeImage.color.a));
            }
        }
#endif
    }

    public void FadeToScene(string sceneName, Transform newPosition)
    {
        Debug.Log("Fading to scene " + sceneName);
        if (!isTransitioning)
            StartCoroutine(TransitionRoutine(sceneName, newPosition));
    }

    IEnumerator TransitionRoutine(string sceneName, Transform newPosition) {
        Vector3 position = newPosition.position;
        Quaternion rotation = newPosition.rotation;
        isTransitioning = true;

        Time.timeScale = 0f; // pause game

        // Fade out
        yield return StartCoroutine(Fade(1f));

        // Load scene async
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        // Wait until almost loaded
        while (op.progress < 0.9f)
        {
            yield return null;
        }

        // Activate scene
        op.allowSceneActivation = true;
        
        PlayerID.isReset = false;

        // wait until scene is actually done loading
        while (!op.isDone)
            yield return null;

        // Wait until Player is loaded
        while (!PlayerID.isReset)
        {
            yield return null;
        }
        
        // extra stabilization frames for UI rebuild
        yield return null;
        yield return null;

        // force UI to settle
        Canvas.ForceUpdateCanvases();

        // lock correct black state
        SetAlpha(1f);

        // Manually set player's transform for start position in new scene
        PlayerID.Instance.gameObject.transform.position = position;
        PlayerID.Instance.gameObject.transform.rotation = rotation;

        // Fade in
        yield return StartCoroutine(Fade(0f));

        Time.timeScale = 1f;

        isTransitioning = false;
    }

    IEnumerator Fade(float targetAlpha)
    {
        Debug.Log("Fading to " + targetAlpha);
        float startAlpha = fadeImage.color.a;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;

            float t = time / fadeDuration;

            // Smooth easing (ease-in-out)
            t = t * t * (3f - 2f * t);

            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            Debug.Log(alpha);
            SetAlpha(alpha);

            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    void SetAlpha(float a)
    {
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }
}
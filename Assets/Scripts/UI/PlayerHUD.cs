using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] CanvasGroup hudCanvasGroup;
    [SerializeField] float maxAlpha = 0.6f;

    public float hudFadeDuration = 1f;
    public float hudTimeout = 5f;
    private float lastTime = 0;
    private bool hudEnabled = false;

    private Coroutine fadeRoutine;


    void Awake()
    {
        ShowCanvas();
        if (SceneManager.GetActiveScene().name == "ShipScene")
        {
            hudCanvasGroup.alpha = 0f;
            hudEnabled = false;
        }
        else
        {
            hudCanvasGroup.alpha = maxAlpha;
            hudEnabled = true;
        }
        lastTime = Time.time;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U)) {
            if (hudEnabled) FadeOut();
            else FadeIn();
        }
        if (GameStateManager.Instance.getGameState() != GameStateManager.GameState.PEACEFUL) {
            TriggerHUDEvent();
        }
        if (Time.time - lastTime >= hudTimeout && hudEnabled) {
            FadeOut();
        }
    }

    public void ShowCanvas()
    {
        canvas.enabled = true;
    }

    public void HideCanvas()
    {
        canvas.enabled = false;
    }

    public void TriggerHUDEvent()
    {
        lastTime = Time.time;
        if (!hudEnabled) {
            FadeIn();
        }
    }

    public void FadeIn()
    {
        // Disable HUD in ship scene
        if (SceneManager.GetActiveScene().name == "ShipScene") return;
        StartFade(maxAlpha);
        hudEnabled = true;
    }

    public void FadeOut()
    {
        StartFade(0f);
        hudEnabled = false;
    }

    void StartFade(float target)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(Fade(target));
    }

    IEnumerator Fade(float target)
    {
        float time = 0f;
        float start = hudCanvasGroup.alpha;

        while (time < hudFadeDuration)
        {
            time += Time.deltaTime;
            hudCanvasGroup.alpha = Mathf.Lerp(start, target, time / hudFadeDuration);
            yield return null;
        }

        hudCanvasGroup.alpha = target;
    }
}

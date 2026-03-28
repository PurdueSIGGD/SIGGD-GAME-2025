using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] CanvasGroup hudCanvasGroup;

    public float hudFadeDuration = 1f;
    private bool hudEnabled = false;

    void Awake()
    {
        ShowCanvas();
        hudCanvasGroup.alpha = 1f;
        FadeOut();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U)) {
            if (hudEnabled) FadeOut();
            else FadeIn();
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

    public void FadeIn()
    {
        StartCoroutine(Fade(0f, 1f));
        hudEnabled = true;
    }

    public void FadeOut()
    {
        StartCoroutine(Fade(1f, 0f));
        hudEnabled = false;
    }

    IEnumerator Fade(float start, float end)
    {
        float time = 0f;

        while (time < hudFadeDuration)
        {
            time += Time.deltaTime;
            hudCanvasGroup.alpha = Mathf.Lerp(start, end, time / hudFadeDuration);
            yield return null;
        }

        hudCanvasGroup.alpha = end;
    }
}

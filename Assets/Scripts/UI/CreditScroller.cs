using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditScroller : MonoBehaviour
{
    [SerializeField] float scrollSpeed = 20f;
    [SerializeField] RectTransform rect;

    [SerializeField] private CanvasGroup skipButton;

    private bool skipShown = false;

    private void Awake()
    {
        skipButton.alpha = 0f;
    }

    private void Update()
    {
        if (skipShown && Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("Main Menu");
        }
        else if (Input.anyKeyDown)
        {
            ShowSkipButton();
        }

        rect.position = new Vector3(rect.position.x, rect.position.y + scrollSpeed * Time.deltaTime, rect.position.z);

        if (rect.position.y > 23500f)
        {
            SceneManager.LoadScene("Main Menu");
        }
    }

    private void ShowSkipButton()
    {
        skipShown = true;

        StartCoroutine(FadeInSkipButton(0.3f, 5f));
    }

    private IEnumerator FadeInSkipButton(float duration, float lastingDuration)
    {
        float elapsed = 0f;
        
        skipButton.alpha = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            skipButton.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        
        skipButton.alpha = 1f;
        
        yield return new WaitForSeconds(lastingDuration);
        
        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            skipButton.alpha = Mathf.Clamp01(1f - (elapsed / duration));
            yield return null;
        }
        
        skipShown = false;
        skipButton.alpha = 0f;
    }
}

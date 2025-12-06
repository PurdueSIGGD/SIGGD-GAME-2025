using System;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    public float alphaFadeSeconds = 1.0f;

    private RawImage img;
    // private RectTransform rectTransform;
    private float alphaSpeed;

    // private float getFitScale(Rect spriteRect)
    // {
    //     AspectRatioFitter
    // }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        alphaSpeed = 1.0f / alphaFadeSeconds;

        img = GetComponent<RawImage>();
        // rectTransform = GetComponent<RectTransform>();
        // rectTransform.localScale = 2.0f * Vector3.one;
        // rectTransform.pivot = Vector2.zero;
        // Debug.Log(img.sprite.bounds);
        // Debug.Log(img.sprite.rect);
    }

    // Update is called once per frame
    void Update()
    {
        // float move = 0.5f * Time.deltaTime;
        // rectTransform.pivot += move * Vector2.one;

        // if (rectTransform.pivot.x >= 1.0)
        // {
        //     rectTransform.pivot = Vector2.zero;
        // }

        Color imgColor = img.color;
        imgColor.a = Mathf.Clamp(imgColor.a + (alphaSpeed * Time.deltaTime), 0.0f, 1.0f);
        img.color = imgColor;
    }

    public void IncreaseOpacity()
    {
        // Debug.Log("AAAAAAAAA");
        alphaSpeed = Mathf.Abs(1.0f / alphaFadeSeconds);
    }
    public void DecreaseOpacity()
    {
        // Debug.Log("BBBBBBBBB");
        alphaSpeed = -Mathf.Abs(1.0f / alphaFadeSeconds);
    }
}

using UnityEngine;
using TMPro;

public class ExpandOnHover : MonoBehaviour
{
    public Vector2 expandDimensions = new Vector2(200,75);
    private Vector2 initialDimensions;
    private Canvas canvas;
    private float fontSizeIncrement = 4;
    private float originalFontSize;
    private TextMeshProUGUI tmpText;
    RectTransform rectTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        tmpText = GetComponentInChildren<TextMeshProUGUI>();
        originalFontSize = tmpText.fontSize;
        initialDimensions = rectTransform.sizeDelta;
        canvas = GetComponentInParent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePosition = Input.mousePosition;
        bool isHovering = RectTransformUtility.RectangleContainsScreenPoint(
            rectTransform,
            mousePosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera);
        if (isHovering)
        {
            Expand();
        } else
        {
            Contract();
        }
                
    }
    public void Expand()
    {
        rectTransform.sizeDelta = expandDimensions;
        tmpText.fontSize = originalFontSize + fontSizeIncrement;

    }
    public void Contract()
    {
        rectTransform.sizeDelta = initialDimensions;
        tmpText.fontSize = originalFontSize;
    }
}

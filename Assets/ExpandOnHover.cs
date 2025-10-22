using UnityEngine;

public class ExpandOnHover : MonoBehaviour
{
    public Vector2 expandDimensions = new Vector2(200,75);
    private Vector2 initialDimensions;
    private Canvas canvas;
    RectTransform rectTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
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
    }
    public void Contract()
    {
        rectTransform.sizeDelta = initialDimensions;
    }
}

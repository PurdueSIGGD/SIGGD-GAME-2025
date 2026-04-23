using UnityEngine;

/// <summary>
/// Placed on the selection border indicator object.
/// HotbarHUD moves this object's RectTransform to sit on the currently selected slot.
/// </summary>
public class HotbarSelectionIndicator : MonoBehaviour
{
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Snaps the indicator to the given world-space anchored position.
    /// </summary>
    public void MoveTo(Vector2 anchoredPosition)
    {
        rectTransform.anchoredPosition = anchoredPosition;
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}


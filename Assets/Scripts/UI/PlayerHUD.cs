using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] Canvas canvas;

    void Awake()
    {
        ShowCanvas();
    }

    public void ShowCanvas()
    {
        canvas.enabled = true;
    }

    public void HideCanvas()
    {
        canvas.enabled = false;
    }
}

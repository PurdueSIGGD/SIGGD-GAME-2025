using UnityEngine;

public class AudioLogMenu : Singleton<AudioLogMenu>
{
    private Canvas audioLogMenuCanvas;

    protected override void Awake()
    {
        base.Awake();

        audioLogMenuCanvas = GetComponentInChildren<Canvas>();
        audioLogMenuCanvas.enabled = false;
    }

    public void ShowAudioLogMenu(bool enabled)
    {
        audioLogMenuCanvas.enabled = enabled;
    }
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

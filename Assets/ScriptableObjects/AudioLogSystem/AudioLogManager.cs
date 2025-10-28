using UnityEngine;
using TMPro;

public class AudioLogManager : MonoBehaviour
{
    public TextMeshProUGUI displayText;
    public TextMeshProUGUI subtitleText;
    [SerializeField] AudioLogObject mine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // toggle visibility of subtitle and send a debug message
    public void TestToggle()
    {
        if (displayText.enabled == true)
        {
            displayText.enabled = false;
            Debug.Log("subtitle hidden");
        }
        else
        {
            displayText.enabled = true;
            Debug.Log("subtitle showed");
        }
    }

    public void PlaySubtitle(string text)
    {
        subtitleText.text = text;
        Debug.Log(text + "\nsubtitle played");
    }
}

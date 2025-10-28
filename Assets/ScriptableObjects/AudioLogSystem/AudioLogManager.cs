using UnityEngine;
using TMPro;

public class AudioLogManager : MonoBehaviour
{
    public TextMeshProUGUI displayText;
    [SerializeField] AudioLogObject mine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleText()
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
}

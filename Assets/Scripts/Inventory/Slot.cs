using TMPro;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public ItemInfo itemInfo = null;
    public int count = 0;
    [HideInInspector] public int index;

    private TextMeshProUGUI textDisplay;

    void Awake()
    {
        textDisplay = GetComponentInChildren<TextMeshProUGUI>();
    }

    /// <summary>
    /// Updates text to match backend inventory slot
    /// </summary>
    /// <param name="slot">Backend inventory slot connected to this button slot</param>
    public void UpdateText()
    {
        if (itemInfo)
        {
            if (!textDisplay) textDisplay = GetComponentInChildren<TextMeshProUGUI>(); // double check
            textDisplay.text = itemInfo.name + "(" + count + ")";
        }
        else {
            textDisplay.text = "empty";
        }
    }
}

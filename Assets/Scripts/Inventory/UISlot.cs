using TMPro;
using UnityEngine;

public class UISlot : MonoBehaviour
{
    public ItemInfo itemInfo;
    public int count = 0;
    [HideInInspector] public int index;

    private TextMeshProUGUI textDisplay;

    void Awake()
    {
        textDisplay = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void UpdateSlot(UISlot uiSlot)
    {
        itemInfo = uiSlot.itemInfo;
        count = uiSlot.count;
        if (itemInfo)
        {
            if (!textDisplay) textDisplay = GetComponentInChildren<TextMeshProUGUI>(); // double check
            textDisplay.text = itemInfo.name;
        }
    }
}

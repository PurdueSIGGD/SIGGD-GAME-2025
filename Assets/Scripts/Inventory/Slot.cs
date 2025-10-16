using TMPro;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public ItemInfo itemInfo;
    public int count = 0;
    [HideInInspector] public int index;

    private TextMeshProUGUI textDisplay;

    void Awake()
    {
        textDisplay = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void UpdateSlot(Slot slot)
    {
        itemInfo = slot.itemInfo;
        count = slot.count;
        if (itemInfo)
        {
            if (!textDisplay) textDisplay = GetComponentInChildren<TextMeshProUGUI>(); // double check
            textDisplay.text = itemInfo.name;
        }
    }
}

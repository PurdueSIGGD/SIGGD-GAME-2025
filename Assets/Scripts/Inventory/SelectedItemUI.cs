using TMPro;
using UnityEngine;

public class SelectedItemUI : MonoBehaviour
{

    public ItemInfo itemInfo = null;
    public int count = 0;
    [HideInInspector] public int index;

    private TextMeshProUGUI textDisplay;

    void Awake()
    {
        textDisplay = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        UpdateSelectedSlot();
    }

    public void UpdateSelectedSlot()
    {
        itemInfo = Inventory.Instance.GetSelectedItem();
        count = Inventory.Instance.GetSelectedItemCount();
        if (itemInfo)
        {
            if (!textDisplay) textDisplay = GetComponentInChildren<TextMeshProUGUI>(); // double check
            textDisplay.text = itemInfo.name + "(" + count + ")";
        }
        else
        {
            textDisplay.text = "empty";
        }
    }
}

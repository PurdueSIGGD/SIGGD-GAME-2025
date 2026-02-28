using TMPro;
using UnityEngine;

public class UISlot : MonoBehaviour
{
    public ItemInfo itemInfo = null;
    
    public int count = 0;
    [HideInInspector] public int index;

    private TextMeshProUGUI textDisplay;

    void Awake()
    {
        textDisplay = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Start()
    {
        // Inventory will handle all of the initialization for UISlots to avoid overwriting slots with Empty

        //Debug.Log(RecipeInfo.Instance == null ? "recipeinfo null" : "recipeinfo not null");
        //Debug.Log((itemInfo == null ? "iteminfo is before" : "ItemInfo is not null before") + " " + index);
        //itemInfo = RecipeInfo.Instance.NamesToItemInfos[ItemInfo.ItemName.Empty];
        //Debug.Log(itemInfo == null ? "iteminfo is null" : "ItemInfo is not null now");
    }

    public void UpdateSlot()
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

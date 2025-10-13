using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class hotbar : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField]
    private Button[] Slots = new Button[3];

    [SerializeField]
    private Button[] InvSlots = new Button[9];

    // For debug testing crafting.
    private List<ItemInfo> lastClickedItems = new();

    void Start()
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            var buttonIndex = i;
            var button = Slots[buttonIndex].GetComponent<Button>();
            button.onClick.AddListener(() => OnSlotSelected(buttonIndex));
        }

        Debug.Log("Slot and their contents:");
        // For debug testing crafting.
        for (int i = 0; i < InvSlots.Length; i++)
        {
            var buttonIndex = i;
            var button = InvSlots[buttonIndex].GetComponent<Button>();
            button.onClick.AddListener(() => DebugOnInvSlotSelected(InvSlots[buttonIndex].GetComponent<InventorySlot>()));
            Debug.Log("Slot " + buttonIndex + " has " + InvSlots[buttonIndex].GetComponent<InventorySlot>().ItemInfo.itemName);
        }
    }

    void DebugOnInvSlotSelected(InventorySlot slot)
    {
        ItemInfo item = slot.ItemInfo;
        lastClickedItems.Add(item);
        if (lastClickedItems.Count >= 2)
        {
            var combined = RecipeInfo.Get().UseRecipe(lastClickedItems[0].itemName, lastClickedItems[1].itemName);
            if (combined != null)
            {
                Debug.Log("Combining " + lastClickedItems[0].itemName + " and " + lastClickedItems[1].itemName);
                combined.log();
                lastClickedItems.Clear();
            }
        }
    }

    void OnSlotSelected(int slotIndex)
    {
        Debug.Log("slot number " + slotIndex + " was chosen");
    }

    // Update is called once per frame
    void Update()
    {

    }
}

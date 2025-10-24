using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    const int HotBarLength = 9;
    const int InventoryLength = 18;

    [Header("Add Slot.cs to these if you like to add an item in edtior")]
    [SerializeField] private Button[] hotbarSlots = new Button[HotBarLength];
    [SerializeField] private Button[] inventorySlots = new Button[InventoryLength];

    private List<ItemInfo> lastClickedItems = new();

    private Slot[] inventory; // array (or 2D-array) for entire inventory; first 9 indices are the hotbar
    private Canvas inventoryCanvas;
    private int selected; // index of selected item in hotbar
    private Slot tempSlot; // temporary slot for holding item that is being moved

    void Awake()
    {
        inventory = new Slot[HotBarLength + InventoryLength];
        inventoryCanvas = GetComponentInChildren<Canvas>();
        inventoryCanvas.enabled = false;
    }

    void Start()
    {
        for (int i = 0; i < HotBarLength; i++)
        {
            // Right now there aren't 9 buttons on the ui menu so we skip everything that's null
            if (hotbarSlots[i] == null) continue;

            // TODO: Add to the existing code below to load in saved items here 
            if (!hotbarSlots[i].TryGetComponent<Slot>(out Slot slot))
            {
                slot = hotbarSlots[i].AddComponent<Slot>();
            }
            slot.index = i;
            SetHotbarSlot(slot.index, slot);

            hotbarSlots[i].onClick.AddListener(() => OnSlotSelected(slot));
        }

        for (int i = 0; i < InventoryLength; i++)
        {
            // Right now there aren't 9 buttons on the ui menu so we skip everything that's null
            if (inventorySlots[i] == null) continue;

            // TODO: Add to the existing code below to load in saved items here 
            if (!inventorySlots[i].TryGetComponent<Slot>(out Slot slot))
            {
                slot = inventorySlots[i].AddComponent<Slot>();
            }
            slot.index = i + HotBarLength;
            SetInventorySlot(slot.index, slot);

            inventorySlots[i].onClick.AddListener(() => DebugOnInvSlotSelected(slot));
        }
    }
    
    // TODO: replace
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ShowInventory(!inventoryCanvas.enabled);
        }
    }

    /// <summary>
    /// Enable the inventory ui, disable player movment, and show cursor
    /// </summary>
    public void ShowInventory(bool enabled)
    {
        inventoryCanvas.enabled = enabled;

        // Inventory ui is still not responsive

        // Added: disable player movement and show cursor.
        if (enabled) Cursor.lockState = CursorLockMode.None;
        else Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = enabled;
        PlayerInput.Instance.DebugToggleInput(enabled);
        Debug.LogWarning("Opening player inventory!");
    }

    private Slot GetHotbarSlot(int index)
    {
        if (index >= 9 || index < 0)
        {
            Debug.LogWarning("Hotbar size 9, index " + index);
            return null;
        }
        return inventory[index];
    }

    private void SetHotbarSlot(int index, Slot slot)
    {
        if (index >= 9 || index < 0)
        {
            Debug.LogWarning("Hotbar size 9, index " + index);
            return;
        }
        if (inventory[index] == null)
        {
            Debug.Log("null");
            inventory[index] = slot;
        }
        inventory[index].UpdateSlot(slot);
    }

    private Slot GetInventorySlot(int index)
    {
        if (index < 9)
        {
            Debug.LogWarning("Inventory size 16, index = " + index);
            return null;
        }
        return inventory[index];
    }

    private void SetInventorySlot(int index, Slot slot)
    {
        if (index < 9)
        {
            Debug.LogWarning("Inventory size 16, index + " + index);
            return;
        }
        if (inventory[index] == null)
        {
            inventory[index] = slot;
        }
        inventory[index].UpdateSlot(slot);
    }

    void OnSlotSelected(Slot uiSlot)
    {
        Debug.Log("Hotbar slot #" + uiSlot.index + " clicked");
    }

    // This method shows recipe crafting, but is considered "debug" because it won't work this way in a playable build.
    void DebugOnInvSlotSelected(Slot slot)
    {
        ItemInfo item = slot.itemInfo;
        lastClickedItems.Add(item);
        if (lastClickedItems.Count >= 2)
        {
            var recipeInfo = RecipeInfo.Get();
            Debug.Log(recipeInfo == null ? "null recipeInfo" : "recipeInfo NOT null");

            var a = lastClickedItems[^2].itemName; 
            var b = lastClickedItems[^1].itemName;

            var combined = recipeInfo.UseRecipe(lastClickedItems[^2].itemName, lastClickedItems[^1].itemName);
            // If there is no valid recipe, null is returned.
            if (combined != null)
            {
                Debug.Log("Combining " + lastClickedItems[^2].itemName + " and " + lastClickedItems[^1].itemName);
                combined.log();
                lastClickedItems.Clear();
            }
            else Debug.Log("combined is null for inputs " + a + " " + b);
        }
    }

    /// <summary>
    /// Switches the selected item (limited to hotbar)
    /// </summary>
    /// <param name="index">Index to switch to</param>
    public void select(int index) {
        selected = index;
        Debug.Log("Selected " + index + " index, containing " + inventory[index].itemInfo.itemName);
    }

    /// <summary>
    /// Searches for item in inventory and returns the index
    /// </summary>
    /// <param name="itemName">Name of the item</param>
    /// <returns>Index of the item or -1 if not found</returns>
    public int find(ItemInfo.ItemName itemName) {
        for (int i = 0; i < inventory.Length; i++) {
            if (inventory[i].count > 0 && inventory[i].itemInfo.itemName == itemName)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Adds item to inventory
    /// </summary>
    /// <param name="itemInfo">Item to add</param>
    /// <param name="count">Amount of items</param>
    /// 
    /// <returns>Number of items that could not be added to the inventory</returns>
    public int add(ItemInfo itemInfo, int count) { // maybe change input type
        // first add to existing stacks
        for (int i = 0; i < inventory.Length; i++) {
            if (inventory[i].count > 0 && inventory[i].itemInfo.itemName == itemInfo.itemName) // matches item
            {
                if (itemInfo.maxStackCount > inventory[i].count) { // has space for at least one item
                    if (itemInfo.maxStackCount < inventory[i].count + count) // not enough space for all items in same stack
                    {
                        count -= itemInfo.maxStackCount - inventory[i].count;
                        inventory[i].count = itemInfo.maxStackCount;
                    }
                    else { // has enough space for all items in same stack
                        inventory[i].count += count;
                        count = 0;
                    }
                    Debug.Log("Added " + itemInfo.itemName + " to existing stack at index " + i + ". Current count is " + inventory[i].count);
                    if (count <= 0) return 0;
                }
                
            }
        }
        // otherwise create new stack if possible
        if (count > 0) {
            for (int i = 0; i < inventory.Length; i++) {
                if (inventory[i].count == 0) { // is empty slot
                    if (count > itemInfo.maxStackCount) // will need to split the items between slots
                    {
                        count -= itemInfo.maxStackCount;
                        inventory[i].itemInfo = itemInfo;
                        inventory[i].count = itemInfo.maxStackCount;
                    }
                    else { // has enough space for all items in same stack
                        inventory[i].itemInfo = itemInfo;
                        inventory[i].count += count;
                        count = 0;
                    }
                    Debug.Log("Added " + itemInfo.itemName + " to new stack at index " + i + ". Current count is " + inventory[i].count);
                    if (count <= 0) return 0;
                }
            }
        }
        return count; // leftover items that could not be added
        // otherwise replace current selected item

    }

    /// <summary>
    /// Drop item at index
    /// </summary>
    /// <param name="index">Index of item to drop</param>
    public void drop(int index) { // maybe create another method for dropping stacks of items
        // instantiate physical item
        

        // remove item
        inventory[index].count--;
        if (inventory[index].count <= 0) {
            inventory[index].itemInfo = null;
        }
    }

    /// <summary>
    /// Drop selected item
    /// </summary>
    public void drop() {
        drop(selected);
    }

    /// <summary>
    /// Swaps item in tempSlot with chosen item
    /// </summary>
    /// <param name="index">Index of item to be moved</param>
    public void move(int index) {
        Slot temp = inventory[index];
        inventory[index] = tempSlot;
        tempSlot = temp;
    }

    /// <summary>
    /// Print out a string representation of player's inventory in console
    /// </summary>
    public void PrintInventory() {
        string s = "{";
        for (int i = 0; i < inventory.Length; i++) {
            if (i % 9 == 0) s += "\n";
            if (inventory[i].count > 0) s += "[" + inventory[i].itemInfo.itemName + ", " + inventory[i].count + "]  ";
            else s += "[empty]  ";
        }
        s += "\n}";
        Debug.Log(s);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index">Index to check</param>
    /// <returns>Whether or not the slot is empty</returns>
    public bool isEmpty(int index) {
        return inventory[index].count == 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Whether or not an item is being moved (stored in tempSlot)</returns>
    public bool isMovingItem() {
        return tempSlot.count > 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>The selected item</returns>
    public ItemInfo getSelectedItem() { // maybe change return type
        return inventory[selected].itemInfo;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns>The </returns>
    public ItemInfo getItem(int index) { // maybe change return type;
        return inventory[index].itemInfo;
    }

    public Slot[] getInventory() {
        return inventory;
    }
}

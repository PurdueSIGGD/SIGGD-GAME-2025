using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;

public class Inventory : Singleton<Inventory>, IInventory
{
    public const int HotBarLength = 3;
    public const int InventoryLength = 9;

    [Header("Add Slot.cs to these if you like to add an item in edtior")]
    [SerializeField] private Button[] hotbarSlots = new Button[HotBarLength]; // hotbar buttons
    [SerializeField] private Button[] inventorySlots = new Button[InventoryLength]; // inventory buttons

    private List<ItemInfo> lastClickedItems = new();

    private UISlot[] inventory; // array (or 2D-array) for entire inventory; first 9 indices are the hotbar
    // [SerializeField] public ItemInfo[] itemInfos = new ItemInfo[7]; // array of all of the different types of item infos; used for loading from file

    private Dictionary<string, ItemInfo> itemInfos;

    [SerializeField] public string[] itemNames = new string[7];
    private Canvas inventoryCanvas;
    private int selected; // index of selected item in hotbar
    private int swapSelection = -1; // index of select item for swapping
    private InventoryInputActions inputActions;

    protected override void Awake()
    {
        base.Awake();
        inventory = new UISlot[HotBarLength + InventoryLength];
        
        inventoryCanvas = GetComponentInChildren<Canvas>();
        inventoryCanvas.enabled = false;

        inputActions = new InventoryInputActions();
    }

    void OnEnable()
    {
        inputActions.InventorySelection.Enable();
        inputActions.InventorySelection.Scroll.performed += OnScroll;
        inputActions.InventorySelection.NumberKeys.performed += OnNumberKeyInput;
    }

    void OnDisable()
    {
        inputActions.InventorySelection.Disable();
        inputActions.InventorySelection.Scroll.performed -= OnScroll;
        inputActions.InventorySelection.NumberKeys.performed -= OnNumberKeyInput;
    }

    private void OnScroll(InputAction.CallbackContext context)
    {
        if (ObjectPlacer.Instance.InPlacementMode) return;
        float scrollValue = context.ReadValue<float>();
        if (scrollValue == 0) return;
        int index = (selected + (int)(scrollValue)) % HotBarLength;
        if (index < 0) {
            index = HotBarLength - 1;
        }
        Select(index);
    }

    private void OnNumberKeyInput(InputAction.CallbackContext context) {
        if (ObjectPlacer.Instance.InPlacementMode) return;
        float value = context.ReadValue<float>();
        int index = (int)(value) - 1;
        if (index >= HotBarLength) return; // since hotbar is only length 3 right now
        Select(index);
    }

    void Start()
    {

        // Update inventory to match manually placed items in scene/saved items
        // get UI slots from scene
        for (int i = 0; i < HotBarLength; i++)
        {
            // Right now there aren't 9 buttons on the ui menu so we skip everything that's null
            if (hotbarSlots[i] == null) continue;

            if (!hotbarSlots[i].TryGetComponent<UISlot>(out UISlot slot))
            {
                slot = hotbarSlots[i].AddComponent<UISlot>();
            }
            inventory[i] = slot;
            //SetHotbarSlot(slot.index, slot);

            hotbarSlots[i].onClick.AddListener(() => OnSlotSelected(slot));
        }

        for (int i = 0; i < InventoryLength; i++)
        {
            // Right now there aren't 9 buttons on the ui menu so we skip everything that's null
            if (inventorySlots[i] == null) continue;

            if (!inventorySlots[i].TryGetComponent<UISlot>(out UISlot slot))
            {
                slot = inventorySlots[i].AddComponent<UISlot>();
            }
            // Check if slot has a non-null slot.iteminfo
            Debug.Log(slot.GetComponent<UISlot>().itemInfo == null ? "inv slot has null iteminfo!" : "inv slot good");
            inventory[i + HotBarLength] = slot;
            //SetInventorySlot(slot.index, slot);

            inventorySlots[i].onClick.AddListener(() => DebugOnInvSlotSelected(slot));
        }

        itemInfos = new();
        foreach (var entry in RecipeInfo.Instance.NamesToItemInfos) {
            itemInfos[entry.Key.ToString()] = entry.Value;
        }

        // Load inventory info from save
        if (InventoryDataSaveModule.inventoryData.inventory != null) // load from save data
        {
            Debug.Log("Initializing inventory from save");
            selected = InventoryDataSaveModule.inventoryData.selected;
            string name;
            for (int i = 0; i < InventoryDataSaveModule.inventoryData.inventory.Length; i++)
            {
                inventory[i].index = i;
                inventory[i].count = InventoryDataSaveModule.inventoryData.inventory[i].count;
                name = InventoryDataSaveModule.inventoryData.inventory[i].name;
                if (inventory[i].count != 0)
                {
                    inventory[i].itemInfo = itemInfos[name];
                    /*
                    // make iteminfo based on name
                    for (int j = 0; j < itemInfos.Length; j++)
                    {
                        if (itemNames[j].Equals(name))
                        {
                            inventory[i].itemInfo = itemInfos[j];
                            break;
                        }
                    }
                    */
                }
                else
                {
                    inventory[i].itemInfo = itemInfos[ItemInfo.ItemName.Empty.ToString()];
                    // make it an empty iteminfo
                    // inventory[i].itemInfo = itemInfos[0];
                }
                inventory[i].UpdateSlot();
            }
        }
        else // initialize empty
        {
            selected = 0;
            Debug.Log("Initializing empty inventory");
            for (int i = 0; i < inventory.Length; i++)
            {
                inventory[i].index = i;
                inventory[i].count = 0;
                inventory[i].itemInfo = itemInfos[ItemInfo.ItemName.Empty.ToString()];
                inventory[i].UpdateSlot();
            }
        }
        PrintInventory();

        for (int i = 0; i < InventoryLength; i++)
        {
            // Right now there aren't 9 buttons on the ui menu so we skip everything that's null
            if (inventorySlots[i] == null) continue;

            var uiSlot = inventorySlots[i].GetComponent<UISlot>();
            // Check if slot has a non-null slot.iteminfo
            Debug.Log(uiSlot.itemInfo == null ? "inv slot has null iteminfo!" : "inv slot good");
        }
    }

    /// <summary>
    /// Enable the inventory ui, disable player movment, and show cursor
    /// </summary>
    public void ShowInventory(bool enabled)
    {
        inventoryCanvas.enabled = enabled;
        if (swapSelection != -1) {
            // Reset the color of buttons if necessary
            inventory[swapSelection].SetColor(Color.white);
            if (inventory[swapSelection].itemInfo && inventory[swapSelection].itemInfo.isIngredient) {
                for (int i = 0; i < HotBarLength; i++)
                {
                    inventory[i].SetColor(Color.white);
                }
            }
            swapSelection = -1;
        }
    }

    public bool isEnabled() {
        return inventoryCanvas.enabled;
    }
    void OnSlotSelected(UISlot uiUISlot)
    {
        Debug.Log("Hotbar slot #" + uiUISlot.index + " clicked");
    }

    // This method shows recipe crafting, but is considered "debug" because it won't work this way in a playable build.
    void DebugOnInvSlotSelected(UISlot uiSlot)
    {
        ItemInfo item = uiSlot.itemInfo;
        if (item == null)
        {
            Debug.Log("why is ui slot iteminfo still null?");
        }
        lastClickedItems.Add(item);
        if (lastClickedItems.Count >= 2)
        {
            var recipeInfo = RecipeInfo.Instance;
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
    public void Select(int index) {
        selected = index;
        if (!inventory[index] || inventory[index].count == 0 || !inventory[index].itemInfo)
        {
            Debug.Log("Selected index " + index + ", which is empty");
        }
        else {
            Debug.Log("Selected index " + index + ", containing " + inventory[index].count + " " + inventory[index].itemInfo.itemName + "s");
        } 
    }

    public void Decrement()
    {
        inventory[selected].count--;
        Debug.Log("Used " + inventory[selected].itemInfo.itemName + ", " + inventory[selected].count + " remaining");
        if (inventory[selected].count == 0) {
            // inventory[selected].itemInfo = itemInfos[0];
            inventory[selected].itemInfo = itemInfos[ItemInfo.ItemName.Empty.ToString()];
        }
        inventory[selected].UpdateSlot();
    }

    /// <summary>
    /// Determines if the inventory contains a certain number of an item
    /// </summary>
    /// <param name="itemName">Name of the item</param>
    /// <parm name="count">The number of items to check that the inventory has</parm>
    /// <returns>Whether or not the inventory contains enough of the item</returns>
    public bool Contains(ItemInfo.ItemName itemName, int count) {
        int found = 0;
        for (int i = 0; i < inventory.Length; i++) {
            if (inventory[i]?.count > 0 && inventory[i].itemInfo.itemName == itemName)
            {
                found += inventory[i].count;
                if (found >= count) {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Crafts an item by removing the ingredients from the inventory
    /// and adding the crafted item.
    /// </summary>
    /// <param name="recipe">Recipe to craft</param>
    public void Craft(Recipe recipe) {
        // remove the necessary amount of both items from the inventory
        int amountToRemove = 0;
        for (int ingredients = 0; ingredients < recipe.counts.Count; ingredients++)
        {
            amountToRemove = recipe.counts[ingredients];
            for (int i = 0; i < inventory.Length; i++)
            {
                if (amountToRemove > 0 && inventory[i]?.count > 0 && inventory[i].itemInfo.itemName == recipe.ingredients[ingredients].itemName)
                {
                    if (inventory[i].count <= amountToRemove) // remove entire stack
                    {
                        amountToRemove -= inventory[i].count;
                        inventory[i].count = 0;
                        inventory[i].itemInfo = itemInfos[ItemInfo.ItemName.Empty.ToString()];
                        inventory[i].UpdateSlot();
                    }
                    else
                    { // remove the rest from this stack
                        inventory[i].count -= amountToRemove;
                        amountToRemove = 0;
                        inventory[i].UpdateSlot();
                    }
                    if (amountToRemove == 0) {
                        break;
                    }
                }
            }
        }
        // add crafted item
        AddItem(recipe.output, 1);
    }

    /// <summary>
    /// Adds item to inventory
    /// </summary>
    /// <param name="itemInfo">Item to add</param>
    /// <param name="count">Amount of items</param>
    /// 
    /// <returns>Number of items that could not be added to the inventory</returns>
    public int AddItem(ItemInfo itemInfo, int count) { // maybe change input type
        // first add to existing stacks
        for (int i = 0; i < inventory.Length; i++) {
            if (itemInfo.isIngredient && i < HotBarLength) { // ingredients are not allowed in the hotbar
                continue;
            }
            if (inventory[i]?.count > 0 && inventory[i].itemInfo.itemName == itemInfo.itemName) // matches item
            {
                if (itemInfo.maxStackCount > inventory[i].count) { // has space for at least one item
                    if (itemInfo.maxStackCount < inventory[i].count + count) // not enough space for all items in same stack
                    {
                        count -= itemInfo.maxStackCount - inventory[i].count;
                        inventory[i].count = itemInfo.maxStackCount;
                        inventory[i].UpdateSlot(); // update UI
                    }
                    else { // has enough space for all items in same stack
                        inventory[i].count += count;
                        inventory[i].UpdateSlot(); // update UI
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
                if (itemInfo.isIngredient && i < HotBarLength)
                { // ingredients are not allowed in the hotbar
                    continue;
                }
                if (inventory[i]?.count == 0) { // is empty slot
                    if (count > itemInfo.maxStackCount) // will need to split the items between slots
                    {
                        count -= itemInfo.maxStackCount;
                        inventory[i].itemInfo = itemInfo;
                        inventory[i].count = itemInfo.maxStackCount;
                        inventory[i].UpdateSlot(); // update UI
                    }
                    else { // has enough space for all items in same stack
                        inventory[i].itemInfo = itemInfo;
                        inventory[i].count += count;
                        inventory[i].UpdateSlot(); // update UI
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

    /**
     * <summary>
     * Removes an item from the inventory. Returns true if the item was successfully removed, false otherwise.
     * </summary>
     *
     * <param name="item">The item to remove.</param>
     * <param name="count">The number of items to remove.</param>
     *
     * <returns>True if the item was successfully removed, false otherwise.</returns>
     */
    public bool RemoveItem(ItemInfo item, int count)
    {
        for (int i = 0; i < inventory.Length; i++) { // take into account removing across multiple stacks
            if (inventory[i].itemInfo.itemName == item.itemName) {
                if (inventory[i].count >= count)
                { // has enough in this stack; remove from this stack and stop looping
                    inventory[i].count -= count;
                    if (inventory[i].count == 0)
                    { // check for empty slot
                        inventory[i].itemInfo = itemInfos[ItemInfo.ItemName.Empty.ToString()];
                    }
                    inventory[i].UpdateSlot();
                    return true; // done removing
                }
                else { // not enough in this stack; remove entire stack and keep looping
                    count -= inventory[i].count; // reduce the number of items left that need to be removed
                    inventory[i].count = 0; // make the slot empty
                    inventory[i].itemInfo = itemInfos[ItemInfo.ItemName.Empty.ToString()];
                    inventory[i].UpdateSlot();
                }
            }
        }
        return false;
    }

    public void SwapSelect(int index) {
        if (swapSelection == -1)
        { // no item selected for swapping
            swapSelection = index;
            inventory[swapSelection].SetColor(Color.green);
            if (inventory[swapSelection].itemInfo && inventory[swapSelection].itemInfo.isIngredient) {
                // Make hotbar slots red to indicate that player cannot swap ingredient to hotbar
                for (int i = 0; i < HotBarLength; i++) {
                    inventory[i].SetColor(Color.red);
                }
            }
            Debug.Log("Swap selected " + index);
        }
        else 
        {
            if (swapSelection == index)
            { // deselect
                inventory[swapSelection].SetColor(Color.white);
                swapSelection = -1;
                Debug.Log("Deselected " + index);
            }
            else 
            {
                if (inventory[swapSelection].itemInfo.isIngredient && index < HotBarLength) {
                    Debug.Log("Cannot swap ingredient to hotbar");
                    return; // cannot swap ingredient into hotbar
                }
                if (inventory[index].itemInfo && inventory[swapSelection].itemInfo &&
                    inventory[index].itemInfo.itemName == inventory[swapSelection].itemInfo.itemName)
                {   // try to stack on index if they are the same item
                    if (inventory[index].count + inventory[swapSelection].count <= inventory[index].itemInfo.maxStackCount)
                    {   // move all from swapSelection into index
                        inventory[index].count += inventory[swapSelection].count;
                        inventory[swapSelection].count = 0;
                    }
                    else 
                    {   // stack as much as possible
                        int moveAmount = inventory[index].itemInfo.maxStackCount - inventory[index].count;
                        inventory[index].count += moveAmount;
                        inventory[swapSelection].count -= moveAmount;
                    }
                    Debug.Log("Stacked " + swapSelection + " onto " + index);
                }
                else
                {   // normal swapping
                    int tempCount = inventory[index].count;
                    ItemInfo tempItemInfo = inventory[index].itemInfo;
                    inventory[index].count = inventory[swapSelection].count;
                    inventory[index].itemInfo = inventory[swapSelection].itemInfo;
                    inventory[swapSelection].count = tempCount;
                    inventory[swapSelection].itemInfo = tempItemInfo;
                    Debug.Log("Swapped " + index + " and " + swapSelection);
                }

                inventory[index].SetColor(Color.white);
                inventory[swapSelection].SetColor(Color.white);
                inventory[index].UpdateSlot();
                inventory[swapSelection].UpdateSlot();

                for (int i = 0; i < HotBarLength; i++)
                {
                    inventory[i].SetColor(Color.white);
                }
                swapSelection = -1;
            }
        }
    }
    public void RemoveInventory()
    {
        //UISlot[] copy = new UISlot[inventory.Length];
        //Array.Copy(inventory, copy, inventory.Length);
        for (int i = 0; i < inventory.Length; i++)
        {
            //copy[i].count = inventory[i].count;
            //copy[i].itemInfo = inventory[i].itemInfo;
            inventory[i].count = 0;
            inventory[i].itemInfo = itemInfos[ItemInfo.ItemName.Empty.ToString()];
            inventory[i].UpdateSlot();
        }
        //return copy;
    }

    public void SetInventory(ItemInfo[] finfo, int[] fcount)
    {
        Debug.Log(inventory.Length + " length");
        //Array.Copy(newInv, inventory, newInv.Length);
        
        for (int i = 0; i < finfo.Length; i++)
        {
            inventory[i].count = fcount[i];
            inventory[i].itemInfo = finfo[i];
            inventory[i].UpdateSlot();
        }
        Debug.Log(inventory.Length + " new length");
    }

    /// <summary>
    /// Drop item at index
    /// </summary>
    /// <param name="index">Index of item to drop</param>
    /// <returns>Whether or not the drop was successful</returns>
    public bool Drop(int index) { // maybe create another method for dropping stacks of items
        // instantiate physical item
        

        // remove item
        inventory[index].count--;
        if (inventory[index].count <= 0) {
            inventory[index].itemInfo = itemInfos[ItemInfo.ItemName.Empty.ToString()];
        }
        
        return true;
    }

    /// <summary>
    /// Drop selected item
    /// </summary>
    public void Drop() {
        Drop(selected);
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
    public bool IsEmpty(int index) {
        return inventory[index].count == 0;
    }
    public bool IsInventoryEmpty()
    {
        bool isEmpty = true;
        for (int i = 0; i < inventory.Length; i++)
        {
            if (!IsEmpty(i))
            {
                isEmpty = false;
            }
        }
        return isEmpty;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>The selected item</returns>
    public ItemInfo GetSelectedItem() { // maybe change return type
        return inventory[selected] ? inventory[selected].itemInfo : null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>The selected item's count</returns>
    public int GetSelectedItemCount()
    {
        return inventory[selected] ? inventory[selected].count : 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>The index of the selected item (for saving)</returns>
    public int GetSelected()
    {
        return selected;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns>The </returns>
    public ItemInfo GetItem(int index) { // maybe change return type;
        return inventory[index].itemInfo;
    }

    public UISlot[] GetInventory()
    {
        return inventory;
    }
}

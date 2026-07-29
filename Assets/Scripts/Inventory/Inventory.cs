using System;
using System.Collections.Generic;
using SIGGD.Save;
using SIGGD.Save.Modules;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;

public class Inventory : Singleton<Inventory>, IInventory
{
    private PlayerHands handsScript;

    public const int HotBarLength = 3;
    public const int InventoryLength = 24;

    [Header("Add Slot.cs to these if you like to add an item in edtior")]
    [SerializeField] private Button[] hotbarSlots = new Button[HotBarLength];
    [SerializeField] private Button[] inventorySlots = new Button[InventoryLength];

    private List<ItemInfo> lastClickedItems = new();

    // Separate data arrays for hotbar and inventory
    private InventorySlot[] hotbarData;
    private InventorySlot[] inventoryData;

    // Unified flat array (indices 0..HotBarLength-1 = hotbar, rest = inventory)
    // This preserves the original indexing used throughout all methods.
    private InventorySlot[] allSlots;

    // Parallel UI arrays
    private UISlot[] hotbarUISlots;
    private UISlot[] inventoryUISlots;

    private Dictionary<string, ItemInfo> itemInfos;

    private Canvas inventoryCanvas;
    private int selected;
    private int swapSelection = -1;
    private InventoryInputActions inputActions;

    /// <summary>Fired whenever the contents of any hotbar slot change (add, remove, swap, craft etc.).</summary>
    public static event Action OnHotbarContentsChanged;
    /// <summary>Fired whenever the selected hotbar index changes. Passes the new index.</summary>
    public static event Action<int> OnHotbarSelectionChanged;

    protected override void Awake()
    {
        base.Awake();
        inventoryCanvas = GetComponentInChildren<Canvas>();
        inventoryCanvas.enabled = false;

        inputActions = new InventoryInputActions();

        hotbarData = new InventorySlot[HotBarLength];
        inventoryData = new InventorySlot[InventoryLength];
        allSlots = new InventorySlot[HotBarLength + InventoryLength];

        hotbarUISlots = new UISlot[HotBarLength];
        inventoryUISlots = new UISlot[InventoryLength];

        itemInfos = new();
        foreach (var entry in RecipeInfo.Instance.NamesToItemInfos)
        {
            itemInfos[entry.Key.ToString()] = entry.Value;
        }
    }

    void OnEnable()
    {
        inputActions.InventorySelection.Enable();
        inputActions.InventorySelection.Scroll.performed += OnScroll;
        //inputActions.InventorySelection.NumberKeys.performed += OnNumberKeyInput;
    }

    void OnDisable()
    {
        inputActions.InventorySelection.Disable();
        inputActions.InventorySelection.Scroll.performed -= OnScroll;
        //inputActions.InventorySelection.NumberKeys.performed -= OnNumberKeyInput;
    }

    private void OnScroll(InputAction.CallbackContext context)
    {
        if (ObjectPlacer.Instance.InPlacementMode) return;
        float scrollValue = context.ReadValue<float>();
        if (scrollValue == 0) return;

        // Build a list of occupied hotbar indices so scrolling skips empty slots
        var occupied = new List<int>();
        for (int i = 0; i < HotBarLength; i++)
        {
            if (allSlots[i] != null && allSlots[i].count > 0 &&
                allSlots[i].itemInfo != null &&
                allSlots[i].itemInfo.itemName != ItemInfo.ItemName.Empty)
                occupied.Add(i);
        }
        if (occupied.Count == 0) return;

        int currentPos = occupied.IndexOf(selected);
        if (currentPos == -1) currentPos = 0;
        int dir = scrollValue > 0 ? 1 : -1;
        int nextPos = (currentPos + dir + occupied.Count) % occupied.Count;
        int nextIndex = occupied[nextPos];

        selected = nextIndex;
        OnHotbarSelectionChanged?.Invoke(selected);
        PlayerID.Instance.playerHUD.TriggerHUDEvent();

        AnimatorOverrideController itemAnimator = GetSlotAnimation(allSlots[selected]);
        if (itemAnimator != null)
            LoadHandAnimation(itemAnimator);
        else
            DeloadHandAnimator();
    }

    private void OnNumberKeyInput(InputAction.CallbackContext context)
    {
        if (ObjectPlacer.Instance.InPlacementMode) return;
        float value = context.ReadValue<float>();
        int index = (int)(value) - 1;
        if (index >= HotBarLength) return;
        Select(index);
    }

    #region Player Hands Helper Functions
    /// <summary>
    /// Loads an animator controller into player hands.
    /// </summary>
    private void LoadHandAnimation(AnimatorOverrideController handAnimatorController)
    {
        if (handAnimatorController != null)
        {
            handsScript.SetOverrideController(handAnimatorController);
        }
    }

    /// <summary>
    /// Deloads the animator controller from player hands, reverting to the default.
    /// </summary>
    private void DeloadHandAnimator()
    {
        handsScript.SetOverrideController();
    }

    /// <summary>
    /// Returns the animator override controller for the item in the given slot, or null if there is none.
    /// </summary>
    private AnimatorOverrideController GetSlotAnimation(InventorySlot slot)
    {
        ItemInfo itemInfo = slot.itemInfo;
        if (itemInfo != null)
        {
            IPlayerActionStrategy actionStrategy = itemInfo.playerActionStrategy;
            if (actionStrategy != null)
            {
                return actionStrategy.handAnimatorController;
            }
        }
        return null;
    }
    #endregion

    /// <summary>
    /// this update function is literally only to fix the bug where items don't have an animation
    /// only when entering a scene with a weapon equip.
    /// </summary>
    void Update()
    {
        Reselect();
    }

    void Start()
    {
        handsScript = PlayerHands.instance;

        hotbarData = new InventorySlot[HotBarLength];
        inventoryData = new InventorySlot[InventoryLength];
        allSlots = new InventorySlot[HotBarLength + InventoryLength];

        hotbarUISlots = new UISlot[HotBarLength];
        inventoryUISlots = new UISlot[InventoryLength];

        // Initialise hotbar data slots and link to UI
        for (int i = 0; i < HotBarLength; i++)
        {
            hotbarData[i] = new InventorySlot(i);
            allSlots[i] = hotbarData[i];

            if (hotbarSlots[i] == null) continue;

            if (!hotbarSlots[i].TryGetComponent<UISlot>(out UISlot slot))
            {
                slot = hotbarSlots[i].AddComponent<UISlot>();
            }
            slot.index = i;
            hotbarUISlots[i] = slot;
            hotbarData[i].SetUISlot(slot);

            hotbarSlots[i].onClick.AddListener(() => OnSlotSelected(slot));
        }

        // Initialise inventory data slots and link to UI
        for (int i = 0; i < InventoryLength; i++)
        {
            inventoryData[i] = new InventorySlot(HotBarLength + i);
            allSlots[HotBarLength + i] = inventoryData[i];

            if (inventorySlots[i] == null) continue;

            if (!inventorySlots[i].TryGetComponent<UISlot>(out UISlot slot))
            {
                slot = inventorySlots[i].AddComponent<UISlot>();
            }
            slot.index = HotBarLength + i;
            Debug.Log(slot.itemInfo == null ? "inv slot has null iteminfo!" : "inv slot good");
            inventoryUISlots[i] = slot;
            inventoryData[i].SetUISlot(slot);

            inventorySlots[i].onClick.AddListener(() => DebugOnInvSlotSelected(slot));
        }

        // Initialise every slot to Empty defaults; saved contents (if any) are pushed in below via SaveManager.
        selected = 0;
        for (int i = 0; i < allSlots.Length; i++)
        {
            allSlots[i].index = i;
            allSlots[i].count = 0;
            allSlots[i].itemInfo = itemInfos[ItemInfo.ItemName.Empty.ToString()];
            allSlots[i].UpdateSlot();
        }

        for (int i = 0; i < InventoryLength; i++)
        {
            if (inventorySlots[i] == null) continue;
            var uiSlot = inventorySlots[i].GetComponent<UISlot>();
            Debug.Log(uiSlot.itemInfo == null ? "inv slot has null iteminfo!" : "inv slot good");
        }
        PrintInventory();

        // Fire initial HUD update now that all slots are ready.
        OnHotbarContentsChanged?.Invoke();
        OnHotbarSelectionChanged?.Invoke(selected);

        // Pull saved inventory data (if any). WhenGameplayReady guarantees the module has been
        // deserialised even if we entered the scene before SaveManager finished its scene-load pass.
        var save = SaveManager.Instance;
        if (save != null)
        {
            save.WhenGameplayReady(() => save.Apply<InventoryModule>());
        }
    }

    /// <summary>
    /// Rebuild the live inventory from a save-data POCO. Called by <see cref="InventoryModule.Apply"/>.
    /// Idempotent: calling it repeatedly with the same data leaves the inventory unchanged.
    /// </summary>
    public void LoadFromSaveData(InventorySaveData data)
    {
        if (data == null || data.inventory == null) return;

        Debug.Log("Initializing inventory from save");
        selected = data.selected;

        int len = Mathf.Min(data.inventory.Length, allSlots.Length);
        for (int i = 0; i < len; i++)
        {
            allSlots[i].index = i;
            allSlots[i].count = data.inventory[i].count;
            string name = data.inventory[i].name;
            if (allSlots[i].count > 0 && itemInfos.TryGetValue(name, out var info))
            {
                allSlots[i].itemInfo = info;
            }
            else
            {
                allSlots[i].itemInfo = itemInfos[ItemInfo.ItemName.Empty.ToString()];
            }
            allSlots[i].UpdateSlot();
        }

        OnHotbarContentsChanged?.Invoke();
        OnHotbarSelectionChanged?.Invoke(selected);
    }

    /// <summary>
    /// Enables or disables the inventory UI and toggles player movement/cursor accordingly.
    /// </summary>
    public void ShowInventory(bool enabled)
    {
        inventoryCanvas.enabled = enabled;
        PlayerInput.Instance.DebugToggleInput(enabled);
        if (inventoryCanvas.enabled)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
        }
        if (swapSelection != -1)
        {
            allSlots[swapSelection].SetColor(Color.white);
            if (allSlots[swapSelection].itemInfo && allSlots[swapSelection].itemInfo.isIngredient)
            {
                for (int i = 0; i < HotBarLength; i++)
                {
                    allSlots[i].SetColor(Color.white);
                }
            }
            swapSelection = -1;
        }
    }

    public bool isEnabled()
    {
        return inventoryCanvas.enabled;
    }

    void OnSlotSelected(UISlot uiSlot)
    {
        Debug.Log("Hotbar slot #" + uiSlot.index + " clicked");
    }

    // Debug method – shows recipe crafting; won't work this way in a playable build.
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
    /// Switches the selected hotbar slot. Only succeeds if the slot is occupied.
    /// </summary>
    /// <param name="index">Hotbar index to switch to.</param>
    public void Select(int index)
    {
        // Number-key presses: only switch if that slot actually has an item
        bool occupied = allSlots[index] && allSlots[index].count > 0 &&
                        allSlots[index].itemInfo != null &&
                        allSlots[index].itemInfo.itemName != ItemInfo.ItemName.Empty;
        if (!occupied) return;

        bool indexIsNew = selected != index;
        selected = index;
        OnHotbarSelectionChanged?.Invoke(selected);

        if (indexIsNew)
        {
            AnimatorOverrideController itemAnimator = GetSlotAnimation(allSlots[index]);
            if (itemAnimator != null)
            {
                LoadHandAnimation(itemAnimator);
            }
            else
            {
                Debug.LogWarning("no tool animation found for current tool!");
                DeloadHandAnimator();
            }
        } else {
            DeloadHandAnimator();
        }

        PlayerID.Instance.playerHUD.TriggerHUDEvent();
    }

    /// <summary>
    /// Reselects the currently selected slot. Use this when the tool's animation may have changed.
    /// </summary>
    public void Reselect() {
        AnimatorOverrideController itemAnimator = GetSlotAnimation(allSlots[selected]);
        if (itemAnimator != null) {
            LoadHandAnimation(itemAnimator);
        }
        else
        {
            DeloadHandAnimator();
        }
    }

    /// <summary>
    /// Decrements the count of the currently selected item by one.
    /// </summary>
    public void Decrement()
    {
        allSlots[selected].count--;
        Debug.Log("Used " + allSlots[selected].itemInfo.itemName + ", " + allSlots[selected].count + " remaining");
        if (allSlots[selected].count == 0)
        {
            allSlots[selected].itemInfo = itemInfos[ItemInfo.ItemName.Empty.ToString()];
        }
        allSlots[selected].UpdateSlot();
        PackHotbar();
        Reselect();
        OnHotbarContentsChanged?.Invoke();
    }

    /// <summary>
    /// Determines whether the inventory contains at least <paramref name="count"/> of the given item.
    /// </summary>
    public bool Contains(ItemInfo.ItemName itemName, int count)
    {
        int found = 0;
        for (int i = 0; i < allSlots.Length; i++)
        {
            if (allSlots[i]?.count > 0 && allSlots[i].itemInfo.itemName == itemName)
            {
                found += allSlots[i].count;
                if (found >= count)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Crafts an item by removing the recipe ingredients and adding the output to the inventory.
    /// </summary>
    /// <param name="recipe">Recipe to craft.</param>
    public void Craft(Recipe recipe)
    {
        int amountToRemove = 0;
        for (int ingredients = 0; ingredients < recipe.counts.Count; ingredients++)
        {
            amountToRemove = recipe.counts[ingredients];
            for (int i = 0; i < allSlots.Length; i++)
            {
                if (amountToRemove > 0 && allSlots[i]?.count > 0 && allSlots[i].itemInfo.itemName == recipe.ingredients[ingredients].itemName)
                {
                    if (allSlots[i].count <= amountToRemove)
                    {
                        amountToRemove -= allSlots[i].count;
                        allSlots[i].count = 0;
                        allSlots[i].itemInfo = itemInfos[ItemInfo.ItemName.Empty.ToString()];
                        allSlots[i].UpdateSlot();
                    }
                    else
                    {
                        allSlots[i].count -= amountToRemove;
                        amountToRemove = 0;
                        allSlots[i].UpdateSlot();
                    }
                    if (amountToRemove == 0)
                    {
                        break;
                    }
                }
            }
        }
        PackHotbar();
        OnHotbarContentsChanged?.Invoke();
        AddItem(recipe.output, 1);
    }

    /// <summary>
    /// Adds <paramref name="count"/> of <paramref name="itemInfo"/> to the inventory.
    /// Hotbar slots are kept gap-free: items are always packed toward index 0, filling
    /// rightward so the occupied slots stay contiguous.
    /// </summary>
    /// <returns>The number of items that could not be added.</returns>
    public int AddItem(ItemInfo itemInfo, int count)
    {
        bool hotbarAffected = false;

        // First try to add to existing stacks
        for (int i = 0; i < allSlots.Length; i++)
        {
            if (itemInfo.isIngredient && i < HotBarLength)
            {
                continue;
            }
            if (allSlots[i]?.count > 0 && allSlots[i].itemInfo.itemName == itemInfo.itemName)
            {
                if (itemInfo.maxStackCount > allSlots[i].count)
                {
                    if (itemInfo.maxStackCount < allSlots[i].count + count)
                    {
                        count -= itemInfo.maxStackCount - allSlots[i].count;
                        allSlots[i].count = itemInfo.maxStackCount;
                        allSlots[i].UpdateSlot();
                    }
                    else
                    {
                        allSlots[i].count += count;
                        allSlots[i].UpdateSlot();
                        count = 0;
                    }
                    if (i < HotBarLength) hotbarAffected = true;
                    Reselect();
                    Debug.Log("Added " + itemInfo.itemName + " to existing stack at index " + i + ". Current count is " + allSlots[i].count);
                    if (count <= 0)
                    {
                        if (hotbarAffected) PackHotbar();
                        OnHotbarContentsChanged?.Invoke();
                        return 0;
                    }
                }
            }
        }

        // Otherwise create a new stack
        if (count > 0)
        {
            for (int i = 0; i < allSlots.Length; i++)
            {
                if (itemInfo.isIngredient && i < HotBarLength)
                {
                    continue;
                }
                if (allSlots[i]?.count == 0)
                {
                    if (count > itemInfo.maxStackCount)
                    {
                        count -= itemInfo.maxStackCount;
                        allSlots[i].itemInfo = itemInfo;
                        allSlots[i].count = itemInfo.maxStackCount;
                        allSlots[i].UpdateSlot();
                    }
                    else
                    {
                        allSlots[i].itemInfo = itemInfo;
                        allSlots[i].count += count;
                        allSlots[i].UpdateSlot();
                        count = 0;
                    }
                    if (i < HotBarLength) hotbarAffected = true;
                    Debug.Log("Added " + itemInfo.itemName + " to new stack at index " + i + ". Current count is " + allSlots[i].count);
                    Reselect();
                    if (count <= 0)
                    {
                        if (hotbarAffected) PackHotbar();
                        OnHotbarContentsChanged?.Invoke();
                        return 0;
                    }
                }
            }
        }

        if (hotbarAffected) PackHotbar();
        Reselect();
        OnHotbarContentsChanged?.Invoke();
        return count;
    }

    /// <summary>
    /// Packs hotbar slots so there are no gaps: occupied items are pushed to the right
    /// (highest indices), mirroring the right-aligned HUD layout.
    /// </summary>
    private void PackHotbar()
    {
        // Gather occupied item data from left to right
        var items = new List<(ItemInfo info, int count)>();
        for (int i = 0; i < HotBarLength; i++)
        {
            if (allSlots[i] != null && allSlots[i].count > 0 &&
                allSlots[i].itemInfo != null &&
                allSlots[i].itemInfo.itemName != ItemInfo.ItemName.Empty)
            {
                items.Add((allSlots[i].itemInfo, allSlots[i].count));
            }
        }

        // Write empty into leading slots, then occupied items into trailing slots
        int emptyCount = HotBarLength - items.Count;
        for (int i = 0; i < HotBarLength; i++)
        {
            if (i < emptyCount)
            {
                allSlots[i].itemInfo = itemInfos[ItemInfo.ItemName.Empty.ToString()];
                allSlots[i].count = 0;
            }
            else
            {
                allSlots[i].itemInfo = items[i - emptyCount].info;
                allSlots[i].count = items[i - emptyCount].count;
            }
            allSlots[i].UpdateSlot();
        }

        // Clamp selected to a valid occupied slot
        if (selected < emptyCount || allSlots[selected].count == 0)
        {
            selected = emptyCount < HotBarLength ? emptyCount : 0;
            OnHotbarSelectionChanged?.Invoke(selected);
        }
    }

    /// <summary>
    /// Removes <paramref name="count"/> of <paramref name="item"/> from the inventory.
    /// </summary>
    /// <returns>True if the items were successfully removed.</returns>
    public bool RemoveItem(ItemInfo item, int count)
    {
        bool hotbarAffected = false;
        for (int i = 0; i < allSlots.Length; i++)
        {
            if (allSlots[i].itemInfo.itemName == item.itemName)
            {
                if (allSlots[i].count >= count)
                {
                    allSlots[i].count -= count;
                    if (allSlots[i].count == 0)
                    {
                        allSlots[i].itemInfo = itemInfos[ItemInfo.ItemName.Empty.ToString()];
                    }
                    allSlots[i].UpdateSlot();
                    if (i < HotBarLength) hotbarAffected = true;
                    Reselect();
                    if (hotbarAffected) PackHotbar();
                    OnHotbarContentsChanged?.Invoke();
                    return true;
                }
                else
                {
                    count -= allSlots[i].count;
                    allSlots[i].count = 0;
                    allSlots[i].itemInfo = itemInfos[ItemInfo.ItemName.Empty.ToString()];
                    allSlots[i].UpdateSlot();
                    if (i < HotBarLength) hotbarAffected = true;
                }
            }
        }
        Reselect();
        if (hotbarAffected) PackHotbar();
        OnHotbarContentsChanged?.Invoke();
        return false;
    }

    public void SwapSelect(int index)
    {
        if (swapSelection == -1)
        {
            if (allSlots[index].count == 0 || allSlots[index].itemInfo.itemName == ItemInfo.ItemName.Empty) return;
            swapSelection = index;
            allSlots[swapSelection].SetColor(Color.green);
            if (allSlots[swapSelection].itemInfo && allSlots[swapSelection].itemInfo.isIngredient)
            {
                for (int i = 0; i < HotBarLength; i++)
                {
                    allSlots[i].SetColor(Color.red);
                }
            }
            Debug.Log("Swap selected " + index);
        }
        else
        {
            if (swapSelection == index)
            {
                allSlots[swapSelection].SetColor(Color.white);
                swapSelection = -1;
                Debug.Log("Deselected " + index);
            }
            else
            {
                if (allSlots[swapSelection].itemInfo.isIngredient && index < HotBarLength)
                {
                    Debug.Log("Cannot swap ingredient to hotbar");
                    return;
                }
                if (allSlots[index].itemInfo && allSlots[swapSelection].itemInfo &&
                    allSlots[index].itemInfo.itemName == allSlots[swapSelection].itemInfo.itemName)
                {
                    if (allSlots[index].count + allSlots[swapSelection].count <= allSlots[index].itemInfo.maxStackCount)
                    {
                        allSlots[index].count += allSlots[swapSelection].count;
                        allSlots[swapSelection].count = 0;
                    }
                    else
                    {
                        int moveAmount = allSlots[index].itemInfo.maxStackCount - allSlots[index].count;
                        allSlots[index].count += moveAmount;
                        allSlots[swapSelection].count -= moveAmount;
                    }
                    Debug.Log("Stacked " + swapSelection + " onto " + index);
                }
                else
                {
                    int tempCount = allSlots[index].count;
                    ItemInfo tempItemInfo = allSlots[index].itemInfo;
                    allSlots[index].count = allSlots[swapSelection].count;
                    allSlots[index].itemInfo = allSlots[swapSelection].itemInfo;
                    allSlots[swapSelection].count = tempCount;
                    allSlots[swapSelection].itemInfo = tempItemInfo;
                    Debug.Log("Swapped " + index + " and " + swapSelection);
                }

                allSlots[index].SetColor(Color.white);
                allSlots[swapSelection].SetColor(Color.white);
                allSlots[index].UpdateSlot();
                allSlots[swapSelection].UpdateSlot();

                for (int i = 0; i < HotBarLength; i++)
                {
                    allSlots[i].SetColor(Color.white);
                }

                // Re-pack if either side of the swap was a hotbar slot
                bool hotbarTouched = index < HotBarLength || swapSelection < HotBarLength;
                swapSelection = -1;
                if (hotbarTouched) PackHotbar();
                OnHotbarContentsChanged?.Invoke();
            }
        }

        Reselect();
    }
    public void RemoveInventory()
    {
        for (int i = 0; i < allSlots.Length; i++)
        {
            allSlots[i].count = 0;
            allSlots[i].itemInfo = itemInfos[ItemInfo.ItemName.Empty.ToString()];
            allSlots[i].UpdateSlot();
        }
        selected = 0;
        Reselect();
        OnHotbarSelectionChanged?.Invoke(selected);
        OnHotbarContentsChanged?.Invoke();
    }

    public void LoadInventory(ItemInfo[] finfo, int[] fcount)
    {
        Debug.Log(allSlots.Length + " length");

        for (int i = 0; i < finfo.Length; i++)
        {
            if (allSlots[i].count == 0 || allSlots[i].itemInfo.itemName == ItemInfo.ItemName.Empty)
            {
                allSlots[i].count = fcount[i];
                allSlots[i].itemInfo = finfo[i];
                allSlots[i].UpdateSlot();

                fcount[i] = 0;
                finfo[i] = itemInfos[ItemInfo.ItemName.Empty.ToString()];
            }
        }

        for (int i = 0; i < finfo.Length; i++)
        {
            if (fcount[i] > 0 && finfo[i].itemName != ItemInfo.ItemName.Empty)
            {
                Debug.Log($"Adding {fcount[i]} {finfo[i].itemName.ToString()}");
                AddItem(finfo[i], fcount[i]);
            }
        }

        Debug.Log(allSlots.Length + " new length");
        PackHotbar();
        Reselect();
        OnHotbarContentsChanged?.Invoke();
    }

    /// <summary>
    /// Drops one of the item at the given inventory index.
    /// </summary>
    /// <returns>Whether the drop was successful.</returns>
    public bool Drop(int index)
    {
        allSlots[index].count--;
        if (allSlots[index].count <= 0)
        {
            allSlots[index].itemInfo = itemInfos[ItemInfo.ItemName.Empty.ToString()];
        }
        if (index < HotBarLength)
        {
            PackHotbar();
            OnHotbarContentsChanged?.Invoke();
        }
        return true;
    }

    /// <summary>
    /// Drops one of the currently selected item.
    /// </summary>
    public void Drop()
    {
        Drop(selected);
    }

    /// <summary>
    /// Prints a string representation of the inventory to the console.
    /// </summary>
    public void PrintInventory()
    {
        string s = "{\n";
        for (int i = 0; i < HotBarLength; i++)
        {
            if (allSlots[i] == null) s += "null  ";
            else if (allSlots[i].count > 0 && allSlots[i].itemInfo) s += "[" + allSlots[i].itemInfo.itemName + ", " + allSlots[i].count + "]  ";
            else s += "[empty]  ";
        }
        for (int i = HotBarLength; i < allSlots.Length; i++)
        {
            if ((i - HotBarLength) % 6 == 0) s += "\n";
            if (allSlots[i] == null) s += "null  ";
            else if (allSlots[i].count > 0 && allSlots[i].itemInfo) s += "[" + allSlots[i].itemInfo.itemName + ", " + allSlots[i].count + "]  ";
            else s += "[empty]  ";
        }
        s += "\n}";
        Debug.Log(s);
    }

    /// <summary>
    /// Returns whether the slot at <paramref name="index"/> is empty.
    /// </summary>
    public bool IsEmpty(int index)
    {
        return allSlots[index].count == 0;
    }

    public bool IsInventoryEmpty()
    {
        for (int i = 0; i < allSlots.Length; i++)
        {
            if (!IsEmpty(i)) return false;
        }
        return true;
    }

    /// <summary>
    /// Returns the ItemInfo of the currently selected hotbar slot.
    /// </summary>
    public ItemInfo GetSelectedItem()
    {
        return allSlots[selected] ? allSlots[selected]?.itemInfo : null;
    }

    /// <summary>
    /// Returns the count of the currently selected hotbar slot.
    /// </summary>
    public int GetSelectedItemCount()
    {
        return allSlots[selected] ? allSlots[selected].count : 0;
    }

    /// <summary>
    /// Returns the index of the currently selected hotbar slot.
    /// </summary>
    public int GetSelected()
    {
        return selected;
    }

    /// <summary>
    /// Returns the ItemInfo at the given flat index.
    /// </summary>
    public ItemInfo GetItem(int index)
    {
        return allSlots[index].itemInfo;
    }

    /// <summary>
    /// Returns the full flat slot array (hotbar + inventory) for serialisation and external reads.
    /// </summary>
    public InventorySlot[] GetInventory()
    {
        return allSlots;
    }

    /// <summary>
    /// Returns only the hotbar data slots (indices 0..HotBarLength-1).
    /// </summary>
    public InventorySlot[] GetHotbarSlots()
    {
        return hotbarData;
    }

    public ItemInfo InfoLookup(string itemName)
    {
        return itemInfos[itemName];
    }

}

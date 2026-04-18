public class InventorySlot
{
    public ItemInfo ItemInfo { get; private set; }
    public int Index { get; private set; }
    public int count;

    // Backing UI slot – set once during initialisation
    private UISlot uiSlot;

    // Convenience accessor kept lowercase to match the old UISlot field names used across the codebase
    public ItemInfo itemInfo
    {
        get => ItemInfo;
        set => ItemInfo = value;
    }

    public int index
    {
        get => Index;
        set => Index = value;
    }

    public void SetItemInfo(ItemInfo itemInfo)
    {
        ItemInfo = itemInfo;
    }

    public void SetUISlot(UISlot slot)
    {
        uiSlot = slot;
    }

    /// <summary>
    /// Pushes the current data to the linked UISlot and refreshes its display.
    /// </summary>
    public void UpdateSlot()
    {
        if (uiSlot != null)
        {
            uiSlot.SetData(ItemInfo, count);
            uiSlot.UpdateSlot();
        }
    }

    /// <summary>
    /// Sets the background colour of the linked UISlot button.
    /// </summary>
    public void SetColor(UnityEngine.Color color)
    {
        uiSlot?.SetColor(color);
    }

    public InventorySlot(int index, ItemInfo itemInfo)
    {
        Index = index;
        ItemInfo = itemInfo;
    }

    public InventorySlot(int index)
    {
        Index = index;
        ItemInfo = null;
    }

    public void SetIndex(int index)
    {
        Index = index;
    }

    public static implicit operator bool(InventorySlot slot) => slot != null;
}
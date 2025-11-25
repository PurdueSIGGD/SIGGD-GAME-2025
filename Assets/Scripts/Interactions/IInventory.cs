public interface IInventory
{
    /**
     * <summary>
     * Adds an item to the inventory. Returns the number of items that could not be added (0 if all were added).
     * </summary>
     *
     * <param name="item">The item to add.</param>
     * <param name="count">The number of items to add.</param>
     *
     * <returns>The number of items that could not be added.</returns>
     */
    public int AddItem(ItemInfo item, int count); // add item to inventory
    
    
    
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
    public bool RemoveItem(ItemInfo item, int count); // remove item from inventory
}
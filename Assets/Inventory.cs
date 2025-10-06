using System.Runtime.ExceptionServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class Inventory
{

    private Slot[] inventory; // array (or 2D-array) for entire inventory
    private Slot[] hotbar; // array for hotbar
    private int selected; // index of selected item in hotbar
    private Slot tempSlot; // temporary slot for holding item that is being moved

    public Inventory() {
        inventory = new Slot[27];
        hotbar = new Slot[9];
    }

    /// <summary>
    /// Switches the selected item (limited to hotbar)
    /// </summary>
    /// <param name="index">Index to switch to</param>
    public void select(int index) {
        selected = index;
        Debug.Log("Selected " + index + " index, containing " + hotbar[index].itemInfo.itemName);
    }

    /// <summary>
    /// Searches for item in inventory and returns the index
    /// </summary>
    /// <param name="itemName">Name of the item</param>
    /// <returns>Index of the item or -1 if not found</returns>
    public int find(ItemInfo.ItemName itemName) {
        for (int i = 0; i < inventory.Length; i++) {
            if (inventory[i].itemInfo.itemName == itemName)
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
    public void add(ItemInfo itemInfo) { // maybe change input type
        // search for item in inventory
        // add to current stack if it exists
        // create new stack if needed
    }

    /// <summary>
    /// Drop item at index
    /// </summary>
    /// <param name="index">Index of item to drop</param>
    public void drop(int index) { // maybe create another method for dropping stacks of items
        // remove item
        // instantiate physical item
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

    /// <summary>
    /// Class to store info for each inventory slot
    /// </summary>
    private class Slot {
        public int count;
        public ItemInfo itemInfo;

        public Slot() {
            count = 0;
            itemInfo = null;
        }
    }
}

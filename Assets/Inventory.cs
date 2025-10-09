using System.Runtime.ExceptionServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class Inventory : MonoBehaviour
{

    private Slot[] inventory; // array (or 2D-array) for entire inventory; first 9 indices are the hotbar
    private int selected; // index of selected item in hotbar
    private Slot tempSlot; // temporary slot for holding item that is being moved

    void Start()
    {
        inventory = new Slot[27];
        for (int i = 0; i < inventory.Length; i++)
        {
            inventory[i] = new Slot();
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
    public void add(ItemInfo itemInfo, int count) { // maybe change input type
        // add to current stack
        int emptyIndex = -1;
        for (int i = 0; i < inventory.Length; i++) {
            if (inventory[i].count > 0 && inventory[i].itemInfo.itemName == itemInfo.itemName)
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
                    if (count <= 0) return;
                }
                
            }
            else if (inventory[i].count == 0 && emptyIndex == -1) {
                emptyIndex = i;
            }
        }
        // otherwise create new stack if possible
        // is it possible to pick up more than a stack of an item at a time? if so, need to rewrite this part to match above
        if (emptyIndex != -1) {
            while (count > 0) {
                
            }
            inventory[emptyIndex].count += count;
            inventory[emptyIndex].itemInfo = itemInfo;
            Debug.Log("Added " + itemInfo.itemName + " to new stack at index " + emptyIndex + ". Current count is " + inventory[emptyIndex].count);
            return;
        }
        // otherwise replace current selected item
        drop();
        inventory[selected].itemInfo = itemInfo;
        inventory[selected].count = 0;

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

    public void displayInventory() {
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

using UnityEngine;
/// <summary>
/// Script for the physical item that can be picked up
/// </summary>
public class PhysicalItem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] public ItemInfo itemInfo;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Pickup() { 
        // remove physical gameobject and add item to inventory
    }

    /*
     * When player interacts with object, call Pickup (Player team handles interacting?) (PhysicalItem and Inventory both have pickup)
     * Inventory Pickup: Inventory class adds item
     * - checks if it is already in inventory; if it is add to stack, if not create new stack
     * Delete physical gameobject
     */

    /*
     * Inventory methods:
     * - Find if an item is in inventory and where
     * - Return things in a specific slot
     * - Return things in selected slot
     * - Change selected slot
     * - Moving items: swap whatever is in temp slot with slot that is selected
     * - Add things to inventory
     * - Drop things from inventory
     * - Use up an item (decrement)
     * - Has Slot class that holds ItemInfo and count
     */

}

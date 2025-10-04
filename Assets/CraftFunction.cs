using NUnit.Framework;
using UnityEngine;


/// <summary>
///  drag 1 item on top of another, that is what crafting is
///  take the input for what the first item is and what is in the slot dropped into
///  check if there's an actual recipe for it
///  if there is remove both items and put the new item into the slot
/// </summary>
public class CraftFunction : MonoBehaviour
{
    int slot1Selected = -1;
    int slot2Selected = -1;

    string[,] recipeList = { {"spear", "sword"} , {"placeholder", "placeholder"} };
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    void OnSlotSelected(int slotIndex)
    {
        if(slot1Selected == -1)
        {
            slot1Selected = slotIndex;
        }
        else
        {
            slot2Selected = slotIndex; 
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(slot1Selected != -1 && slot2Selected != -1)
        {
            // implement getItemAtSlot function <---
            // item1 = slot1Selected.getItemAtSlot();
            // item2 = slot2Selected.getItemAtSlot();
            item item1 = "Spear";
            item item2 = "Not Spear";
            for(int i = 0; i< recipeList.Length; i++)
            {
                if(recipeList[i,0] == item1 && recipeList[i,1] == item2)
                {
                    // item at slot2Selected -> output of recipe of two items
                }
            }
        }
    }
}

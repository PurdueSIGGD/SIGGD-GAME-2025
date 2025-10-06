using NUnit.Framework;
using UnityEngine;


/// <summary>
///  drag 1 item on top of another, that is what crafting is
///  take the input for what the first item is and what is in the slot dropped into
///  check if there's an actual recipe for it
///  if there is remove both items and put the new item into the slot
/// </summary>
public class CraftFunction : Inventory
{
    // i'm using -1 because arrays / lists dont have a -1? unless its like python and decides to read the far right index
    int slot1Selected = -1;
    int slot2Selected = -1;

    // need to implement the real recipe list, this should just be temporary but I'm thinking it will look like this
    string[,] recipeList = { { "item 1", "item 2", "result item" }, { "placeholder", "placeholder", "placeholders" } };


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
        if((slot1Selected != -1) && (slot2Selected != -1))
        {
            // I don't understand how to use getItem 
            /*
            ItemInfo.ItemName item1 = getItem(slot1Selected);
            ItemInfo.ItemName item2 = getItem(slot2Selected);
            */

            string item1 = "item 1";
            string item2 = "item 2";

            for(int i = 0; i < recipeList.Length; i++)
            {
                if(recipeList[i,0] == item1 && recipeList[i,1] == item2)
                {
                    // is there a delete function?
                    drop(slot1Selected);
                    drop(slot2Selected);

                    // This adds the result to the next available index
                    // but I don't understand how the ItemInfo type works
                    // add(recipeList[i, 2]);

                    // can't forget to "unselect" the slots
                    slot1Selected = -1;
                    slot2Selected = -1;
                }
            }
        }
    }
}

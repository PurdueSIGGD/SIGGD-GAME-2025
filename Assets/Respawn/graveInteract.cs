using System;
using UnityEngine;

public class graveInteract : MonoBehaviour, IInteractable<IInteractor>
{
    private UISlot[] inventory; // array (or 2D-array) for entire inventory; first 9 indices are the hotbar
    ItemInfo[] info;
    int[] count;
    Inventory inventoryObj;
    void Start()
    {
        //inventoryObj = GameObject.Find("Inventory");
    }
    public void OnHoverEnter(InteractableUI ui)
    {
        ui.ActivateUI(this);
        //Debug.Log($"Hovering over item: {itemInfo.itemName}");
    }
    
    public void OnHoverExit(InteractableUI ui)
    {
        ui.DeactivateUI();
        //Debug.Log($"Stopped hovering over item: {itemInfo.itemName}");
    }
    
    public void OnInteract(IInteractor interactor)
    {
        inventoryObj.SetInventory(info, count);
        Destroy(this.gameObject); // Remove the item from the scene
    }
    public void FillGrave(Inventory inv)
    {
        Debug.Log("grave filled");
        inventoryObj = inv;
        info = new ItemInfo[inventoryObj.GetInventory().Length];
        count = new int[inventoryObj.GetInventory().Length];
        for (int i = 0; i < info.Length; i++)
        {
            info[i] = inventoryObj.GetInventory()[i].itemInfo;
            count[i] = inventoryObj.GetInventory()[i].count;
        }
        inventoryObj.RemoveInventory();
    }

}

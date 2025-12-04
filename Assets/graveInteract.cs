using System;
using UnityEngine;

public class graveInteract : MonoBehaviour, IInteractable<IInteractor>
{
    private UISlot[] inventory; // array (or 2D-array) for entire inventory; first 9 indices are the hotbar
    GameObject inventoryObj;
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
         Debug.Log("inventoryObj = " + inventoryObj);
    Debug.Log("Inventory component = " + inventoryObj.GetComponent<Inventory>());
    Debug.Log("inventory = " + inventory);
        inventoryObj.GetComponent<Inventory>().SetInventory(inventory);
        Destroy(this.gameObject); // Remove the item from the scene

    }
    public void FillGrave(GameObject inv)
    {
        Debug.Log("grave filled");
        inventoryObj = inv;
        inventory = new UISlot[inventoryObj.GetComponent<Inventory>().RemoveInventory().Length];
        Array.Copy(inventoryObj.GetComponent<Inventory>().RemoveInventory(), inventory, inventoryObj.GetComponent<Inventory>().RemoveInventory().Length);
    }

}

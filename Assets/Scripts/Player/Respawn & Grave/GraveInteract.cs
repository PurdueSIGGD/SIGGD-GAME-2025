using UnityEngine;

public class GraveInteract : MonoBehaviour, IInteractable<IInteractor>
{
    public ItemInfo[] info;
    public int[] count;
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
        Debug.Log("Grave interacted");
        string infos = "[";
        for (int i = 0; i < info.Length; i++)
        {
            infos += info[i].itemName + " ";
        }
        infos += "]";
        Debug.Log(infos);
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

    public void FillGrave(Inventory inv, ItemInfo[] finfo, int[] fcount)
    {
        Debug.Log("grave filled from save");
        inventoryObj = inv;
        info = new ItemInfo[finfo.Length];
        count = new int[finfo.Length];
        for (int i = 0; i < info.Length; i++)
        {
            info[i] = finfo[i];
            count[i] = fcount[i];
        }
    }

}

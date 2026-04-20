using UnityEngine;
using System;
public class PickUpBait : MonoBehaviour, IInteractable<IInteractor>
{
    public Action<ItemInfo, IInteractor> OnItemPickUp;
    [Tooltip("Different bait types that can be dropped by the Carcass")]
    public ItemInfo[] itemInfos;
    [Tooltip("The drop rates for each item in itemInfo. Must be the same length as itemInfo and sum to 100")]
    public int[] dropRates;
    ItemInfo selectedItem;
    InteractableUI ui;
    public void Start()
    {
        int totalRate = 0;
        foreach (int rate in dropRates)
        {
            totalRate += rate;
        }
        int randomValue = UnityEngine.Random.Range(0, totalRate);
        int cumulativeRate = 0;
        for (int i = 0; i < itemInfos.Length; i++)
        {
            cumulativeRate += dropRates[i];
            if (randomValue < cumulativeRate)
            {
                selectedItem = itemInfos[i];
                return;
            }
        }
        selectedItem = itemInfos[itemInfos.Length - 1];
    }
    public void OnHoverEnter(InteractableUI ui)
    {
        ui.ActivateUI(this);
        this.ui = ui;
    }

    public void OnHoverExit(InteractableUI ui)
    {
        ui.DeactivateUI();
        this.ui = ui;
    }

    public void OnInteract(IInteractor interactor)
    {
        int leftover = interactor.Inventory.AddItem(selectedItem, 1);
        if (leftover > 0)
        { // Don't do anything if there is no inventory space
            ui.ResetInteractUI();
        }
        else
        {
            OnItemPickUp?.Invoke(selectedItem, interactor);
            Debug.Log(selectedItem.itemName + " picked up!");
            Destroy(this.gameObject); // Remove the item from the scene
        }
    }
}
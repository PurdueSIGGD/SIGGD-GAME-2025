using System;
using UnityEngine;
public class Interactable : MonoBehaviour, IInteractable<IInteractor>
{
    public Action<ItemInfo, IInteractor> OnItemInteract;

    public ItemInfo itemInfo;
    public bool destroyAfterPickup;

    private bool interactable = true;
    private InteractableUI currentUi;

    public static readonly string interactSound = "PlayerExamine";

    public void OnHoverEnter(InteractableUI ui)
    {
        if (interactable)
        {
            ui.ActivateUI(this);
            currentUi = ui;
            //Debug.Log($"Hovering over item: {itemInfo.itemName}");
        }
    }

    public void OnHoverExit(InteractableUI ui)
    {
        ui.DeactivateUI();
        currentUi = null;
        //Debug.Log($"Stopped hovering over item: {itemInfo.itemName}");
    }

    public void OnInteract(IInteractor interactor)
    {
        if (interactable)
        {
            //Debug.Log($"Item {itemInfo.itemName} interacted up by interactor.");
            //OnItemInteract?.Invoke(itemInfo, interactor);
            try
            {
                OnItemInteract?.Invoke(itemInfo, interactor);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during OnItemInteract: {e}");
            }
            interactable = false;
            if (currentUi) currentUi.DeactivateUI();
            if (destroyAfterPickup)
            {
                AudioManager.Instance.PlayOneShotNoAsync(InteractableItem.itemPickupSound, PlayerID.Instance.gameObject.transform.position);
                Destroy(this.gameObject); // Remove the item from the scene
            }
            else
            {
                AudioManager.Instance.PlayOneShotNoAsync(interactSound, PlayerID.Instance.gameObject.transform.position);
            }
        }
    }
}
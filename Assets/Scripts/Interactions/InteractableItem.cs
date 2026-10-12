using System;
using SIGGD.Save;
using SIGGD.Save.Modules;
using UnityEngine;

public class InteractableItem : MonoBehaviour, IInteractable<IInteractor>
{
    public Action<ItemInfo, IInteractor> OnItemPickUp;

    public ItemInfo itemInfo;

    InteractableUI ui;

    public static readonly string itemPickupSound = "ItemPickup";

    // Music boxes are one-off pickups: once taken they are recorded in the save so reloading does not respawn them
    private bool IsOneOffPickup => itemInfo != null && itemInfo.IsMusicBox;

    private void Start()
    {
        if (!IsOneOffPickup) return;

        SaveManager.Instance?.WhenGameplayReady(() =>
        {
            if (this == null) return;
            var player = SaveManager.Instance.Get<PlayerModule>();
            if (player != null && player.IsPickupCollected(itemInfo.itemName.ToString()))
            {
                Debug.Log($"Item {itemInfo.itemName} was already collected in this save, removing it from the scene.");
                Destroy(gameObject);
            }
        });
    }

    public void OnHoverEnter(InteractableUI ui)
    {
        ui.ActivateUI(this);
        this.ui = ui;
        Debug.Log($"Hovering over item: {itemInfo.itemName}");
    }

    public void OnHoverExit(InteractableUI ui)
    {
        ui.DeactivateUI();
        this.ui = ui;
        Debug.Log($"Stopped hovering over item: {itemInfo.itemName}");
    }

    public void OnInteract(IInteractor interactor)
    {
        int leftover = interactor.Inventory.AddItem(itemInfo, 1);
        AudioManager.Instance.PlayOneShotNoAsync(itemPickupSound, PlayerID.Instance.gameObject.transform.position);
        if (leftover > 0)
        { // Don't do anything if there is no inventory space
            Debug.Log($"Out of inventory space. Unable to pick up {itemInfo.itemName}.");
            ui.ResetInteractUI();
        }
        else
        {
            Debug.Log($"Item {itemInfo.itemName} picked up by interactor.");

            // DEBUG
            Debug.Log(OnItemPickUp == null ? "No APPLE listeners" : "APPLE Listeners found");

            OnItemPickUp?.Invoke(itemInfo, interactor);
            if (IsOneOffPickup)
            {
                SaveManager.Instance?.Get<PlayerModule>()?.MarkPickupCollected(itemInfo.itemName.ToString());
            }
            Destroy(this.gameObject); // Remove the item from the scene
        }
    }
}
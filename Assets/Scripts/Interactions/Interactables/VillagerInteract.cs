using UnityEngine;
using FMOD;
using FMODUnity;
using FMOD.Studio;
using Debug = UnityEngine.Debug;

public class VillagerInteract : MonoBehaviour, IInteractable<IInteractor>
{
    InteractableUI curUI;
    [SerializeField] private float lookSpeed = 5f;

    private bool playerIsLooking = false;
    private Transform playerTransform;

    private void Update()
    {
        // Smoothly rotate toward the player while they are looking at us and holding a flower
        if (playerIsLooking && playerTransform != null /*&& PlayerIsHoldingFlower()*/)
        {
            Vector3 direction = playerTransform.position - transform.position;
            direction.y = 0f;
            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * lookSpeed);
            }
        }
    }

    public void OnHoverEnter(InteractableUI ui) {
        playerIsLooking = true;
        if (PlayerID.Instance != null)
        {
            playerTransform = PlayerID.Instance.transform;
        }
        if (PlayerIsHoldingFlower())
        {
            ui.ActivateUI(this);
            curUI = ui;
        }
    }

    public void OnHoverExit(InteractableUI ui) {
        ui.DeactivateUI();
        curUI = null;
        playerIsLooking = false;
        playerTransform = null;
    }

    public void OnInteract(IInteractor interactor)
    {
        // when interact with slug play slug noise
        RuntimeManager.PlayOneShot(FMODEvents.Instance.GetEventReferenceNoAsync("AilenTalk"), transform.position); // play sound for axe

        // Check player has a flower selected
        if (Inventory.Instance.GetSelectedItem()?.itemName == ItemInfo.ItemName.Flower)
        {
            AudioManager.Instance.PlayOneShotNoAsync(Interactable.interactSound, PlayerID.Instance.gameObject.transform.position);
            Inventory.Instance.Decrement();
            ItemInfo slimeball = RecipeInfo.Instance.NamesToItemInfos[ItemInfo.ItemName.Slimeball];
            Inventory.Instance.AddItem(slimeball, 1);
            curUI.DeactivateUI();
            curUI = null;
        }
    }
    private bool PlayerIsHoldingFlower()
    {
        var selectedItem = Inventory.Instance.GetSelectedItem();
        return selectedItem != null && selectedItem.itemName == ItemInfo.ItemName.Flower;
    }
}

using UnityEngine;

public class VillagerInteract : MonoBehaviour, IInteractable<IInteractor>
{
    InteractableUI ui;
    [SerializeField] private float lookSpeed = 5f;
    [SerializeField] private look_at_player headlook;

    private bool playerIsLooking = false;
    private Transform playerTransform;
    headLook.transform = playerTransform;

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
        if (headLook != null) {
            headlook.setActive(true);
        }
    }

    public void OnHoverExit(InteractableUI ui) {
        playerIsLooking = false;
        playerTransform = null;
        if (headLook != null) {
            headLook.setActive(false);
        }
    }

    public void OnInteract(IInteractor interactor)
    {
        // Check player has a flower selected
        if (Inventory.Instance.GetSelectedItem()?.itemName == ItemInfo.ItemName.Flower)
        {
            Inventory.Instance.Decrement();
            ItemInfo slimeball = RecipeInfo.Instance.NamesToItemInfos[ItemInfo.ItemName.Slimeball];
            Inventory.Instance.AddItem(slimeball, 1);
        }
    }
    private bool PlayerIsHoldingFlower()
    {
        var selectedItem = Inventory.Instance.GetSelectedItem();
        return selectedItem != null && selectedItem.itemName == ItemInfo.ItemName.Flower;
    }
}

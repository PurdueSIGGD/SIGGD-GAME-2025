using UnityEngine;

public class ShopKeeperInteract : MonoBehaviour, IInteractable<IInteractor>
{

    // For now the shopkeeper script is just opening the crafting menu since the shopkeeper
    // doesn't need to move (and I am unsure if it should also rotate to face the player or
    // if it stays completely stationary).
    InteractableUI ui;
    [SerializeField] private float lookSpeed = 5f;

    private bool playerIsLooking = false;
    private Transform playerTransform;

    private void Update()
    {
        // Smoothly rotate toward the player
        if (playerIsLooking && playerTransform != null)
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

    public void OnHoverEnter(InteractableUI ui)
    {
        playerIsLooking = true;
        if (PlayerID.Instance != null)
        {
            playerTransform = PlayerID.Instance.transform;
        }
    }                         

    public void OnHoverExit(InteractableUI ui)
    {
        playerIsLooking = false;
        playerTransform = null;
    }

    public void OnInteract(IInteractor interactor)
    {
        if (CraftingMenu.Instance)
        {
            CraftingMenu.Instance.ShowCraftingMenu(true);
        }
        else
        {
            Debug.LogError("ShopKeeper: CraftingMenu instance not found!");
        }
    }


}

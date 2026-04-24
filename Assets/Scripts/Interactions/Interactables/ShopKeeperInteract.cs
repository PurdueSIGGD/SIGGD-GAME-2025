using System.Collections;
using UnityEngine;

public class ShopKeeperInteract : MonoBehaviour, IInteractable<IInteractor>
{

    // For now the shopkeeper script is just opening the crafting menu since the shopkeeper
    // doesn't need to move (and I am unsure if it should also rotate to face the player or
    // if it stays completely stationary).
    InteractableUI ui;
    [SerializeField] private float lookSpeed = 5f;

    [Header("really important sliming out feature")]
    [SerializeField] private GameObject slimeObject;

    [SerializeField] private Transform startPoint;

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
        ui.ActivateUI(this);
    }                         

    public void OnHoverExit(InteractableUI ui)
    {
        ui.DeactivateUI();
    }

    public void OnInteract(IInteractor interactor)
    {
        if (PlayerID.Instance.stateMachine.GetEquippedItem()?.itemName == ItemInfo.ItemName.MusicBox)
        {
            Debug.Log("Shopkeeper: Player interacted with ShopKeeper while holding Music Box. Triggering slime out.");
            SlimeOutPlayer();
            ui.DeactivateUI();
            return;
        }
        
        if (CraftingMenu.Instance)
        {
            AudioManager.Instance.PlayOneShotNoAsync(Interactable.interactSound, PlayerID.Instance.gameObject.transform.position);
            CraftingMenu.Instance.ShowCraftingMenu(true);
        }
        else
        {
            Debug.LogError("ShopKeeper: CraftingMenu instance not found!");
        }
    }

    private void SlimeOutPlayer()
    {
        Inventory.Instance.Decrement();
        
        Debug.Log("Shopkeeper: Player interacted with ShopKeeper");
        var direction = PlayerID.Instance.transform.position - startPoint.position;
        
        Debug.Log("Shopkeeper: Calculated direction to player: " + direction);
        var slime = Instantiate(slimeObject, startPoint.position, Quaternion.LookRotation(direction));
        slime.transform.localScale = Vector3.one * 0.5f;

        Debug.Log("Shopkeeper: Instantiated slime at " + startPoint.position + " facing " + direction);
        StartCoroutine(MoveSlime(slime, direction, 0.5f));
    }
    
    IEnumerator MoveSlime(GameObject slime, Vector3 direction, float duration)
    {
        float elapsedTime = 0f;

        Vector3 startPosition = slime.transform.position;
        Vector3 endPosition = PlayerID.Instance.transform.position + Vector3.up * 0.5f;

        while (elapsedTime < duration)
        {
            slime.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        slime.transform.position = endPosition;

        
        Debug.Log("Shopkeeper: Increasing player's slime level and triggering slimed out effect.");
        SaveManager.Instance.playerModule.playerData.slimeLevel = Mathf.Clamp(SaveManager.Instance.playerModule.playerData.slimeLevel + 1, 0, 4);
        
        Debug.Log("Shopkeeper: Sliming out");
        SlimedOut.Instance.TriggerSlimedOut();
        Destroy(slime);
    }
}
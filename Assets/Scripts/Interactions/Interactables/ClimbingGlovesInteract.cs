using UnityEngine;

public class ClimbingGlovesInteract : MonoBehaviour, IInteractable<IInteractor>
{

    private PlayerStamina playerStamina;

    void Start() {
        playerStamina = PlayerID.Instance.playerStamina;
        if (SaveManager.Instance.playerModule.playerData.hasGloves) {
            Debug.Log("Removed climbing gloves from scene since they've already been picked up");
            Destroy(gameObject);
        }
    }
    public void OnHoverEnter(InteractableUI ui) {
        ui.ActivateUI(this);
    }
    public void OnHoverExit(InteractableUI ui) {
        ui.DeactivateUI();
    }

    public void OnInteract(IInteractor interactor)
    {
        Debug.Log("Picked up climbing gloves");
        SaveManager.Instance.playerModule.playerData.hasGloves = true;
        AudioManager.Instance.PlayOneShotNoAsync(InteractableItem.itemPickupSound, PlayerID.Instance.gameObject.transform.position);
        Destroy(this.gameObject);
    }
}

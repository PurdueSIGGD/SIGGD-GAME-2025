using UnityEngine;

public class ClimbingGlovesInteract : MonoBehaviour, IInteractable<IInteractor>
{

    private PlayerStamina playerStamina;

    void Start() {
        playerStamina = PlayerID.Instance.playerStamina;
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
        playerStamina.HasGloves = true;
        Destroy(this.gameObject);
    }
}

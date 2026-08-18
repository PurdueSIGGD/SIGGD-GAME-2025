using UnityEngine;

public class ConsoleInteract : MonoBehaviour, IInteractable<IInteractor>
{
    public static int consolesBroken = 0;
    
    public static readonly string ConsoleBroken = "ConsoleDamaged";

    public void OnHoverEnter(InteractableUI ui) {
        ui.ActivateUI(this);
    }
    public void OnHoverExit(InteractableUI ui) {
        ui.DeactivateUI();
    }

    public void OnInteract(IInteractor interactor)
    {
        Debug.Log("Broke this console");
        AudioManager.Instance.PlayOneShotNoAsync(ConsoleBroken, transform.position);
        consolesBroken++;
        Destroy(this.gameObject);
    }
}
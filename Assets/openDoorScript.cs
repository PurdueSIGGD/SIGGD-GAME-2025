using Unity.VisualScripting;
using UnityEngine;

public class openDoorScript : MonoBehaviour, IInteractable<IInteractor>
{    
    [Tooltip("Set this to the Retrieve Key Card Objective")]
    public QuestObjective keyCardobjective;
    
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
        // Checks if the retrieve key card objective is completed, and will "open" the door if the keycard is retrieved
        if (QuestManager.Instance.IsObjectiveComplete(keyCardobjective))
        {
            Destroy(this.gameObject);
        }
    }
}

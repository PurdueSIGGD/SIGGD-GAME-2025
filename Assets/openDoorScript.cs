using UnityEngine;

public class openDoorScript : MonoBehaviour, IInteractable<IInteractor>
{    
    public QuestObjective keyCardobjective;
    
    
    public void OnHoverEnter(InteractableUI ui)
    {
        ui.ActivateUI(this);
        //Debug.Log($"Hovering over item: {itemInfo.itemName}");
    }

    public void OnHoverExit(InteractableUI ui)
    {
        ui.DeactivateUI();
        //Debug.Log($"Stopped hovering over item: {itemInfo.itemName}");
    }

    public void OnInteract(IInteractor interactor)
    {
        Debug.Log(QuestManager.Instance.IsObjectiveComplete(keyCardobjective) + "objective status");
        
        if (QuestManager.Instance.IsObjectiveComplete(keyCardobjective))
        {
            Destroy(this.gameObject); // Remove the item from the scene
        }
        else Debug.Log("Quest not complete");
    }
}

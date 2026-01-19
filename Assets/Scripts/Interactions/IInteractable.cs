using UnityEngine;

public interface IInteractable<in T> where T : IInteractor
{
    void OnHoverEnter(InteractableUI ui); // visual or audio feedback when player looks at interactable
    
    void OnHoverExit(InteractableUI ui); // visual or audio feedback when player looks at interactable
    
    void OnInteract(T interactor); // interactables determine what they do when interacted with (pickup, dialogue, open door, etc.)
}
using System;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
public class InteractDoor : MonoBehaviour, IInteractable<IInteractor>
{
    public Action<ItemInfo, IInteractor> OnItemInteract;

    private bool interactable = true;
    private InteractableUI currentUi;
    private Animator animator;
    private MeshCollider[] colliders;

    private static readonly string openDoorSound = "HeavySpaceDoorOpen";
    
    public void Start()
    {
        animator = GetComponent<Animator>();
        colliders = GetComponents<MeshCollider>();
    }

    public void OnHoverEnter(InteractableUI ui)
    {
        if (interactable)
        {
            ui.ActivateUI(this);
            currentUi = ui;
        }
    }

    public void OnHoverExit(InteractableUI ui)
    {
        ui.DeactivateUI();
        currentUi = null;
    }

    public void OnInteract(IInteractor interactor)
    {
        if (interactable)
        {
            animator.SetTrigger("door_open");
            AudioManager.Instance.PlayOneShotNoAsync(openDoorSound, transform.position);
            interactable = false;
            if (currentUi) currentUi.DeactivateUI();
            
            // this toggles the enabled state of the two colliders of the door so the player can move through the door when it opens
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = !colliders[i].enabled;
            }
        }
    }
}
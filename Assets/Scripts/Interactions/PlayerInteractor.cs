using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour, IInteractor
{
    public float interactionDistance = 3f;
    public IInteractable<IInteractor> Interactable;
    
    public InteractableUI interactableUI;
    
    [HideInInspector] public PlayerID playerID;

    private void Start()
    {
        playerID = GetComponent<PlayerID>();
        PlayerInput.Instance.OnInteract += OnInteractAction;
    }

    private void Update()
    {
        if (Physics.SphereCast(playerID.cam.transform.position, 0.1f, 
                playerID.cam.transform.forward, out RaycastHit hit, interactionDistance))
        {
            var interactable = hit.collider.GetComponentInParent<IInteractable<IInteractor>>();
            if (interactable != null)
            {
                if (interactable is MonoBehaviour mb && mb.enabled == false) // added check to avoid interacting with disabled interactables
                {
                    Interactable?.OnHoverExit(interactableUI);
                    Interactable = null;
                    return;
                }

                if ((Interactable == null || !Interactable.Equals(interactable)) && !ObjectPlacer.Instance.InPlacementMode &&
                    PlayerID.Instance.IsAlive)
                {
                    Interactable?.OnHoverExit(interactableUI);
                    Interactable = interactable;
                    Debug.Log(Interactable);
                    Interactable?.OnHoverEnter(interactableUI);
                }
                else if (Interactable != null && (ObjectPlacer.Instance.InPlacementMode || !PlayerID.Instance.IsAlive))
                { // Disable interactable UI if entered placement mode
                    Interactable?.OnHoverExit(interactableUI);
                    Interactable = null;
                }
            }
            else
            {
                Interactable?.OnHoverExit(interactableUI);
                Interactable = null;
            }
        }
        else
        {
            Interactable?.OnHoverExit(interactableUI);
            Interactable = null;
        }
    }
    
    private void OnInteractAction(InputAction.CallbackContext context)
    {
        if (context.performed && Interactable != null)
        {
            Interact(Interactable);
            // TODO: Play Interact sound
        }
    }

    #region IInteractor Implementation
    
    public IInventory Inventory => playerID.Inventory;

    public void Interact(IInteractable<IInteractor> interactable)
    {
        interactableUI.BeginInteractUI(Interactable, () =>
        {
            interactable.OnInteract(this);
        }, () => !PlayerInput.Instance.interactionHeld);
    }

    #endregion
}
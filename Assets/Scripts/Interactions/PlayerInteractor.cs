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
            if (hit.collider.TryGetComponent<IInteractable<IInteractor>>(out var interactable))
            {
                if (Interactable == null || !Interactable.Equals(interactable))
                {
                    Interactable?.OnHoverExit(interactableUI);
                    Interactable = interactable;
                    Interactable.OnHoverEnter(interactableUI);
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
using System.Collections;
using SIGGD.Save;
using SIGGD.Save.Modules;
using UnityEngine;
using UnityEngine.UI;

public class VillagerDeadInteract : MonoBehaviour, IInteractable<IInteractor>
{

    public void OnHoverEnter(InteractableUI ui) {
        ui.ActivateUI(this);
    }
    public void OnHoverExit(InteractableUI ui) {
        ui.DeactivateUI();
    }

    public void OnInteract(IInteractor interactor)
    {
        var player = SaveManager.Instance?.Get<PlayerModule>();
        if (player != null) player.SlimeLevel++;
    }
}
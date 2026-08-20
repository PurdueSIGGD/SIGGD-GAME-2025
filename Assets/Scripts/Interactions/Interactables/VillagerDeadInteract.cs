using System.Collections;
using SIGGD.Save;
using SIGGD.Save.Modules;
using UnityEngine;
using UnityEngine.UI;

public class VillagerDeadInteract : Interactable
{

    public void OnInteract(IInteractor interactor)
    {
        base.OnInteract(interactor);
        var player = SaveManager.Instance?.Get<PlayerModule>();
        if (player != null) player.SlimeLevel++;
    }
}
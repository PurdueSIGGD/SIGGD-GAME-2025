using UnityEngine;

public class WorkbenchInteract : MonoBehaviour, IInteractable<IInteractor>
{
    InteractableUI ui;

    public void OnHoverEnter(InteractableUI ui)
    {
        if (CraftingMenu.Instance.IsCanvasActive()) return;

        ui.ActivateUI(this);
        this.ui = ui;
    }

    public void OnHoverExit(InteractableUI ui)
    {
        ui.DeactivateUI();
        this.ui = ui;
    }
    public void OnInteract(IInteractor interactor, InteractableUI ui)
    {
        this.ui.DeactivateUI();
        if (CraftingMenu.Instance)
        {
            CraftingMenu.Instance.ShowCraftingMenu(true);
        }
        else
        {
            Debug.LogError("Workbench cannot find crafting canvas!");
        }
    }

}

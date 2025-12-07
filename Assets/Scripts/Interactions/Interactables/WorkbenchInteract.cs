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
    public void OnInteract(IInteractor interactor)
    {
        ui.DeactivateUI();
        if (CraftingMenu.Instance)
        {
            CraftingMenu.Instance.ShowCraftingMenu(true);
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            PlayerInput.Instance.DebugToggleInput(true);
        }
        else
        {
            Debug.LogError("Workbench cannot find crafting canvas!");
        }
    }

}

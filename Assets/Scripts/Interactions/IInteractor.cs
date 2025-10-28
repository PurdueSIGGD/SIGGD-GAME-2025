/**
 * An interactor is an entity that can interact with interactables.
 * Examples of interactors include players, NPCs, or automated systems.
 * Interactors define how they interact with various interactables, but not what the interactables do.
 */
public interface IInteractor
{
    public IInventory Inventory { get; } // interactor's inventory
    public void Interact(IInteractable<IInteractor> interactable); // interactor determines how to interact with interactable
}
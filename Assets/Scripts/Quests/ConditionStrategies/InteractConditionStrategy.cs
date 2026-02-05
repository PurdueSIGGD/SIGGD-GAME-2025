public class InteractConditionStrategy : IQuestConditionStrategy
{
    private bool itemInteracted = false;
    public Interactable interactable;
    public string targetItemName; 


    protected override void OnInitialize()
    {
        base.OnInitialize();
        interactable.OnItemInteract += OnItemInteract;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        interactable.OnItemInteract -= OnItemInteract;
    }

    private void OnItemInteract(ItemInfo item, IInteractor interactor)
    {
        itemInteracted = true;
        Broadcast(Broadcaster);
    }

    public override bool Evaluate() => itemInteracted;

    public override string ToString() => $"Item Pickup: {targetItemName}";

    public override bool StopIfTriggered() => false;

}

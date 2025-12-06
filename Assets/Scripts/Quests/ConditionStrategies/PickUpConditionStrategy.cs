public class ItemPickupConditionStrategy : IQuestConditionStrategy
{
    private bool itemPickedUp = false;
    public InteractableItem interactable;
    public string targetItemName; 


    protected override void OnInitialize()
    {
        base.OnInitialize();
        interactable.OnItemPickUp += OnItemPickUp;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        interactable.OnItemPickUp -= OnItemPickUp;
    }

    private void OnItemPickUp(ItemInfo item, IInteractor interactor)
    {
        //if (item.itemName.ToString() == targetItemName)
        //{
            itemPickedUp = true;
            Broadcast(Broadcaster);
        //}
    }

    public override bool Evaluate() => itemPickedUp;

    public override string ToString() => $"Item Pickup: {targetItemName}";

    public override bool StopIfTriggered() => false;

}

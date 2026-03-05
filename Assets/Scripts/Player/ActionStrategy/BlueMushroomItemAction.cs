using UnityEngine;

public class BlueMushroomItemAction : IPlayerActionStrategy
{
    protected override void OnEnter()
    {
        base.OnEnter();
        Inventory.Instance.Decrement();
        DamageContext poisonContext = new DamageContext();
        poisonContext.attacker = poisonContext.victim = PlayerID.Instance.gameObject;
        poisonContext.amount = 100;
        PlayerID.Instance.GetComponent<EntityHealthManager>().TakeDamage(poisonContext);
        PlayerID.Instance.GetComponent<PlayerHunger>().UpdateHunger(-20);
        Debug.Log("player ate a mushroom");
    }
}

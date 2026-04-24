using UnityEngine;

public class GreenMushroomItemAction : IPlayerActionStrategy
{
    protected override void OnEnter()
    {
        base.OnEnter();
        Inventory.Instance.Decrement();
        DamageContext poisonContext = new DamageContext();
        poisonContext.attacker = poisonContext.victim = PlayerID.Instance.gameObject;
        poisonContext.amount = 30;
        PlayerID.Instance.GetComponent<EntityHealthManager>().TakeDamage(poisonContext);
        PlayerID.Instance.GetComponent<PlayerHunger>().UpdateHunger(-20);
        Debug.Log("player ate a mushroom");
    }
}

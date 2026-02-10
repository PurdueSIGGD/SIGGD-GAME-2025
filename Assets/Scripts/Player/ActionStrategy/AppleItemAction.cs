using UnityEngine;

public class AppleItem : IPlayerActionStrategy
{
    protected override void OnEnter()
    {
        base.OnEnter();
        Inventory.Instance.Decrement();
        DamageContext healContext = new DamageContext();
        healContext.attacker = healContext.victim = PlayerID.Instance.gameObject;
        healContext.amount = 20;
        PlayerID.Instance.GetComponent<EntityHealthManager>().Heal(healContext);
        PlayerID.Instance.GetComponent<PlayerHunger>().UpdateHunger(20);
        Debug.Log("player ate an apple");
    }
}

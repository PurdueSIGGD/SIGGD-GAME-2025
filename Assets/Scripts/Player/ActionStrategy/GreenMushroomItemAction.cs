using UnityEngine;

public class GreenMushroomItemAction : IPlayerActionStrategy
{
    private static readonly string eatSound = "PlayerConsume";
    protected override void OnEnter()
    {
        base.OnEnter();
        Inventory.Instance.Decrement();
        AudioManager.Instance.PlayOneShotNoAsync(eatSound, PlayerID.Instance.gameObject.transform.position);
        DamageContext poisonContext = new DamageContext();
        poisonContext.attacker = poisonContext.victim = PlayerID.Instance.gameObject;
        poisonContext.amount = 30;
        PlayerID.Instance.GetComponent<EntityHealthManager>().TakeDamage(poisonContext);
        PlayerID.Instance.GetComponent<PlayerHunger>().UpdateHunger(-20);
        Debug.Log("player ate a mushroom");
    }
}

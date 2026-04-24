using UnityEngine;
using System.Collections;

public class PurpleMushroomItemAction : IPlayerActionStrategy
{
    private static readonly string eatSound = "PlayerConsume";
    protected override void OnEnter()
    {
        base.OnEnter();
        Inventory.Instance.Decrement();
        AudioManager.Instance.PlayOneShotNoAsync(eatSound, PlayerID.Instance.gameObject.transform.position);
        GameObject player = PlayerID.Instance.gameObject;
        HealOverTime(player);
        Debug.Log("player ate purple mushroom");
    }

    private void HealOverTime(GameObject player)
    {
        DamageContext healContext = new DamageContext();
        healContext.attacker = healContext.victim = PlayerID.Instance.gameObject;
        healContext.amount = -20;
        PlayerID.Instance.GetComponent<EntityHealthManager>().Heal(healContext);
        PlayerID.Instance.GetComponent<PlayerHunger>().UpdateHunger(20);
        Debug.Log("player ate a mushroom");
    }
}
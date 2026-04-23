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
        player.GetComponent<MonoBehaviour>().StartCoroutine(HealOverTime(player));
        Debug.Log("player ate purple mushroom");
    }

    private IEnumerator HealOverTime(GameObject player)
    {
        int totalHeals = 8;
        float totalDuration = 16f;
        float interval = totalDuration / totalHeals;
        for (int i = 0; i < totalHeals; i++)
        {
            DamageContext healContext = new DamageContext();
            healContext.attacker = healContext.victim = player;
            healContext.amount = 20;
            PlayerID.Instance.GetComponent<EntityHealthManager>().Heal(healContext);
            PlayerID.Instance.GetComponent<PlayerHunger>().UpdateHunger(20);
            yield return new WaitForSeconds(interval);
        }
    }
}
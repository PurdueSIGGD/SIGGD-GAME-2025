using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Or HighDefinition if using HDRP

public class BlueMushroomAction : IPlayerActionStrategy
{
    private static readonly string eatSound = "PlayerConsume";
    protected override void OnEnter()
    {
        base.OnEnter();
        Inventory.Instance.Decrement();
        AudioManager.Instance.PlayOneShotNoAsync(eatSound, PlayerID.Instance.gameObject.transform.position);
        GameObject player = PlayerID.Instance.gameObject;
        player.GetComponent<MonoBehaviour>().StartCoroutine(HealOverTime(player));
        //PlayerID.Instance.GetComponent<PlayerHunger>().UpdateHunger(20);
        Debug.Log("player ate dark purple mushroom");
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
            //healContext.amount = -20;
            PlayerID.Instance.GetComponent<EntityHealthManager>().Heal(healContext);
            PlayerID.Instance.GetComponent<PlayerHunger>().UpdateHunger(5);
            yield return new WaitForSeconds(interval);
        }

    }
}
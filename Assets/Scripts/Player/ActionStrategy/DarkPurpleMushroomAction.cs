using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class DarkPurpleMushroomItemAction : IPlayerActionStrategy
{
    protected override void OnEnter()
    {
        base.OnEnter();
        Inventory.Instance.Decrement();
        GameObject player = PlayerID.Instance.gameObject;
        player.GetComponent<MonoBehaviour>().StartCoroutine(HealOverTime(player));
        Debug.Log("player ate dark purple mushroom");
    }

    private IEnumerator HealOverTime(GameObject player)
    {
        GameObject globalVolume = GameObject.Find("Global Volume");
        Transform volume = globalVolume.GetComponent<Transform>().GetChild(0);
        volume.gameObject.SetActive(true);
        int totalHeals = 8;
        float totalDuration = 16f;
        float interval = totalDuration / totalHeals;
        bool jumpscared = false;
        for (int i = 0; i < totalHeals; i++)
        {
            globalVolume.GetComponent<Transform>().GetChild(1).gameObject.SetActive(false);
            if (!jumpscared)
            {
                int randomInt = Random.Range(0, 5);
                if (randomInt == 4)
                {
                    jumpscared = true;
                    globalVolume.GetComponent<Transform>().GetChild(1).gameObject.SetActive(true);
                }
            }
            DamageContext healContext = new DamageContext();
            healContext.attacker = healContext.victim = player;
            healContext.amount = 20;
            PlayerID.Instance.GetComponent<EntityHealthManager>().Heal(healContext);
            PlayerID.Instance.GetComponent<PlayerHunger>().UpdateHunger(20);
            yield return new WaitForSeconds(interval);
        }
        volume.gameObject.SetActive(false);
    }
}
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Or HighDefinition if using HDRP

public class YellowMushroomAction : IPlayerActionStrategy
{
    private static readonly string eatSound = "PlayerConsume";
    protected override void OnEnter()
    {
        base.OnEnter();
        Inventory.Instance.Decrement();
        AudioManager.Instance.PlayOneShotNoAsync(eatSound, PlayerID.Instance.gameObject.transform.position);
        GameObject player = PlayerID.Instance.gameObject;
        player.GetComponent<MonoBehaviour>().StartCoroutine(HealOverTime(player));
        PlayerID.Instance.GetComponent<PlayerHunger>().UpdateHunger(10);
        Debug.Log("player ate yellow mushroom");
    }

    private IEnumerator HealOverTime(GameObject player)
    {
        player.GetComponent<Light>().enabled = true;
        yield return new WaitForSeconds(20);
        player.GetComponent<Light>().enabled = false;
    }
}
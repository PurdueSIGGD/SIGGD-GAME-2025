using UnityEngine;
using System.Collections;

public class OrangeMushroomItemAction : IPlayerActionStrategy
{
    [SerializeField] private float speedMultiplier = 5f;
    protected override void OnEnter()
    {
        base.OnEnter();
        Inventory.Instance.Decrement();
        GameObject player = PlayerID.Instance.gameObject;
        player.GetComponent<MonoBehaviour>().StartCoroutine(SpeedUp(player));
        Debug.Log("player ate an orange mushroom");
    }

    private IEnumerator SpeedUp(GameObject player)
    {
        player.GetComponent<PlayerMovement>().speedMultiplier = speedMultiplier;
        yield return new WaitForSeconds(8);
        player.GetComponent<PlayerMovement>().speedMultiplier = 1f;
    }
}
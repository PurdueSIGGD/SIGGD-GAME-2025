using UnityEngine;

public class RockItemAction : IPlayerActionStrategy
{
    private Camera playerCam;
    protected override void OnEnter()
    {
        playerCam = PlayerID.Instance.cam.GetComponentInChildren<Camera>();
        Debug.Log("Player is facing " + playerCam.transform.forward);


        base.OnEnter();
        PlayHandAction(); // plays animation
        Inventory.Instance.Decrement();
        Debug.Log("player threw a rock");

    }
}

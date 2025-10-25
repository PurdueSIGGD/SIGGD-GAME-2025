using UnityEngine;

public class SpawnItemActionStrategy : IPlayerActionStrategy
{
    public GameObject prefab;

    protected override void OnEnter()
    {
        base.OnEnter();
        GameObject.Instantiate(prefab);
    }
}

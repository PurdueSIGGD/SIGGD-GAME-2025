using System;
using UnityEngine;

[Serializable]
public class MusicBoxStrategy : IPlayerActionStrategy
{
    [Header("Music Box Attributes")] [SerializeField]
    private string cassetteName;

    protected override void OnEnter()
    {
        MusicManager.Instance.StartCoroutine(MusicManager.Instance.playCassette(cassetteName));
    }
}

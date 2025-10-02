using UnityEngine;
using FMODUnity;
using System.Collections.Generic;

[System.Serializable]
public class FMODEvents : MonoBehaviour
{
    [field: SerializeField] public EventReference enemyDeath { get; private set; }
    [field: SerializeField] public EventReference footsteps { get; private set; }
    public static FMODEvents instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            // another message that shouldnt be seen
            Debug.Log("too many instances");
        }

        instance = this;
    }
}

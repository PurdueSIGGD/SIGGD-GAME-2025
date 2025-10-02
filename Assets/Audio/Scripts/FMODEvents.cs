using UnityEngine;
using FMODUnity;
using System.Collections.Generic;

public class FMODEvents : MonoBehaviour
{
    [field: SerializeField] public EventReference enemyDeath { get; private set; }
    [field: SerializeField] public EventReference footsteps { get; private set; }

    public Dictionary<string, EventReference> test2 = new Dictionary<string, EventReference>();
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
    private void Start()
    {
        test2.Add("enemyDeath", enemyDeath);
    }
}

using System.Runtime.CompilerServices;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class GenericStatPickup : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] private StatType statToAffect;
    public float multiplier = 2f;
    public float duration = 5f;

    private GameObject o;

    private void OnTriggerEnter(Collider other)
    {
        Stat stats = other.GetComponent<Stat>();
        o = other.gameObject;

        if (stats != null)
        {
            stats.ApplyMultiplier(statToAffect, multiplier, duration);
            Destroy(gameObject);
        }

         
    }


    #region DEBUGGING

    // TESTING ONLY

    //public Stat targetStats;   // drag the creature here
    //public StatType statToWatch = StatType.Attack;

    //void Update()
    //{
    //    if (targetStats != null)
    //    {
    //        Debug.Log($"{targetStats.name}'s {statToWatch}: {targetStats.GetStat(statToWatch)}");
    //    }

    //    if (Input.GetKeyDown(KeyCode.T) && o != null)
    //    {
    //        Stat stats = o.GetComponent<Stat>();
    //        if (stats != null)
    //        {
    //            stats.ResetModifier(statToAffect);
    //            Debug.Log($"Reset {statToAffect} modifier on {o.name}");
    //        }
    //    }
    //}

    #endregion
}

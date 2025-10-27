using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Stat : MonoBehaviour
{

    [SerializeField] private StatData statData;


    // base stats
    public Dictionary<StatType, float> baseStats = new Dictionary<StatType, float>();

    // current modifiers
    public Dictionary<StatType, float> modifiers = new Dictionary<StatType, float>();

    // holds currently running coroutines for each stat
    private Dictionary<Coroutine, StatType> activeCoroutines = new Dictionary<Coroutine, StatType>();

    private EntityHealthManager healthManager;

    private void Awake()
    {
        foreach (var stat in statData.stats)
        {
            baseStats[stat.stat] = stat.value;
            modifiers[stat.stat] = 1f; // 1 is no modifier
        }
    }

    #region Generic Stat Methods / Modifiers

    public float GetStat(StatType type)
    {
        return baseStats[type] * modifiers[type];
    }   

    public float ChangeStat(StatType type, float amount)
    {
        if (!baseStats.ContainsKey(type))
            return 0f;
        baseStats[type] += amount;
        return baseStats[type];


    }

    public void ApplyMultiplier(StatType type, float multiplier, float duration)
    {
        activeCoroutines[StartCoroutine(ApplyMultiplierCoroutine(type, multiplier, duration))] = type;
        // StartCoroutine(ApplyMultiplierCoroutine(type, multiplier, duration));
    }

    // Coroutine to handle multiplier for x seconds
    IEnumerator ApplyMultiplierCoroutine(StatType type, float multiplier, float duration)
    {
        modifiers[type] *= multiplier;
        yield return new WaitForSeconds(duration);
        modifiers[type] /= multiplier;
    }

    // stop coroutines also
    public void ResetModifier(StatType type)
    {
        // Stop all coroutines affecting this stat
        for (int i = 0; i < activeCoroutines.Count; i++)
        {
            Coroutine coroutine = activeCoroutines.ElementAt(i).Key;

            if (activeCoroutines[coroutine] == type)
            {
                StopCoroutine(coroutine);
                activeCoroutines.Remove(coroutine);
                i--; // Adjust index after removal
            }
        }

        modifiers[type] = 1f;
    }

    // Get base stat without modifiers or 0 if not found
    public float GetBaseStat(StatType stat) => baseStats.ContainsKey(stat) ? baseStats[stat] : 0f;

    public void SetStat(StatType stat, float value)
    {
        baseStats[stat] = value;
    }

    #endregion

 
}

using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Stat : MonoBehaviour
{

    [SerializeField] private StatData statData;

    // base stats
    public Dictionary<StatType, float> baseStats = new Dictionary<StatType, float>();

    // current modifiers
    public Dictionary<StatType, float> modifiers = new Dictionary<StatType, float>();

    private void Awake()
    {
        foreach (var stat in statData.stats)
        {
            baseStats[stat.stat] = stat.value;
            modifiers[stat.stat] = 1f; // 1 is no modifier
        }
    }

    public float GetStat(StatType type)
    {
        return baseStats[type] * modifiers[type];
    }   

    public void ApplyMultiplier(StatType type, float multiplier, float duration)
    {
        StartCoroutine(ApplyMultiplierCoroutine(type, multiplier, duration));
    }

    // Coroutine to handle multiplier for x seconds
    IEnumerator ApplyMultiplierCoroutine(StatType type, float multiplier, float duration)
    {
        modifiers[type] *= multiplier;
        yield return new WaitForSeconds(duration);
        modifiers[type] /= multiplier;
    }

    public void ResetModifier(StatType type)
    {
        modifiers[type] = 1f;
    }

    // Get base stat without modifiers or 0 if not found
    public float GetBaseStat(StatType stat) => baseStats.ContainsKey(stat) ? baseStats[stat] : 0f;
}

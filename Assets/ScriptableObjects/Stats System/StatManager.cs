using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// This class should only be interfaced by the Stat class. It provides the functionality 
/// to update stat values and modifiers for a duration, and will handle resetting those
/// changes after the duration expires.
/// </summary>
public class StatManager : Singleton<StatManager>
{
    /// <summary>
    /// Update modifier for the given stat
    /// </summary>
    public void UpdateModifier(Stat stat, float newMod, float duration)
    {
        if (duration == default) // if duration is 0, apply the modifier indefinitely
        {
            stat.modifier += newMod;
        }
        else
        {
            Coroutine modifierCoroutine = null;
            modifierCoroutine = StartCoroutine(HoldModifier(stat, newMod, duration, () => modifierCoroutine));
            stat.activeModChanges.Add(modifierCoroutine);
        }
    }

    public void UpdateValue(Stat stat, float newVal, float duration)
    {
        if (duration == default)
        {
            stat.value = newVal;
        }
        else
        {
            Coroutine statChangeCoroutine = null;
            statChangeCoroutine = StartCoroutine(HoldValue(stat, newVal, duration, () => statChangeCoroutine));
            stat.activeValChanges.Add(statChangeCoroutine);
        }
    }

    private IEnumerator HoldModifier(Stat stat, float mod, float duration, Func<Coroutine> self)
    {
        if (stat == null)
        {
            Debug.Log("Cannot find stat to modify, obj likely destroyed");
            yield return null;
        }
        stat.modifier += mod;
        yield return new WaitForSeconds(duration);
        stat.modifier -= mod;
        stat.activeModChanges.Remove(self()); // remove the finished coroutine from the list
    }

    private IEnumerator HoldValue(Stat stat, float val, float duration, Func<Coroutine> self)
    {
        if (stat == null)
        {
            Debug.Log("Cannot find stat to modify, obj likely destroyed");
            yield return null;
        }
        stat.value += val;
        yield return new WaitForSeconds(duration);
        stat.value -= val;
        stat.activeValChanges.Remove(self());
    }
}

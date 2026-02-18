using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Provides access to a stat value. Its value and modifier can be updated directly or for a duration.
/// All updates to value and modifier are additive.
/// </summary>
[Serializable]
public class Stat
{
    public float Value => value * (modifier / 100);
    public float UnmodValue => value;

    public float value;
    [NonSerialized] public float modifier;
    [NonSerialized] public float baselineValue; // value first assigned when stat is initialized, used for resetting stat

    public List<Coroutine> activeValChanges;
    public List<Coroutine> activeModChanges;

    [NonSerialized] public MonoBehaviour parent;

    public Stat(float value)
    {
        this.value = baselineValue = value;
        modifier = 100f;

        activeValChanges = new();
        activeModChanges = new();
        //this.parent = parent;
    }

    /// <summary>
    /// Call to reset all active modifications on the stat
    /// </summary>
    public void ResetAll()
    {
        ResetModifier();
        ResetValue();
    }

    /// <summary>
    /// Update modifier for the stat for a duration. If no duration is provided, the change will last until stat is reset.
    /// </summary>
    /// <param name="newMod">amount to add to modifier value</param>
    /// <param name="duration">duration of change</param>
    public void SetModifier(float newMod, float duration = default)
    {
        //if (!StatManager.Instance)
        //{
        //    Debug.LogError("No StatManager instance found. Cannot set modifier.");
        //}
        //else
        //{
        //    StatManager.Instance.UpdateModifier(this, newMod, duration);
        //}
        StatManager.UpdateModifier(this, newMod, duration);
    }

    /// <summary>
    /// Reset all active modifications to the stat
    /// </summary>
    public void ResetModifier()
    {
        //if (StatManager.Instance)
        //{
            foreach (Coroutine modChange in activeModChanges)
            {
                if (modChange != null) parent.StopCoroutine(modChange);
            }
        //}
        //else
        //{
        //    Debug.LogError("No StatManager instance found. Cannot reset modifier.");
        //}
        modifier = 1f;
        activeModChanges.Clear();
    }

    /// <summary>
    /// Directly update the value of the stat for a duration. If no duration is provided, the change will last until stat is reset.
    /// </summary>
    /// <param name="newVal">amount to add to modifier value</param>
    /// <param name="duration">duration of change</param>
    public void SetStatValue(float newVal, float duration = default)
    {
        //if (!StatManager.Instance)
        //{
        //    Debug.LogError("No StatManager instance found. Cannot set value.");
        //}
        //else
        //{
        //    StatManager.Instance.UpdateValue(this, newVal, duration);
        //}
        StatManager.UpdateValue(this, newVal, duration);
    }

    /// <summary>
    /// Reset all direct stat value modifications
    /// </summary>
    public void ResetValue()
    {
        //if (StatManager.Instance)
        //{
            foreach (Coroutine valChange in activeValChanges)
            {
                if (valChange != null) parent.StopCoroutine(valChange);
            }
        //}
        //else
        //{
        //    Debug.LogError("No StatManager instance found. Cannot rest value");
        //}
        value = baselineValue;
        activeValChanges.Clear();
    }
}
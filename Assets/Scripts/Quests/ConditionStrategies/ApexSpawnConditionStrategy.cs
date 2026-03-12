
using System;
using Unity.VisualScripting;
using UnityEngine;

/**
 * A TimerConditionStrategy evaluates to true when a certain duration has elapsed. This time can be
 * sped up, slowed down, skipped or reset based on game events. It also resets itself after being met,
 * allowing for repeated timed conditions.
 */
[Serializable]
public class ApexSpawnConditionStrategy : TimerConditionStrategy
{
    private GameObject spawnedApex;

    protected override void OnInitialize()
    {
        base.OnInitialize();
        spawnedApex = null;
    }

    protected override void OnUpdate()
    {
        if (spawnedApex != null)
        {
            Pause();
            ResetTimer();
        }
        else
        {
            IsRunning = true;
        }

        base.OnUpdate();
    }

    public void SetSpawnedApex(GameObject apex)
    {
        spawnedApex = apex;
    }

    public override string ToString()
    {
        return $"Apex Spawn Strategy";
    }
}
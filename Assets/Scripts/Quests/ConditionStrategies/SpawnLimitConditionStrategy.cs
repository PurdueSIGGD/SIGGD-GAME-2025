
using System;
using UnityEngine;

/**
 * A TimerConditionStrategy evaluates to true when a certain duration has elapsed. This time can be
 * sped up, slowed down, skipped or reset based on game events. It also resets itself after being met,
 * allowing for repeated timed conditions.
 */
[Serializable]
public class SpawnLimitConditionStrategy : IQuestConditionStrategy
{
    public float duration = 5f;
    public float timeScale = 1f;
    private float elapsedTime = 0f;
    public bool IsRunning { get; private set; } = true;

    public bool pauseOnComplete = false; // If true, the timer will pause when the condition is met.
    public bool singleUse = false; // If true, the condition will stop checking after being triggered once.

    public string tag; // tag to limit the spawn count of
    public int spawnLimit = 1;

    protected override void OnInitialize()
    {
        base.OnInitialize();
        elapsedTime = 0f;
        IsRunning = true;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (GameObject.FindGameObjectsWithTag(tag).Length >= spawnLimit)
        {
            elapsedTime = 0f;
            return;
        }

        if (IsRunning)
        {
            elapsedTime += Time.deltaTime * timeScale;
        }

        if (elapsedTime >= duration)
        {
            IsRunning = false;
            Broadcast(Broadcaster);
        }
    }

    public override bool Evaluate()
    {
        if (elapsedTime >= duration)
        {
            elapsedTime = 0f; // Reset timer for repeated use
            if (!singleUse && !pauseOnComplete)
            {
                IsRunning = true; // Resume running if not single use
            }
            return true;
        }
        return false;
    }

    public override bool StopIfTriggered()
    {
        return singleUse;
    }

    public void SetTimeScale(float newScale)
    {
        timeScale = Mathf.Clamp(newScale, 0f, float.MaxValue);
    }

    public void Skip(float time)
    {
        elapsedTime = Mathf.Min(elapsedTime + time, duration);
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
    }

    public void Pause()
    {
        IsRunning = false;
    }

    public void Resume()
    {
        IsRunning = true;
    }

    public override string ToString()
    {
        return $"Timer Strategy";
    }
}
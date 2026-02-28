using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Implement to use stat fields.
/// This enables us to set default values to a stat field.
/// </summary>
public abstract class StatProvider : MonoBehaviour, ISerializationCallbackReceiver
{
    private bool hasInitialized = false;

    protected virtual void OnValidate()
    {
        InitializeStats();
    }

    protected virtual void Awake()
    {
        InitializeStats();
    }

    public void OnAfterDeserialize()
    {
        InitializeStats();
    }

    public void OnBeforeSerialize() { }

    private void InitializeStats()
    {
        if (hasInitialized) return;

        var fields = GetType().GetFields(
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance
        );

        foreach (var field in fields)
        {
            if (field.FieldType == typeof(Stat))
            {
                var stat = field.GetValue(this) as Stat;
                if (stat != null)
                {
                    stat.modifier = 100f;
                    stat.baselineValue = stat.value;
                    stat.parent = this;

                    stat.activeValChanges ??= new List<Coroutine>();
                    stat.activeModChanges ??= new List<Coroutine>();
                }
            }
        }

        hasInitialized = true;
    }
}

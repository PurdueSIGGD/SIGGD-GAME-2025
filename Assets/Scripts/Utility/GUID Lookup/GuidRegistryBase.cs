using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public abstract class GuidRegistryBase : ScriptableObject
{
    public abstract void BuildLookup();
}

public abstract class GuidRegistry<T> : GuidRegistryBase
    where T : ScriptableObject
{
    [SerializeField]
    private List<T> items = new();

    private Dictionary<string, T> lookup;

    private FieldInfo GetGuidFieldFromType(System.Type type)
    {
        // Search up the inheritance hierarchy for the guid field
        while (type != null && type != typeof(object))
        {
            var field = type.GetField("guid", 
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (field != null) return field;
            type = type.BaseType;
        }
        return null;
    }

    public override void BuildLookup()
    {
        lookup = new(items.Count);

        foreach (var item in items)
        {
            if (item == null) continue;

            var guidField = GetGuidFieldFromType(item.GetType());

            if (guidField == null)
            {
                Debug.LogWarning($"Item {item.name} (type: {item.GetType().Name}) does not have a 'guid' field");
                continue;
            }

            string guid = guidField.GetValue(item) as string;

            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogWarning($"Item {item.name} has null or empty guid - AutoGuidProcessor may not have run");
                continue;
            }

            lookup[guid] = item;
        }
    }

    public T Get(string guid)
    {
        if (lookup == null)
            BuildLookup();

        if (lookup == null) return null;
        return lookup.TryGetValue(guid, out var result) ? result : null;
    }

    /// <summary>
    /// Retrieve the GUID of a given item via reflection. Useful for saving references by GUID.
    /// </summary>
    public string GetGuid(T item)
    {
        if (item == null) return null;

        var guidField = GetGuidFieldFromType(item.GetType());

        if (guidField == null)
        {
            Debug.LogError($"Item {item.name} (type: {item.GetType().Name}) does not have a 'guid' field");
            return null;
        }

        string guidValue = guidField.GetValue(item) as string;

        if (string.IsNullOrEmpty(guidValue))
        {
            Debug.LogWarning($"Item {item.name} has empty guid - AutoGuidProcessor may not have run on this asset");
            return null;
        }

        return guidValue;
    }

    public IReadOnlyList<T> Items => items;
}

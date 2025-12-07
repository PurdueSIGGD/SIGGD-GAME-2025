using System.Collections.Generic;
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

    public override void BuildLookup()
    {
        lookup = new(items.Count);

        foreach (var item in items)
        {
            if (item == null) continue;

            var guidField = item.GetType()
                .GetField("guid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (guidField == null) continue;

            string guid = guidField.GetValue(item) as string;

            if (!string.IsNullOrEmpty(guid))
                lookup[guid] = item;
        }
    }

    public T Get(string guid)
    {
        if (lookup == null)
            BuildLookup();

        return lookup.TryGetValue(guid, out var result) ? result : null;
    }

    public IReadOnlyList<T> Items => items;
}

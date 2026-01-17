using UnityEngine;
using System.Collections.Generic;

public class RegistryHub : MonoBehaviour
{
    public static RegistryHub Instance { get; private set; }

    [Header("Registries")]
    public GuidRegistry<QuestObjective> questObjectives;

    private Dictionary<System.Type, GuidRegistryBase> registryCache = new();

    private void Awake()
    {
        Instance = this;

        questObjectives?.BuildLookup();
        RegisterRegistry(questObjectives);
    }

    private void RegisterRegistry(GuidRegistryBase registry)
    {
        if (registry == null) return;

        // Extract the generic type T from GuidRegistry<T>
        var baseType = registry.GetType().BaseType;
        if (baseType != null && baseType.IsGenericType)
        {
            var registryType = baseType.GetGenericArguments()[0];
            registryCache[registryType] = registry;
        }
    }

    public GuidRegistry<T> GetRegistry<T>() where T : ScriptableObject
    {
        if (registryCache.TryGetValue(typeof(T), out var registry))
            return registry as GuidRegistry<T>;

        return null;
    }

}

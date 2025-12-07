using UnityEngine;

public class RegistryHub : MonoBehaviour
{
    public static RegistryHub Instance { get; private set; }

    [Header("Registries")]
    public GuidRegistry<QuestObjective> questObjectives;

    private void Awake()
    {
        Instance = this;

        questObjectives?.BuildLookup();
    }

    public GuidRegistry<T> GetRegistry<T>() where T : ScriptableObject
    {
        if (typeof(T) == typeof(QuestObjective))
            return questObjectives as GuidRegistry<T>;

        return null;
    }

}

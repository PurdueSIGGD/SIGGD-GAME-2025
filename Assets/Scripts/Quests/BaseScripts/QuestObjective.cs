using UnityEngine;

/**
 * A QuestObjective represents a specific goal or task within a quest. It contains a description of the objective
 * and a flag indicating whether the objective has been completed.
 */
[CreateAssetMenu(fileName = "New Quest Objective", menuName = "Quests/Quest Objective")]

public class QuestObjective : ScriptableObject
{
    [Header("Objective Description")]
    [TextArea] public string description;
    public int maxCompletions = 1;
}
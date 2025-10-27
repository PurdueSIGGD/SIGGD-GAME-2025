using System.Collections;
using UnityEngine;

// what type of stats do we want for all entities?

// TOOD: add all modfiable stats that we want all creatures to have
public enum StatType
{
    // resources
    maxHealth,
    maxHunger,
    hungerDecayRate,

    // movement
    walkSpeed,
    sprintSpeed
}
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Stats/Stat Profile")]
public class StatData : ScriptableObject
{
    public List<StatValue> stats;
}

[System.Serializable]
public class StatValue
{
    public StatType stat;
    public float value;
}

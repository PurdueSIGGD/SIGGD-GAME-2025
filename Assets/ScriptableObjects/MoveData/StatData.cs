using UnityEngine;

[CreateAssetMenu(fileName = "Stat Data", menuName = "ScriptableObjects/Stat Data", order = 1)]
public class StatData : ScriptableObject
{
    public string name = "test";

    public void TEST()
    {
        Debug.Log(name);
    }
}

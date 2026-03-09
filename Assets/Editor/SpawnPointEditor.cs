using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using Unity.VisualScripting;

[CustomEditor(typeof(SpawnPoint))]
public class SpawnPointEditor : Editor
{
    private SpawnPoint spawnPoint;

    private void OnEnable()
    {
        spawnPoint = (SpawnPoint)target;
    }

    private void OnSceneGUI()
    {

        if (spawnPoint == null) return;
        DrawPoint(Color.green);

    }
    private void DrawPoint(Color drawColor)
    {
        Handles.color = drawColor;
        Handles.DrawSolidDisc(spawnPoint.transform.position, Vector3.up, HandleUtility.GetHandleSize(spawnPoint.transform.position) / 10);
    }
}

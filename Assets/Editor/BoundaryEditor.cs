using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using Unity.VisualScripting;

[CustomEditor(typeof(Boundary))]
public class BoundaryEditor : Editor
{
    private Boundary boundary;
    private int selectedPointIndex = -1;

    private void OnEnable()
    {
        boundary = (Boundary)target;
    }

    private void OnSceneGUI()
    {
        foreach (var spawnPoint in boundary.GetComponentsInChildren<SpawnPoint>())
        {
            Handles.color = Color.green;
            Handles.DrawSolidDisc(spawnPoint.transform.position, Vector3.up, HandleUtility.GetHandleSize(spawnPoint.transform.position) / 10);
        }
        if (boundary == null) return;
        if (Application.isPlaying)
        {
            DrawOnly(Color.white);
            return;
        }

        Event e = Event.current;
        int bestIndex = -1;
        Color boundaryColor = boundary.isBaked ? Color.cyan : Color.red;

        for (int i = 0; i < boundary.GetPointsCount(); i++)
        {


            Vector2 point2D = boundary.GetPoint(i);
            Vector3 point3D = new Vector3(point2D.x, 0f, point2D.y);
            float size = HandleUtility.GetHandleSize(point3D) * boundary.handleSize;
            Handles.color = (i == selectedPointIndex) ? Color.green : boundaryColor;

            int id = GUIUtility.GetControlID(FocusType.Passive);

            if (e.type == EventType.Layout)
                HandleUtility.AddControl(id, HandleUtility.DistanceToCircle(point3D, size));

            if (HandleUtility.nearestControl == id)
                bestIndex = i;

            if (i == selectedPointIndex)
            {
                EditorGUI.BeginChangeCheck();

                Vector3 newWorld = Handles.FreeMoveHandle(
                    point3D,
                    size,
                    Vector3.zero,
                    Handles.SphereHandleCap
                );
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(boundary, "Move Point");
                    boundary.SetPoint(new Vector2(newWorld.x, newWorld.z), i);
                    boundary.isBaked = false;
                }
            }
            else
            {
                Handles.SphereHandleCap(0, point3D, Quaternion.identity, size, EventType.Repaint);
            }
        }
        DrawOnly(boundaryColor);
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.alt && Event.current.shift)
        {
            Vector3 world = GetMouseWorld();
            Vector2 world2D = new Vector2(world.x, world.z);

            bool inside = boundary.IsInBoundary(world2D);
            if (inside)
            {
                Debug.DrawRay(world, Vector3.up * 100, Color.green);
            } else
            {
                Debug.DrawRay(world, Vector3.up * 100, Color.red);
            }

            Debug.Log(inside ? "Inside" : "Outside");

            Event.current.Use();
        }
        HandleAddPoint();
        HandleDeletePoint();
        HandleSelectPoint(bestIndex);
    }
    private void DrawOnly(Color drawColor)
    {
        Handles.color = drawColor;
        for (int i = 0; i < boundary.GetPointsCount(); i++)
        {
            Vector2 a = boundary.GetPoint(i);
            Vector2 b = boundary.GetPoint((i + 1) % boundary.GetPointsCount());

            Handles.DrawLine(
                new Vector3(a.x, 0, a.y),
                new Vector3(b.x, 0, b.y)
            );
            Handles.DrawWireCube(new Vector3(boundary.Centroid.x, 0f, boundary.Centroid.y), new Vector3(5,5,5));
            Handles.DrawWireDisc(new Vector3(boundary.Centroid.x, 0f, boundary.Centroid.y), Vector3.up, boundary.MaxDist);
        }
    }
    private void HandleAddPoint()
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0 && e.shift)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Undo.RecordObject(boundary, "Add Boundary Point");
                boundary.AddPoint(new Vector2(hit.point.x, hit.point.z));
                boundary.isBaked = false;
                selectedPointIndex = boundary.GetPointsCount() - 1;
                e.Use();
            }
        }
    }

    private Vector3 GetMouseWorld()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (plane.Raycast(ray, out float dist))
            return ray.GetPoint(dist);

        return Vector3.zero;
    }
    private void HandleDeletePoint()
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown && e.alt && e.shift && e.capsLock)
        {
            if (selectedPointIndex >= 0)
            {
                Undo.RecordObject(boundary, "Delete Point");
                boundary.RemovePoint(selectedPointIndex);
                boundary.isBaked = false;
                selectedPointIndex = -1;
            }
            e.Use();
        }
    }
    private void HandleSelectPoint(int bestIndex)
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt && !e.shift)
        {
            if (selectedPointIndex == bestIndex)
            {
                selectedPointIndex = -1;
            } else
            {
                selectedPointIndex = bestIndex;
            }
            e.Use();

        }
    }

}

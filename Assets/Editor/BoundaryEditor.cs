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
        Handles.color = Color.cyan;
        Event e = Event.current;
        int bestIndex = -1;
        Color boundaryColor = boundary.IsBaked ? Color.cyan : Color.red;

        for (int i = 0; i < boundary.points.Count; i++)
        {
            Vector2 point = boundary.points[i];
            Vector3 local3D = new Vector3(point.x, 0f, point.y);
            Vector3 worldPoint = boundary.transform.TransformPoint(local3D);
            float size = HandleUtility.GetHandleSize(worldPoint) * boundary.handleSize;
            Handles.color = (i == selectedPointIndex) ? Color.green : boundaryColor;

            int id = GUIUtility.GetControlID(FocusType.Passive);

            if (e.type == EventType.Layout)
                HandleUtility.AddControl(id, HandleUtility.DistanceToCircle(worldPoint, size));

            if (HandleUtility.nearestControl == id)
                bestIndex = i;

            if (i == selectedPointIndex)
            {
                EditorGUI.BeginChangeCheck();

                Vector3 newWorld = Handles.FreeMoveHandle(
                    worldPoint,
                    size,
                    Vector3.zero,
                    Handles.SphereHandleCap
                );
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(boundary, "Move Point");
                    Vector3 newLocal = boundary.transform.InverseTransformPoint(newWorld);
                    boundary.points[i] = new Vector2(newLocal.x, newLocal.z);
                }
            } else
            {
                Handles.SphereHandleCap(0, worldPoint, Quaternion.identity, size, EventType.Repaint);
            }
            Handles.color = boundaryColor;
            if (i > 0)
            {
                Vector2 prev = boundary.points[i - 1];
                Vector3 prevLocal3 = new Vector3(prev.x, 0f, prev.y);
                Handles.DrawLine(
                    boundary.transform.TransformPoint(prevLocal3),
                    worldPoint
                );
            }
        }

        if (boundary.points.Count > 2)
        {
            Vector2 first = boundary.points[0];
            Vector2 last = boundary.points[boundary.points.Count - 1];

            Vector3 localFirst3D = new Vector3(first.x, 0f, first.y);
            Vector3 localLast3D = new Vector3(last.x, 0f, last.y);
            Vector3 worldFirst3D = boundary.transform.TransformPoint(localFirst3D);
            Vector3 worldLast3D = boundary.transform.TransformPoint(localLast3D);
            Handles.DrawLine(worldFirst3D, worldLast3D);
        }
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.alt && Event.current.shift)
        {
            Vector3 world = GetMouseWorld();
            Vector2 world2 = new Vector2(world.x, world.z);

            bool inside = boundary.IsInBoundary(world2);
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

    private void HandleAddPoint()
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0 && e.shift)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Undo.RecordObject(boundary, "Add Boundary Point");
                Vector3 local = boundary.transform.InverseTransformPoint(hit.point);
                boundary.points.Add(new Vector2(local.x, local.z));
                selectedPointIndex = boundary.points.Count - 1;
                e.Use();
            }
        }
    }
    private Vector3 GetMouseWorld()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        Plane plane = new Plane(
            boundary.transform.up, 
            boundary.transform.position 
        );

        if (plane.Raycast(ray, out float distance))
            return ray.GetPoint(distance);

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
                boundary.points.RemoveAt(selectedPointIndex);
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

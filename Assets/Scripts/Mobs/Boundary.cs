using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Boundary : MonoBehaviour
{
    public List<Vector2> points = new List<Vector2>();

    [SerializeField] private List<Vector2> bakedPoints = new();
    [SerializeField] private Vector2 centroid;
    [SerializeField] private bool isBaked;

    public static float EPSILON = 0.0001f;

    public float handleSize = 0.15f;

    public bool IsBaked => isBaked;

    public Vector2 Centroid => centroid;
    public void BakePoints()
    {
        bakedPoints.Clear();
        Debug.Log("BAKED");
        if (points == null || points.Count == 0)
        {
            Debug.LogWarning("Boundary has no points to bake");
            isBaked = false;
            return;
        }
        for (int i = 0; i < points.Count; i++) {
           Vector3 local3D = new Vector3(points[i].x, 0f, points[i].y);
           Vector3 world3D = this.transform.TransformPoint(local3D);
           bakedPoints.Add(new Vector2(world3D.x, world3D.z));
        }
        CreateCentroid();
        isBaked = true;
    }

    public void PrintBoundary()
    {
        foreach (var point in bakedPoints)
        {
            Debug.Log(point);
        }
    }
    public bool IsInBoundary(Vector2 point)
    {
        if (points == null || points.Count < 3)
            return false;
        int windingCount = 0;
        for (int i = 0; i < bakedPoints.Count; i++)
        {
            Vector2 a = bakedPoints[i];
            Vector2 b = bakedPoints[(i + 1) % bakedPoints.Count];
            if (a.y <= point.y)
            {
                if (b.y > point.y && IsLeft(a, b, point) > 0)
                    ++windingCount;
            } else
            {
                if (b.y <= point.y)
                    if (IsLeft(a, b, point) < 0)
                        --windingCount;
            }
        }
        return (windingCount != 0);
    }
    private static int IsLeft(Vector2 a, Vector2 b, Vector2 p)
    {
        float cross = (b.x - a.x) * (p.y - a.y) - (p.x - a.x) * (b.y - a.y);
        if (cross < -EPSILON)
        {
            return -1;
        } else if (cross > EPSILON) {
            return 1;
        } else
        {
            return 0;
        }

    }
    private void CreateCentroid()
    {
        if (bakedPoints.Count == 0)
        {
            centroid = Vector2.zero;
            return;
        }
        float cx = 0;
        float cy = 0;
        foreach  (var p in bakedPoints)
        {
            cx += p.x;
            cy += p.y;
        }
        centroid = new Vector2(cx / bakedPoints.Count, cy / bakedPoints.Count);
    }
}

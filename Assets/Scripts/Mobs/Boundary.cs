using System.Collections.Generic;
using UnityEngine;

public class Boundary : MonoBehaviour
{
    [SerializeField]
    private List<Vector2> points = new();

    [SerializeField] private List<Vector2> bakedPoints = new();
    [SerializeField] private Vector2 centroid;

    public void SetPoint(Vector2 point, int index) => points[index] = point;

    public Vector2 GetPoint(int index) => points[index];

    public int GetPointsCount() => points.Count;

    public void AddPoint(Vector2 point) => points.Add(point);

    public void RemovePoint(int index) => points.RemoveAt(index);

    public void ClearPoints() => points.Clear();


    public static float EPSILON = 0.0001f;

    public float handleSize = 0.15f;

    public bool isBaked;
    public Vector2 Centroid => centroid;

    [SerializeField]
    private float maxDist;
    public float MaxDist => maxDist;
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
        bakedPoints = new List<Vector2>(points);
        CreateCentroidAndMaxDist();
        isBaked = true;
    }
    private void OnDrawGizmos()
    {
        if (points == null || points.Count < 2)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 a = points[i];
            Vector2 b = points[(i + 1) % points.Count];

            Gizmos.DrawLine(new Vector3(a.x, 0, a.y),
                            new Vector3(b.x, 0, b.y));
        }
    }
    public void ConvertFromBaked()
    {
        if (bakedPoints == null || bakedPoints.Count == 0) return;
        points = new List<Vector2>(bakedPoints);
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
    private void CreateCentroidAndMaxDist()
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

        maxDist = 0f;
        foreach (var point in bakedPoints)
        {
            maxDist = Mathf.Max(Vector2.Distance(point, centroid), maxDist);
        }
    }
}

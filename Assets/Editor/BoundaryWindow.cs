using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using Sirenix.OdinInspector.Editor.Internal;

public class BoundaryWindow : EditorWindow
{
    private Boundary boundary;
    private string boundaryCheckResult = "";


    [MenuItem("Window/Boundary")]
    public static void ShowWindow()
    {
        GetWindow<BoundaryWindow>("Boundary Editor");
    }
    private void OnEnable()
    {
        Selection.selectionChanged += OnSelectionChanged;

        AssignBoundary();

    }
    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
    }
    private void OnGUI()
    {
        GUILayout.Label("Test", EditorStyles.boldLabel);
        boundary = (Boundary)EditorGUILayout.ObjectField(boundary, typeof(Boundary), true);

        if (boundary == null)
        {
            return;
        }
        if (GUILayout.Button("Clear points"))
        {
            Undo.RecordObject(boundary, "Clear All Points");
            boundary.ClearPoints();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Bake boundary"))
        {
            Undo.RecordObject(boundary, "Bake boundary");
            boundary.BakePoints();
            boundary.PrintBoundary();
        }
        if (GUILayout.Button("Revert To Bake"))
        {
            Undo.RecordObject(boundary, "Revert to bake");
            boundary.ConvertFromBaked();
            SceneView.RepaintAll();
        }
    }

    private void OnSelectionChanged()
    {
        AssignBoundary();
    }
    private void AssignBoundary()
    {
        if (Selection.activeGameObject == null)
        {
            boundary = null;
            Close();
            Repaint();
            return;
        }
        Boundary b = Selection.activeGameObject.GetComponent<Boundary>();
        if (b != null)
        {
            boundary = b;
            Focus();
            Repaint();
        }
        else
        {
            boundary = null;
            Close();
            Repaint();
            return;
        }
    }
}

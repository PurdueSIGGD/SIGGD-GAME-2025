using UnityEngine;
using UnityEditor;
[InitializeOnLoad]
public static class AutoOpenBoundaryWindow
{
    static AutoOpenBoundaryWindow()
    {
        Selection.selectionChanged += OnSelectionChanged;
    }
    private static void OnSelectionChanged()
    {
        if (Selection.activeGameObject == null)
            return;

        Boundary boundary = Selection.activeGameObject.GetComponent<Boundary>();

        if (boundary != null)
        {
            BoundaryWindow.ShowWindow();
        }
    }
}

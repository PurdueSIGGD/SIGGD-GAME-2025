using ProceduralAnimation.Runtime;
using UnityEditor;
using UnityEngine;

namespace ProceduralAnimation.Editor {
    [CustomEditor(typeof(SecondOrderSettings))]
    public class SecondOrderSettingsEditor : UnityEditor.Editor {
        public static int steps = 1000;
        public static float assumedTimeElapsed = 10f;

        SerializedProperty f, z, r;
        ProcAnimEditorUtils.SecondOrderFloatSystem filter;

        public void OnEnable() {
            f = serializedObject.FindProperty("f");
            z = serializedObject.FindProperty("z");
            r = serializedObject.FindProperty("r");

            filter = new ProcAnimEditorUtils.SecondOrderFloatSystem(f.floatValue, z.floatValue, r.floatValue, ProcAnimEditorUtils.F(0));
        }

        public override void OnInspectorGUI() {
            //  Create base unity's inspector
            serializedObject.Update();

            EditorGUILayout.PropertyField(f, new GUIContent("Frequency", "How quick the system responds to changes."));
            EditorGUILayout.PropertyField(z, new GUIContent("Damping", "The damping of the system. If 1 > z > 0, the system can overshoot. If z > 1, it will not overshoot."));
            EditorGUILayout.PropertyField(r, new GUIContent("Reponse", "The initial response of the system. If r is negative, the system will try and anticipate movements. If r is positive, it will overshoot then compensate."));

            serializedObject.ApplyModifiedProperties();

            //  Create Graph background
            //      For Graph Outline
            Rect outline = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(200), GUILayout.ExpandWidth(true));
            Rect rect = outline;

            //      Add a one pixel outline
            rect.xMin++;
            rect.xMax--;
            rect.yMin++;
            rect.yMax--;

            //      Draw rects, outline first then overlap with actual rect
            EditorGUI.DrawRect(outline, ProcAnimEditorUtils.Grey(24f));
            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? ProcAnimEditorUtils.Grey(36f) : ProcAnimEditorUtils.Grey(237f));

            //  Start Drawing Graph
            Handles.BeginGUI();
            Handles.color = Color.cyan;

            //  Calculate K values
            filter.ComputeKValues(f.floatValue, z.floatValue, r.floatValue);

            //  Iterate over an amount of steps and generate the non-filtered graph
            Vector3[] points = new Vector3[steps];
            Vector3[] interpolatedPoints = new Vector3[steps];
            for (int i = 0; i < steps; i++) {
                float x = i * 1000f / steps;

                float T = assumedTimeElapsed / steps;
                float interpolatedY = filter.Update(T, ProcAnimEditorUtils.F(x));

                points[i] = ProcAnimEditorUtils.FitInRect(new Vector2(x, ProcAnimEditorUtils.F(x)), rect, 0f, 1000f, -rect.height / 2);
                interpolatedPoints[i] = ProcAnimEditorUtils.FitInRect(new Vector2(x, interpolatedY), rect, 0f, 1000f, -rect.height / 2);
            }

            //  Draw line using points
            Handles.DrawAAPolyLine(4, points);

            //  Draw interpolated line
            Handles.color = Color.yellow;
            Handles.DrawAAPolyLine(4, interpolatedPoints);

            Rect buttonRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(18), GUILayout.ExpandWidth(true));

            if (GUI.Button(buttonRect, "Enlarge Graph"))
                SecondOrderGraphWindow.Open((SecondOrderSettings) target);
        }
    }
}
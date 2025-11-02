using ProceduralAnimation.Runtime;
using UnityEditor;
using UnityEngine;

namespace ProceduralAnimation.Editor {
    /// <summary>
    /// Custom editor for the scriptable object "SecondOrderSettings" which controls the f, z, r values of a second order system.
    /// Has float inputs for each value and shows a graph that can be popped out using the class "SecondOrderGraphWindow".
    /// </summary>
    [CustomEditor(typeof(SecondOrderSettings))]
    public class SecondOrderSettingsEditor : UnityEditor.Editor {
        public static int steps = 1000; //  How many x values there are over an interval of [0, 1000]
        public static float assumedTimeElapsed = 10f; //    How long it has been to go from x = 0 to x = 1000
        SerializedProperty f, z, r; //  The references to the scriptable object's input fields
        ProcAnimEditorUtils.SecondOrderFloatSystem filter; //   The filter to get interpolated versions from

        /// <summary>
        /// Initialize parameters
        /// </summary>
        public void OnEnable() {
            //  Get references to scriptable objects' input fields
            f = serializedObject.FindProperty("f");
            z = serializedObject.FindProperty("z");
            r = serializedObject.FindProperty("r");

            //  Create instance of filter
            filter = new ProcAnimEditorUtils.SecondOrderFloatSystem(f.floatValue, z.floatValue, r.floatValue, ProcAnimEditorUtils.F(0));
        }

        /// <summary>
        /// Essentially the update loop where it runs the math and the graph display.
        /// </summary>
        public override void OnInspectorGUI() {
            //  Create float inputs
            //      Check for changes
            serializedObject.Update();

            //      Create float fields
            EditorGUILayout.PropertyField(f, new GUIContent("Frequency", "How quick the system responds to changes."));
            EditorGUILayout.PropertyField(z, new GUIContent("Damping", "The damping of the system. If 1 > z > 0, the system can overshoot. If z > 1, it will not overshoot."));
            EditorGUILayout.PropertyField(r, new GUIContent("Reponse", "The initial response of the system. If r is negative, the system will try and anticipate movements. If r is positive, it will overshoot then compensate."));

            //      Apply changes
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

            //  Calculate K values
            filter.ComputeKValues(f.floatValue, z.floatValue, r.floatValue);

            //  Iterate over an amount of steps and generate the non-filtered graph
            Vector3[] points = new Vector3[steps];
            Vector3[] interpolatedPoints = new Vector3[steps];

            //  Time step doesn't change
            float T = assumedTimeElapsed / steps;

            for (int i = 0; i < steps; i++) {
                //  Calculate x by inverse lerping using i from 0 to 1000 * steps
                float x = i * 1000f / steps;

                //  The value returned by the filter
                float interpolatedY = filter.Update(T, ProcAnimEditorUtils.F(x));

                //  Write to arrays for drawing
                points[i] = ProcAnimEditorUtils.FitInRect(new Vector2(x, ProcAnimEditorUtils.F(x)), rect, 0f, 1000f, -rect.height / 2);
                interpolatedPoints[i] = ProcAnimEditorUtils.FitInRect(new Vector2(x, interpolatedY), rect, 0f, 1000f, -rect.height / 2);
            }

            //  Start Graph drawing
            Handles.BeginGUI();

            //  Draw line using points            
            Handles.color = Color.cyan;
            Handles.DrawAAPolyLine(4, points);

            //  Draw interpolated line
            Handles.color = Color.yellow;
            Handles.DrawAAPolyLine(4, interpolatedPoints);

            //  End Graph drawing
            Handles.EndGUI();

            //  Create button to pop out graph
            Rect buttonRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(18), GUILayout.ExpandWidth(true));

            if (GUI.Button(buttonRect, "Enlarge Graph"))
                SecondOrderGraphWindow.Open((SecondOrderSettings) target);
        }
    }
}
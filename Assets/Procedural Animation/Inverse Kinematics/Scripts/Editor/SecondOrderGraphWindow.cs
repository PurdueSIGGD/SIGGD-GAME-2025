#if UNITY_EDITOR
using ProceduralAnimation.Runtime;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace ProceduralAnimation.Editor {
    public class SecondOrderGraphWindow : OdinEditorWindow {
        SecondOrderSettings settings;
        ProcAnimEditorUtils.SecondOrderFloatSystem filter;
        float f, z, r;

        /// <summary>
        /// Initialization, works similarly to OnEnable() in "SecondOrderSettingsEditor".
        /// </summary>
        /// <param name="_settings">The settings to show the graph as well as tweak inside the window.</param>
        public static void Open(SecondOrderSettings _settings) {
            SecondOrderGraphWindow window = GetWindow<SecondOrderGraphWindow>("Second Order Graph");

            window.settings = _settings;
            window.filter = new ProcAnimEditorUtils.SecondOrderFloatSystem(window.settings, ProcAnimEditorUtils.F(0));

            window.f = window.settings.f;
            window.z = window.settings.z;
            window.r = window.settings.r;
        }

        protected override void DrawEditors() {
            //  If no graph selected show error message.
            if (settings == null || filter == null) {
                GUILayout.Label(
                    new GUIContent("No graph selected. Please click \"Enlarge Graph\" to view a graph."),
                    SirenixGUIStyles.LabelCentered, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                return;
            }

            //  Create Graph background
            //      For Graph Outline
            Rect outline = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(position.height - 90), GUILayout.ExpandWidth(true));
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
            filter.ComputeKValues(f, z, r);

            //  Iterate over an amount of steps and generate the non-filtered graph
            Vector3[] points = new Vector3[SecondOrderSettingsEditor.steps];
            Vector3[] interpolatedPoints = new Vector3[SecondOrderSettingsEditor.steps];
            for (int i = 0; i < SecondOrderSettingsEditor.steps; i++) {
                float x = i * 1000f / SecondOrderSettingsEditor.steps;

                float T = SecondOrderSettingsEditor.assumedTimeElapsed / SecondOrderSettingsEditor.steps;
                float interpolatedY = filter.Update(T, ProcAnimEditorUtils.F(x));

                points[i] = ProcAnimEditorUtils.FitInRect(new Vector2(x, ProcAnimEditorUtils.F(x)), rect, 0f, 1000f, -rect.height / 2);
                interpolatedPoints[i] = ProcAnimEditorUtils.FitInRect(new Vector2(x, interpolatedY), rect, 0f, 1000f, -rect.height / 2);
            }

            //  Begin Graph drawing
            Handles.BeginGUI();

            //  Draw line using points
            Handles.color = Color.cyan;
            Handles.DrawAAPolyLine(4, points);

            //  Draw interpolated line
            Handles.color = Color.yellow;
            Handles.DrawAAPolyLine(4, interpolatedPoints);

            //  End Graph drawing
            Handles.EndGUI();

            //  Create float fields
            f = EditorGUILayout.FloatField("Frequency", f, GUILayout.ExpandWidth(true));
            z = EditorGUILayout.FloatField("Damping", z, GUILayout.ExpandWidth(true));
            r = EditorGUILayout.FloatField("Response", r, GUILayout.ExpandWidth(true));

            //  Create save button
            Rect buttonRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(18), GUILayout.ExpandWidth(true));

            //  Disable button if there aren't any changes
            GUI.enabled = settings.f != f || settings.r != r || settings.z != z;
            if (GUI.Button(buttonRect, "Save Changes")) {
                //  Apply changes
                settings.f = f;
                settings.z = z;
                settings.r = r;
            }
            //  Reenable GUI so the float fields don't get greyed out
            GUI.enabled = true;
        }
    }
}
#endif

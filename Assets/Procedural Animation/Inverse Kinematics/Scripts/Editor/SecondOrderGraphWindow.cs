using ProceduralAnimation.Runtime;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace ProceduralAnimation.Editor {
    public class SecondOrderGraphWindow : OdinEditorWindow {
        SecondOrderSettings settings;
        ProcAnimEditorUtils.SecondOrderFloatSystem filter;
        public static void Open(SecondOrderSettings _settings) {
            SecondOrderGraphWindow window = GetWindow<SecondOrderGraphWindow>("Second Order Graph");

            window.settings = _settings;
            window.filter = new ProcAnimEditorUtils.SecondOrderFloatSystem(window.settings, ProcAnimEditorUtils.F(0));
        }

        protected new void OnGUI() {
            //  If no graph selected show error message.
            if (settings == null || filter == null) {
                GUILayout.Label(
                    new GUIContent("No graph selected. Please click \"Enlarge Graph\" to view a graph."),
                    SirenixGUIStyles.LabelCentered, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                return;
            }

            GUI.enabled = true;
            GUIUtility.keyboardControl = 0;

            //  Create Graph background
            //      For Graph Outline
            Rect outline = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(position.height), GUILayout.ExpandWidth(true));
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
            filter.ComputeKValues(settings.f, settings.z, settings.r);

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

            //  Draw line using points
            Handles.DrawAAPolyLine(4, points);

            //  Draw interpolated line
            Handles.color = Color.yellow;
            Handles.DrawAAPolyLine(4, interpolatedPoints);
        }
    }
}
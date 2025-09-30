using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SecondOrderDemo))]
public class SecondOrderDemoEditor : Editor {
    const int samples = 200;
    const float stepSize = 0.016f;

    public override void OnInspectorGUI() {
        if (GUI.changed)
            Repaint();

        serializedObject.Update();

        // draw your default inspector fields first
        DrawDefaultInspector();

        // grab the component reference
        var follow = (SecondOrderDemo) target;

        // separator
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Step Response Preview", EditorStyles.boldLabel);

        // draw graph area
        Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(150));
        DrawStepGraph(rect, follow.frequency, follow.damping, follow.response);

        serializedObject.ApplyModifiedProperties();
    }

    void DrawStepGraph(Rect rect, float f, float z, float r) {
        // background
        EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f));
        Handles.BeginGUI();
        Handles.color = new Color(1, 1, 1, 0.05f);

        // faint vertical grid
        for (int i = 1; i < 10; i++) {
            float x = Mathf.Lerp(rect.xMin, rect.xMax, i / 10f);
            Handles.DrawLine(new Vector3(x, rect.yMin), new Vector3(x, rect.yMax));
        }

        // target line at 10
        Handles.color = new Color(0, 1, 0, 0.4f);
        float targetY = Mathf.Lerp(rect.yMax, rect.yMin, 10f / 15f);
        Handles.DrawLine(new Vector2(rect.xMin, targetY), new Vector2(rect.xMax, targetY));

        // response curve
        Handles.color = new Color(0.2f, 0.7f, 1f, 1f);
        var sys = new SecondOrderScalar(f, z, r);
        Vector2 prev = Vector2.zero;

        for (int i = 0; i < samples; i++) {
            float y = sys.Step(stepSize, 10f, 0f);
            float nx = Mathf.Lerp(rect.xMin, rect.xMax, i / (samples - 1f));
            float ny = Mathf.Lerp(rect.yMax, rect.yMin, Mathf.Clamp01(y / 15f));
            Vector2 p = new Vector2(nx, ny);
            if (i > 0) Handles.DrawLine(prev, p);
            prev = p;
        }

        Handles.EndGUI();
    }

    // private tiny scalar filter for plotting
    class SecondOrderScalar {
        float k1, k2, k3, y, yd, Tcrit;
        public SecondOrderScalar(float f, float z, float r) {
            k1 = z / (Mathf.PI * f);
            k2 = 1f / Mathf.Pow(2f * Mathf.PI * f, 2f);
            k3 = r * z / (2f * Mathf.PI * f);
            Tcrit = 0.8f * (Mathf.Sqrt(4f * k2 + k1 * k1) - k1);
        }
        public float Step(float T, float x, float xd) {
            int n = Mathf.Max(1, Mathf.CeilToInt(T / Tcrit));
            float h = T / n;
            for (int i = 0; i < n; i++) {
                y += h * yd;
                yd += h * (x + k3 * xd - y - k1 * yd) / k2;
            }
            return y;
        }
    }
}

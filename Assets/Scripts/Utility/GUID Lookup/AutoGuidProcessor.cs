#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;

[InitializeOnLoad]
public static class AutoGuidProcessor
{
    static AutoGuidProcessor()
    {
        EditorApplication.delayCall += ProcessAllAssets;
    }

    private static void ProcessAllAssets()
    {
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");

        foreach (string assetGuid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(assetGuid);
            ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

            if (asset == null) continue;

            bool changed = ApplyGuids(asset);

            if (changed)
            {
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }
        }
    }

    private static bool ApplyGuids(ScriptableObject obj)
    {
        bool updated = false;

        var fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        foreach (var field in fields)
        {
            var attr = field.GetCustomAttribute<AutoGuidAttribute>();
            if (attr == null) continue;
            if (field.FieldType != typeof(string)) continue;

            string current = (string)field.GetValue(obj);

            if (string.IsNullOrEmpty(current))
            {
                string newGuid = System.Guid.NewGuid().ToString();
                field.SetValue(obj, newGuid);
                updated = true;
            }
        }

        return updated;
    }
}
#endif

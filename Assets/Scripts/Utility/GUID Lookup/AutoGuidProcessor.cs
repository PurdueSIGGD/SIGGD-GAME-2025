#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Keeps every [AutoGuid] field filled with a unique value and every GUID registry listing all of its assets.
/// Runs when scripts reload, when a GUID-carrying asset is added, moved or deleted, and before a build.
/// </summary>
[InitializeOnLoad]
public static class AutoGuidProcessor
{
    private static readonly Dictionary<System.Type, List<FieldInfo>> guidFieldCache = new();
    private static bool processQueued;

    static AutoGuidProcessor()
    {
        QueueProcess();
    }

    public static void QueueProcess()
    {
        if (processQueued) return;
        processQueued = true;
        EditorApplication.delayCall += ProcessAllAssets;
    }

    public static void ProcessAllAssets()
    {
        processQueued = false;

        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");

        // Sorted so that when two assets share a GUID (one was duplicated from the other), it is always
        // the same asset that keeps it
        List<string> paths = new(guids.Length);
        foreach (string assetGuid in guids)
        {
            paths.Add(AssetDatabase.GUIDToAssetPath(assetGuid));
        }
        paths.Sort(System.StringComparer.Ordinal);

        HashSet<string> usedGuids = new();
        bool anyChanged = false;

        foreach (string path in paths)
        {
            ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

            if (asset == null) continue;

            if (ApplyGuids(asset, usedGuids))
            {
                EditorUtility.SetDirty(asset);
                anyChanged = true;
            }
        }

        anyChanged |= SyncRegistries();

        if (anyChanged)
        {
            AssetDatabase.SaveAssets();
        }
    }

    /// <summary>
    /// True if assets of this type carry an [AutoGuid] field, including one declared on a base class.
    /// </summary>
    public static bool HasGuidFields(System.Type type)
    {
        return type != null && GetGuidFields(type).Count > 0;
    }

    private static bool ApplyGuids(ScriptableObject obj, HashSet<string> usedGuids)
    {
        bool updated = false;

        foreach (var field in GetGuidFields(obj.GetType()))
        {
            string current = (string)field.GetValue(obj);

            if (string.IsNullOrEmpty(current) || usedGuids.Contains(current))
            {
                if (!string.IsNullOrEmpty(current))
                {
                    Debug.LogWarning($"AutoGuidProcessor: {obj.name} shared its GUID with another asset, assigning it a new one.", obj);
                }

                current = System.Guid.NewGuid().ToString();
                field.SetValue(obj, current);
                updated = true;
            }

            usedGuids.Add(current);
        }

        return updated;
    }

    // Private fields of a base class are not returned by GetFields on the derived type, so walk up the
    // hierarchy. Without this a QuestOutcome never receives the guid declared on QuestObjective.
    private static List<FieldInfo> GetGuidFields(System.Type type)
    {
        if (guidFieldCache.TryGetValue(type, out var cached)) return cached;

        List<FieldInfo> fields = new();
        for (System.Type t = type; t != null && t != typeof(ScriptableObject); t = t.BaseType)
        {
            foreach (var field in t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (field.GetCustomAttribute<AutoGuidAttribute>() == null) continue;
                if (field.FieldType != typeof(string)) continue;
                fields.Add(field);
            }
        }

        guidFieldCache[type] = fields;
        return fields;
    }

    private static bool SyncRegistries()
    {
        bool anyChanged = false;

        foreach (string registryGuid in AssetDatabase.FindAssets("t:GuidRegistryBase"))
        {
            GuidRegistryBase registry = AssetDatabase.LoadAssetAtPath<GuidRegistryBase>(AssetDatabase.GUIDToAssetPath(registryGuid));
            if (registry == null) continue;

            if (registry.EditorSyncItems())
            {
                EditorUtility.SetDirty(registry);
                anyChanged = true;
            }
        }

        return anyChanged;
    }

    private class AssetWatcher : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            // The type of a deleted asset can no longer be read, so any deleted .asset triggers a pass
            foreach (string path in deleted)
            {
                if (path.EndsWith(".asset")) { QueueProcess(); return; }
            }

            foreach (string path in imported)
            {
                if (IsGuidAsset(path)) { QueueProcess(); return; }
            }

            foreach (string path in moved)
            {
                if (IsGuidAsset(path)) { QueueProcess(); return; }
            }
        }

        private static bool IsGuidAsset(string path)
        {
            if (!path.EndsWith(".asset")) return false;
            System.Type type = AssetDatabase.GetMainAssetTypeAtPath(path);
            return type != null && typeof(ScriptableObject).IsAssignableFrom(type) && HasGuidFields(type);
        }
    }

    // A build must never ship with a registry that is missing assets, or saved data cannot be loaded back
    private class BuildCheck : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            ProcessAllAssets();
        }
    }
}
#endif

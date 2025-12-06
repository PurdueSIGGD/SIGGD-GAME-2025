using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Sirenix.OdinInspector;

public class TerrainPrefabReplacer : MonoBehaviour
{
    [Serializable]
    public struct TerrainReplacementSettings
    {
        public Terrain terrain;
        public int prototypeIndexToReplace;
        public GameObject replacementPrefab;
        public Vector3 scaleMultiplier;
    }
    public TerrainReplacementSettings[] terrainSettings;
    public bool clearTreesBeforeReplace = true;

    // Store original trees per terrain
    private Dictionary<Terrain, TreeInstance[]> originalTrees = new Dictionary<Terrain, TreeInstance[]>();

[Button("Replace Trees")]
public void ReplaceTrees()
{
#if UNITY_EDITOR

    foreach (var t in terrainSettings)
    {
        var terrainData = t.terrain.terrainData;
        var instances = terrainData.treeInstances;

        // Save original trees
        if (!originalTrees.ContainsKey(t.terrain))
            originalTrees[t.terrain] = (TreeInstance[])instances.Clone();

        // Copy to iterate
        TreeInstance[] instancesToProcess = (TreeInstance[])instances.Clone();

        Undo.RegisterCompleteObjectUndo(terrainData, "Replace Trees");

        if (clearTreesBeforeReplace)
            terrainData.treeInstances = Array.Empty<TreeInstance>();

        // Create or reuse container in scene
        string containerName = t.terrain.name + "_Trees";
        Transform container = transform.Find(containerName);
        if (container == null)
        {
            GameObject go = new GameObject(containerName);
            go.transform.SetParent(transform, false);
            container = go.transform;
        }
        else
        {
            // Clear previous children
            for (int i = container.childCount - 1; i >= 0; i--)
                DestroyImmediate(container.GetChild(i).gameObject);
        }

        // Instantiate prefabs in the scene root under container
        foreach (var tree in instancesToProcess)
        {
            if (tree.prototypeIndex != t.prototypeIndexToReplace) continue;

            Vector3 worldPos = Vector3.Scale(tree.position, terrainData.size) + t.terrain.transform.position;
            Quaternion rot = Quaternion.Euler(0, tree.rotation * Mathf.Rad2Deg, 0);
            Vector3 scale = Vector3.Scale(new Vector3(tree.widthScale, tree.heightScale, tree.widthScale), t.scaleMultiplier);

            // Use PrefabUtility with scene context
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(t.replacementPrefab, container.gameObject.scene);
            obj.transform.position = worldPos;
            obj.transform.rotation = rot;
            obj.transform.localScale = scale;

            // Parent to container in scene hierarchy
            obj.transform.SetParent(container, true);
        }
    }
#endif
}

    [Button("Reset Trees")]
    public void ResetTrees()
    {
#if UNITY_EDITOR
        // Destroy all containers
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // Restore original trees for all terrains
        foreach (var kvp in originalTrees)
        {
            Terrain t = kvp.Key;
            TreeInstance[] trees = kvp.Value;
            if (t != null && trees != null)
            {
                Undo.RegisterCompleteObjectUndo(t.terrainData, "Reset Trees");
                t.terrainData.treeInstances = trees;
            }
        }

        originalTrees.Clear();
#endif
    }
}

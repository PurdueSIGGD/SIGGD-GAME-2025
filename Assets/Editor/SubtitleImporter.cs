using UnityEngine;
using UnityEditor;
using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.IO;

public class SubtitleImporter : EditorWindow
{
    private string inputText = "";

    [MenuItem("Tools/Subtitle Importer")]
    public static void ShowWindow()
    {
        GetWindow<SubtitleImporter>("Subtitle Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Paste the input text here:");
        inputText = EditorGUILayout.TextArea(inputText, GUILayout.Height(350));

        if (GUILayout.Button("Create Subtitle Objects"))
        {
            ParseAndCreateAssets(inputText);
        }
    }

    void ParseAndCreateAssets(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return;
        }

        // Get Folder Name
        Match folderMatch = Regex.Match(input, @"['‘]([^'’]+)['’]");
        if (!folderMatch.Success)
        {
            return;
        }

        string folderName = folderMatch.Groups[1].Value.Trim();

        string baseFolder = "Assets/ScriptableObjects/AudioLogs";
        string folderPath = $"{baseFolder}/{folderName}";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder(baseFolder, folderName);
        }

        // Split into blocks by ~
        string[] blocks = input.Split(new[] { "~" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string rawBlock in blocks)
        {
            string block = rawBlock.Trim();
            if (string.IsNullOrEmpty(block)) continue;

            // Get Audio Name {Name}
            Match nameMatch = Regex.Match(block, @"\{([^}]+)\}");
            if (!nameMatch.Success) continue;
            string audioLogName = nameMatch.Groups[1].Value.Trim();

            List<AudioLogObject.lineInfo> parsedLines = new List<AudioLogObject.lineInfo>();

            // --- STATE TRACKING ---
            bool currentIsRadio = false;

            string[] lines = block.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();

                // Skip headers, folder names, and bracketed notes [Note]
                if (line.StartsWith("{") || line.StartsWith("‘") || line.StartsWith("'") ||
                    line.StartsWith("[") || string.IsNullOrEmpty(line))
                    continue;

                // This Regex handles BOTH "Full Lines" and "Continuation Lines"
                // Group 1: ID (optional)
                // Group 2: Flag % or $ (optional)
                // Group 3: Speaker Name (optional)
                // Group 4: Dialogue Text (required)
                // Group 5: Timestamp (required)
                Regex lineRegex = new Regex(@"^(?:(\d+)\s*([$%]?)\s*([^:]+):\s*)?[""“'‘]?(.*?)[""”'’]?\s*\\?\s*<(\d{2}:\d{2}\.\d{3})>$");
                Match match = lineRegex.Match(line);

                if (match.Success)
                {
                    string idStr = match.Groups[1].Value;
                    string flag = match.Groups[2].Value;
                    string dialogue = match.Groups[4].Value.Trim();
                    string timeText = match.Groups[5].Value;

                    // Update the radio state ONLY if a new ID or Flag is provided
                    // This allows Mark's line 2 and 3 to inherit the "%" from line 1
                    if (!string.IsNullOrEmpty(idStr) || !string.IsNullOrEmpty(flag))
                    {
                        currentIsRadio = (flag == "%");
                    }

                    if (TimeSpan.TryParseExact(timeText, @"mm\:ss\.fff", null, out TimeSpan ts))
                    {
                        parsedLines.Add(new AudioLogObject.lineInfo
                        {
                            line = dialogue,
                            seconds = (float)ts.TotalSeconds,
                            isFromRadio = currentIsRadio
                        });
                    }
                }
            }

            if (parsedLines.Count > 0)
            {
                // (Reusing your logic to save the asset)
                SaveAndRegisterAsset(folderPath, audioLogName, parsedLines);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", "Subtitle assets created.", "OK");
    }

    private void SaveAndRegisterAsset(string folderPath, string audioLogName, List<AudioLogObject.lineInfo> lines)
    {
        string assetPath = $"{folderPath}/{audioLogName}.asset";

        // Try to load existing asset to keep references intact
        AudioLogObject asset = AssetDatabase.LoadAssetAtPath<AudioLogObject>(assetPath);

        bool isNew = false;
        if (asset == null)
        {
            asset = CreateInstance<AudioLogObject>();
            isNew = true;
        }

        asset.audioName = audioLogName;
        asset.subtitles = lines.ToArray();

        if (isNew)
            AssetDatabase.CreateAsset(asset, assetPath);
        else
            EditorUtility.SetDirty(asset);

        // Update the AudioManager in the current scene
        AddOrUpdateLogInScene(asset);
    }
    private void AddOrUpdateLogInScene(AudioLogObject asset)
    {
        // Find the AudioManager GameObject in the current scene
        GameObject audioManagerGO = GameObject.Find("AudioManager");
        if (audioManagerGO == null)
        {
            Debug.LogError("No AudioManager GameObject found in the scene.");
            return;
        }

        // Find the child AudioLogList under AudioManager
        Transform logListTransform = audioManagerGO.transform.Find("AudioLogList");
        if (logListTransform == null)
        {
            Debug.LogError("AudioLogList child not found under AudioManager in the scene.");
            return;
        }

        // Get the AudioLogManager component on AudioLogList
        AudioLogManager manager = logListTransform.GetComponent<AudioLogManager>();
        if (manager == null)
        {
            Debug.LogError("AudioLogManager component missing on AudioLogList in the scene.");
            return;
        }

        // clean list of Null entries
        manager.logs.RemoveAll(item => (item == null));

        // see if it already exists in the list and if it does replace it with the now updated version
        int index = manager.logs.FindIndex(x => (x.audioName == asset.audioName));
        if (index >= 0)
        {
            manager.logs[index] = asset; // Replace existing
        }
        else
        {
            manager.logs.Add(asset); // Add new
        }

        // Mark the manager as dirty so Unity knows it changed
        EditorUtility.SetDirty(manager);

        Debug.Log($"AudioLogObject '{asset.audioName}' added/updated in scene AudioManager's AudioLogList.");
    }
}

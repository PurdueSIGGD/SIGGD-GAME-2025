using UnityEngine;
using UnityEditor;
using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.IO;

public class SubtitleImporter : EditorWindow
{
    private string inputText = ""; // inputted in the window

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
            Debug.LogError("Empty input.");
            return;
        }

        // ===============================
        // 1. Extract Folder Name
        // ===============================
        Match folderMatch = Regex.Match(input, @"['‘]([^'’]+)['’]");
        if (!folderMatch.Success)
        {
            Debug.LogError("No folder name found.");
            return;
        }

        string folderName = folderMatch.Groups[1].Value.Trim();

        // Remove folder line from input
        input = input.Substring(folderMatch.Index + folderMatch.Length);

        string baseFolder = "Assets/ScriptableObjects/AudioLogs";

        string folderPath = baseFolder + "/" + folderName;

        if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects/AudioLogs", folderName);

        // ===============================
        // 2. Split Into Blocks
        // ===============================
        string[] blocks = input.Split(new[] { "~" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string rawBlock in blocks)
        {
            string block = rawBlock.Trim();
            if (string.IsNullOrEmpty(block))
                continue;

            // ===============================
            // 3. Extract {AudioLog Name}
            // ===============================
            Match nameMatch = Regex.Match(block, @"\{([^}]+)\}");
            if (!nameMatch.Success)
            {
                Debug.LogWarning("Block missing {Name}, skipped.");
                continue;
            }

            string audioLogName = nameMatch.Groups[1].Value.Trim();

            // Remove {Name} from block
            block = block.Substring(nameMatch.Index + nameMatch.Length);

            // ===============================
            // 4. Remove Stage Directions
            // ===============================
            block = Regex.Replace(block, @"\[.*?\]", "");

            // ===============================
            // 5. Merge Continuation Lines
            // ===============================
            List<string> mergedLines = MergeContinuationLines(block);

            List<AudioLogObject.lineInfo> parsedLines = new List<AudioLogObject.lineInfo>();

            // ===============================
            // 6. Parse Each Subtitle Line
            // ===============================
            Regex lineRegex = new Regex(@"\d+\s*([$%]?)\s*[^:]+:\s*""?(.*?)""?\s*<(\d{2}:\d{2}\.\d{3})>", RegexOptions.Compiled);

            foreach (string line in mergedLines)
            {
                Match match = lineRegex.Match(line);
                if (!match.Success)
                    continue;

                string flag = match.Groups[1].Value;
                string dialogue = match.Groups[2].Value.Trim();
                string timeText = match.Groups[3].Value;

                TimeSpan ts = TimeSpan.ParseExact(timeText, @"mm\:ss\.fff", null);

                parsedLines.Add(new AudioLogObject.lineInfo
                {
                    line = dialogue,
                    seconds = (float)ts.TotalSeconds,
                    isFromRadio = flag == "%"
                });
            }

            if (parsedLines.Count == 0)
                continue;

            // ===============================
            // 7. Create ScriptableObject
            // ===============================
            AudioLogObject asset = CreateInstance<AudioLogObject>();
            asset.audioName = audioLogName;
            asset.subtitles = parsedLines.ToArray();

            string assetPath = folderPath + "/" + audioLogName + ".asset";
            AssetDatabase.CreateAsset(asset, assetPath);

            AddOrUpdateLogInScene(asset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", "Subtitle assets created.", "OK");
    }
    List<string> MergeContinuationLines(string block)
    {
        List<string> result = new List<string>();

        string[] lines = block.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        string currentLine = "";

        foreach (string raw in lines)
        {
            string trimmed = raw.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            currentLine += " " + trimmed;

            if (!trimmed.EndsWith("\\"))
            {
                result.Add(currentLine.Replace("\\", "").Trim());
                currentLine = "";
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
            result.Add(currentLine.Replace("\\", "").Trim());

        return result;
    }

    private void AddOrUpdateLogInScene(AudioLogObject asset)
    {
        // Find the AudioManager GameObject in the active scene
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

        // Initialize logs list if null
        if (manager.logs == null)
            manager.logs = new List<AudioLogObject>();

        // Remove any null entries to keep the list clean
        manager.logs.RemoveAll(item => item == null);

        // Add or replace the asset in the list
        int index = manager.logs.FindIndex(x => x.audioName == asset.audioName);
        if (index >= 0)
        {
            manager.logs[index] = asset; // Replace existing
        }
        else
        {
            manager.logs.Add(asset);     // Add new
        }

        // Mark the manager as dirty so Unity knows it changed
        EditorUtility.SetDirty(manager);

        // Optionally mark the scene dirty if you want to prompt saving
        // using UnityEditor.SceneManagement;
        // EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"AudioLogObject '{asset.audioName}' added/updated in scene AudioManager's AudioLogList.");
    }
    private void CreateFolderIfNotExists(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
            return;

        string parent = Path.GetDirectoryName(folderPath).Replace("\\", "/");
        string newFolderName = Path.GetFileName(folderPath);

        if (!AssetDatabase.IsValidFolder(parent))
        {
            // Recursively create parent folders first
            CreateFolderIfNotExists(parent);
        }

        AssetDatabase.CreateFolder(parent, newFolderName);
    }
}

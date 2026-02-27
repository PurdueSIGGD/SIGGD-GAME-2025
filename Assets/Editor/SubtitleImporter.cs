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
            Debug.LogError("Empty input.");
            return;
        }

        // get folder name
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

        // split all the soon to be AudioLogObjects into blocks to parse their lines
        string[] blocks = input.Split(new[] { "~" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string rawBlock in blocks)
        {
            string block = rawBlock.Trim();
            if (string.IsNullOrEmpty(block))
            {
                continue;
            }

            // get the AudioName between {}
            Match nameMatch = Regex.Match(block, @"\{([^}]+)\}");
            if (!nameMatch.Success)
            {
                Debug.LogWarning("Block missing name in {}");
                continue;
            }

            // save the name for later then remove it from the block
            string audioLogName = nameMatch.Groups[1].Value.Trim();
            block = block.Substring(nameMatch.Index + nameMatch.Length);

            // remove anything in between [] as they dont go in subtitles
            block = Regex.Replace(block, @"\[.*?\]", "");

            List<string> mergedLines = MergeContinuationLines(block);

            // temporay storage of lineInfos before we make them into AudioLogObjects
            List<AudioLogObject.lineInfo> parsedLines = new List<AudioLogObject.lineInfo>();

            // this is what each line looks like in Regex so we cna get the %/$ actual content and timestamp in one group
            Regex lineRegex = new Regex(@"^\d+\s*([$%]?)\s*.*?:\s*""?(.*)""?\s*<(\d{2}:\d{2}\.\d{3})>\s*$", RegexOptions.Compiled);

            foreach (string line in mergedLines)
            {
                Match match = lineRegex.Match(line);
                if (!match.Success)
                {
                    continue;
                }

                // % or $ or nothing
                string flag = match.Groups[1].Value;

                // actual text in quotations
                string dialogue = match.Groups[2].Value.Trim();

                // timestamp
                string timeText = match.Groups[3].Value;

                TimeSpan ts = TimeSpan.ParseExact(timeText, @"mm\:ss\.fff", null);

                // add to the holder list
                parsedLines.Add(new AudioLogObject.lineInfo
                {
                    line = dialogue,
                    seconds = (float)ts.TotalSeconds,
                    isFromRadio = (flag == "%")
                });
            }

            if (parsedLines.Count == 0)
            {
                continue;
            }

            // turning all the parsed info into an asset
            AudioLogObject asset = CreateInstance<AudioLogObject>();
            asset.audioName = audioLogName;
            asset.subtitles = parsedLines.ToArray();

            string assetPath = folderPath + "/" + audioLogName + ".asset";
            AssetDatabase.CreateAsset(asset, assetPath);

            // updating the version in the current scene youre in
            AddOrUpdateLogInScene(asset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", "Subtitle assets created.", "OK");
    }

    // takes in a block then merges each line in the block into one list of each line for easier parsing
    private List<string> MergeContinuationLines(string block)
    {
        List<string> result = new List<string>();

        // split on newlines
        string[] lines = block.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        string currentLine = "";

        // loop through all lines
        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            // add lines to current line
            currentLine += " " + line;

            // see if we are at the determined line end \ if not add to the result and go again
            if (!line.EndsWith("\\"))
            {
                result.Add(currentLine.Replace("\\", "").Trim());
                currentLine = "";
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            result.Add(currentLine.Replace("\\", "").Trim());
        }

        return result;
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

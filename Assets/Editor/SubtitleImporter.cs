using UnityEngine;
using UnityEditor;
using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

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
        // make sure there is input
        if (string.IsNullOrEmpty(input))
        {
            Debug.LogError("Empty string inputted");
            return;
        }

        // first we get folder name (inside quotations for now)
        Match folderMatch = Regex.Match(input, "‘([^’]+)’");

        // if a folder name isnt in the input
        if (!folderMatch.Success)
        {
            Debug.LogError("No folder name please input a file with a folder name");
            return;
        }

        string folderName = folderMatch.Groups[1].Value.Trim();

        // removes the first name inputted after we grab the folder name so it doesnt get added into the line outputs
        int lineEnd = input.IndexOf('\n');
        if (lineEnd != -1)
        {
            input = input.Substring(lineEnd + 1).TrimStart();
        }

        // path to the new folder thatll hold all the AudioLogObjects
        string folderPath = "Assets/ScriptableObjects/AudioLogs/" + folderName + "/";

        // if the folder we trying to store these in doesnt exist we make one
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/ScriptableObjects/AudioLogs", folderName);
        }

        string[] splitInput = input.Split(new[] { "-------------------------" }, StringSplitOptions.RemoveEmptyEntries);

        // this dictionary stores the object name and the data that will eventually be used to make an AudioLogObject
        Dictionary<string, List<AudioLogObject.lineInfo>> audioLines = new Dictionary<string, List<AudioLogObject.lineInfo>>();

        foreach (string block in splitInput)
        {
            string[] lines = block.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // another check to make sure we dont parse if the lines is empty
            if (lines.Length == 0)
            {
                continue;
            }

            // temporarily hold the lineInfo while the rest of the line is being parsed before it all gets put into one AudioLogObject
            List<AudioLogObject.lineInfo> parsedLines = new List<AudioLogObject.lineInfo>();

            string name = null;
            string text = "";
            float currentSecLength = 0f;

            foreach (string line in lines)
            {
                string trimmed = line.Trim();

                // make sure its not empty before we parse
                if (string.IsNullOrEmpty(trimmed)) continue;

                // cut out the stage directions
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]")) continue;

                // getting name between {}
                Match nameMatch = Regex.Match(trimmed, @"\{([^}]+)\}");
                if (nameMatch.Success)
                {
                    name = nameMatch.Groups[1].Value.Trim();
                    continue;
                }

                float secLength = -1f; // if it stays -1 then no time is found and itll error
                // getting timestamp between <>
                Match timeMatch = Regex.Match(trimmed, @"<([^>]+)>");
                if (timeMatch.Success)
                {
                    string timeText = timeMatch.Groups[1].Value.Trim();

                    // with the way that the time is formatted its easy to parse
                    TimeSpan ts = TimeSpan.ParseExact(timeText, @"mm\:ss\.fff", null);
                    secLength = (float)ts.TotalSeconds;

                    // Remove timestamp from line text so it doesnt get added to the final line
                    trimmed = Regex.Replace(trimmed, @"<([^>]+)>", "").Trim();
                }

                // checking if there are multiple lines broken up by \
                bool continuing = trimmed.EndsWith("\\");
                if (continuing)
                {
                    trimmed = trimmed.TrimEnd('\\').Trim();
                }

                // if a time was found
                if (secLength > 0)
                {
                    // make sure text has content
                    if (!string.IsNullOrEmpty(text))
                    {
                        // store the lineInfo in the list until we make the scriptable objects later
                        parsedLines.Add(new AudioLogObject.lineInfo
                        {
                            line = text.Trim(),
                            seconds = currentSecLength
                        });
                    }

                    text = "";
                    currentSecLength = secLength;
                }

                // adding trimmed text to the current line
                if (text.Length > 0)
                {
                    text += " ";
                }

                text += trimmed;
                /*
                // the line doesnt end wtih \ so we add it to the list
                if (!continuing && secLength > -1f && secLength != 0)
                {
                    parsedLines.Add(new AudioLogObject.lineInfo
                    {
                        line = text.Trim(),
                        seconds = currentSecLength
                    });

                    // reset for next line
                    text = "";
                    currentSecLength = 0f;
                }*/
            }

            // if nothing was parsed skip the rest of the loop
            if (parsedLines.Count == 0) continue;

            // error checking for if a line is just a unicode character and doesnt have any real content
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning("Block has no {Name} and is being skipped");
                continue;
            }

            // adding a new entry if the entry at the key doesnt exist
            if (!audioLines.ContainsKey(name))
                audioLines[name] = new List<AudioLogObject.lineInfo>();

            // add all values from parsedLines to the dictionary
            audioLines[name].AddRange(parsedLines);
        }

        // now we loop through the dictionary and turn each entry into a real AudioLogObject
        foreach (var entry in audioLines)
        {
            AudioLogObject asset = ScriptableObject.CreateInstance<AudioLogObject>();

            asset.audioName = entry.Key;
            asset.subtitles = entry.Value.ToArray();

            string assetPath = folderPath + entry.Key + ".asset";
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", "All subtitle ScriptableObjects created.", "OK");
    }
}

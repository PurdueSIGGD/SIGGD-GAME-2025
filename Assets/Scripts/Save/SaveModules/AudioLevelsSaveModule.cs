using System.Collections.Generic;
using FMODUnity;
using Sirenix.Serialization;

public class AudioLevelsSaveModule : ISaveModule
{
    private readonly string savePath = $"{FileManager.savesDirectory}/audioLevels";
    private readonly string[] vcaNames =
    {
        "Master", "Music", "Ambience", "SFX", "Voicelines"
    };

    public bool serialize()
    {
        Dictionary<string, float> audioLevels = new();
        foreach (string vcaName in vcaNames)
        {
            RuntimeManager.GetVCA("vca:/" + vcaName).getVolume(out float volume);
            audioLevels[vcaName] = volume;
        }

        AudioLevelsSaveData saveData = new()
        {
            audioLevels = audioLevels
        };

        byte[] bytes = SerializationUtility.SerializeValue(saveData, DataFormat.Binary);
        FileManager.Instance.WriteFile(savePath, bytes);

        return true;
    }

    public bool deserialize()
    {
        if (!FileManager.Instance.FileExists(savePath)) return false;

        byte[] bytes = FileManager.Instance.ReadFile(savePath);
        var saveData = SerializationUtility.DeserializeValue<AudioLevelsSaveData>(bytes, DataFormat.Binary);

        foreach (string vcaName in vcaNames)
        {
            RuntimeManager.GetVCA("vca:/" + vcaName).setVolume(saveData.audioLevels[vcaName]);
        }

        return true;
    }

    public void ResetAudioLevels()
    {
        foreach (string vcaName in vcaNames)
        {
            RuntimeManager.GetVCA("vca:/" + vcaName).setVolume(1.0f);
        }

        // To write the new audio levels to the file
        serialize();
        // We don't need to call `deserialize` because the foreach loop already played the role of `deserialize`
    }
}

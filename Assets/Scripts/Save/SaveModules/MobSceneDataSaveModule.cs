using MobCensus;
using Sirenix.Serialization;
using System.Collections.Generic;

public class MobSceneDataSaveModule : ISaveModule
{
    private readonly string savePath = $"{FileManager.savesDirectory}/mobSceneData";
    private MobCensusManager mobCensusManager;
    public MobSceneDataSaveModule(MobCensusManager mobCensusManager)
    {
        this.mobCensusManager = mobCensusManager;
    }

    public bool deserialize()
    {
        if (mobCensusManager == null) return false;

        if (!FileManager.Instance.FileExists(savePath)) return false;
        byte[] bytes = FileManager.Instance.ReadFile(savePath);

        List<MobCitizenDataRaw> rawDataList = SerializationUtility.DeserializeValue<List<MobCitizenDataRaw>>(bytes, DataFormat.Binary);
        mobCensusManager.LoadRawDataFromSave(rawDataList);
        return true;
    }

    public bool serialize()
    {
        if (mobCensusManager == null) return false;

        List<MobCitizenData> citizens = mobCensusManager.GetCitizens();
        List<MobCitizenDataRaw> rawDataList = new List<MobCitizenDataRaw>();
        foreach (MobCitizenData citizen in citizens)
        {
            rawDataList.Add(citizen.GetRawDataReference());
        }

        byte[] bytes = SerializationUtility.SerializeValue(rawDataList, DataFormat.Binary);
        FileManager.Instance.WriteFile(savePath, bytes);

        return true;
    }
}
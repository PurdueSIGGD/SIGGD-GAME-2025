using MobCensus;
using Sirenix.Serialization;
using System.Collections.Generic;

public class MobSceneDataSaveModule : ISaveModule
{
    private readonly string mobSavePath = $"{FileManager.savesDirectory}/mobSceneData";
    private readonly string regionSavePath = $"{FileManager.savesDirectory}/spawnRegionSceneData";

    private MobCensusManager mobCensusManager;
    public MobSceneDataSaveModule(MobCensusManager mobCensusManager)
    {
        this.mobCensusManager = mobCensusManager;
    }

    public bool deserialize()
    {
        if (mobCensusManager == null) return false;

        if (FileManager.Instance.FileExists(regionSavePath))
        {
            byte[] regionBytes = FileManager.Instance.ReadFile(regionSavePath);
            List<MobRegionDataRaw> rawRegionList = SerializationUtility.DeserializeValue<List<MobRegionDataRaw>>(regionBytes, DataFormat.Binary);
            mobCensusManager.InitializeSpawnRegionsFromSave(rawRegionList);
        }
        else
        {
            mobCensusManager.InitializeSpawnRegionsForNewGame();
        }

        if (FileManager.Instance.FileExists(mobSavePath))
        {
            byte[] bytes = FileManager.Instance.ReadFile(mobSavePath);
            List<MobCitizenDataRaw> rawDataList = SerializationUtility.DeserializeValue<List<MobCitizenDataRaw>>(bytes, DataFormat.Binary);
            mobCensusManager.LoadRawDataFromSave(rawDataList);
        }

        return true;
    }

    public bool serialize()
    {
        if (mobCensusManager == null) return false;

        List<MobCitizenData> citizens = mobCensusManager.GetCitizens();
        List<MobCitizenDataRaw> rawDataList = new List<MobCitizenDataRaw>();
        foreach (MobCitizenData citizen in citizens)
        {
            citizen.UpdateRawData();
            rawDataList.Add(citizen.GetRawData());
        }
        List<MobRegionData> regions = mobCensusManager.GetRegions();
        List<MobRegionDataRaw> regionsRaw = new List<MobRegionDataRaw>();
        foreach (MobRegionData region in regions)
        {
            region.UpdateRawData();
            regionsRaw.Add(region.GetRawData());
        }

        byte[] bytes = SerializationUtility.SerializeValue(rawDataList, DataFormat.Binary);
        FileManager.Instance.WriteFile(mobSavePath, bytes);
        byte[] regionBytes = SerializationUtility.SerializeValue(regionsRaw, DataFormat.Binary);
        FileManager.Instance.WriteFile(regionSavePath, regionBytes);

        return true;
    }
}
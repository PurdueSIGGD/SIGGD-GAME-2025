using MobCensus;
using Sirenix.Serialization;
using System.Collections.Generic;

/// <summary>
/// Depreciated, I know it may still referenced in save manager but it doesn't do anything.
/// I crippled the deserialize and serialize functions.
/// </summary>
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
        return false;
    }

    public bool serialize()
    {
        return false;
    }
}
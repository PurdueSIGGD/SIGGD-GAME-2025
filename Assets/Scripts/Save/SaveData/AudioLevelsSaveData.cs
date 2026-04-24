// Should this be an array or a dictionary?
// If it is an array, then if VCAs are added/deleted in updates, it could cause problems.
// So we'll make it a dictionary.

using System.Collections.Generic;

public class AudioLevelsSaveData
{
    public Dictionary<string, float> audioLevels;
}

using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    public List<ISaveModule> modules = new List<ISaveModule>();
    public void Load()
    {
        foreach (ISaveModule i in modules)
        {
            i.deserialize(null);
        }
    }
    public void Save()
    {
        foreach(ISaveModule i in modules)
        {
            i.serialize(null);
        }
    }
}

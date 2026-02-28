using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public static class LureManager
{
    private static readonly List<Lure> activeLures = new();

    public static IReadOnlyList<Lure> ActiveLures = activeLures;

    public static void RegisterLure(Lure lure)
    {
        if(lure !=  null && !activeLures.Contains(lure))
        {
            activeLures.Add(lure);
        }
    }

    public static void UnregisterLure(Lure lure)
    {
        activeLures.Remove(lure);
    }
}

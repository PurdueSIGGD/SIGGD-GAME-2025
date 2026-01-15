using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scriptable object that holds the info for a crafting recipe
/// </summary>
[CreateAssetMenu(menuName = "Scriptable Objects/Recipe")]
public class Recipe : ScriptableObject
{
    [SerializeField] public ItemInfo output; // the resulting item that is crafted from the recipe
    [SerializeField] public bool craftableOnTheGo; // whether the recipe can be crafted using the on-the-go crafting menu
    [SerializeField] public List<ItemInfo> ingredients; // list of ingredients
    [SerializeField] public List<int> counts; // list of amounts needed for each ingredient

    public bool isUnlocked; // whether the recipe has been unlocked or not
    

}

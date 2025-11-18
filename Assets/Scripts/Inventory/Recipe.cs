using System;
using UnityEngine;

/// <summary>
/// Scriptable object that holds the info for a crafting recipe
/// </summary>
[CreateAssetMenu(menuName = "Scriptable Objects/Recipe")]
public class Recipe : ScriptableObject
{
    [SerializeField] public ItemInfo ingredient1; // one of the items needed for the recipe
    [SerializeField] public int count1; // amount of ingredient1 needed for crafting
    [SerializeField] public ItemInfo ingredient2; // the other item needed for the recipe
    [SerializeField] public int count2; // amount of ingrdient2 needed for crafting
    [SerializeField] public ItemInfo output; // the resulting item that is crafted from the recipe
    [SerializeField] public bool craftableOnTheGo; // whether the recipe can be crafted using the on-the-go crafting menu

    public bool isUnlocked; // whether the recipe has been unlocked or not
    

}

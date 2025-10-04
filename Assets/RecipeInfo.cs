using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using static ItemInfo;
/// <summary>
/// Scriptable Object that holds all of the recipes
/// </summary>
[CreateAssetMenu(menuName = "Scriptable Objects/Recipe Info")]
public class RecipeInfo : ScriptableObject
{
    [SerializeField] ItemInfo Ingredient1; // first ingredient used to craft item
    
    [SerializeField] ItemInfo Ingredient2; // second ingredient used to craft item
    
    [SerializeField] ItemInfo Output; // item that is produced as a result of crafting; should be an existing item in ItemInfo

    public void log()
    {
        Debug.Log("First ingredient: " + Ingredient1);
        Debug.Log("First ingredient: " + Ingredient2);
        Debug.Log("Result: " + Output);
    }
}





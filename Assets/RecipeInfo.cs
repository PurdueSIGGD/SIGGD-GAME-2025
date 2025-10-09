using System.Collections.Generic;
using UnityEngine;
using static ItemInfo;

/// <summary>
/// Singleton object in the scene.
/// </summary>
public class RecipeInfo : MonoBehaviour {
    // No, these combos don't really make any sense.
    private Dictionary<(ItemName, ItemName), ItemName> recipeBook = new() // dictionary containing all possible crafting combos
    {
        { (ItemName.Spear, ItemName.Rock), ItemName.RockSpear },
        { (ItemName.RockSpear, ItemName.RockSpear), ItemName.Empty },
    };

    public ItemInfo UseRecipe(ItemName item1, ItemName item2)
    {
        var key = (item1, item2);
        ItemName name;
        if (!recipeBook.ContainsKey(key))
        {
            // Check the other order?
            var revKey = (item2, item1);
            if (!recipeBook.ContainsKey(revKey)) return null;
            name = recipeBook[revKey];
        }
        else
        {
            name = recipeBook[key];
        }
        return namesToItemInfos[name];
    }

    private Dictionary<ItemName, ItemInfo> namesToItemInfos = new(); 
    public RecipeInfo() {}

    public void Start()
    {
        var items = Resources.LoadAll("", typeof(ItemInfo));

        foreach (var rawItemInfo in items)
        {
            var itemInfo = rawItemInfo as ItemInfo;
            namesToItemInfos[itemInfo.itemName] = itemInfo;
        }
    }
    private static RecipeInfo instance;
    public static RecipeInfo Get()
    {
        if (instance == null)
        {
            instance = FindFirstObjectByType<RecipeInfo>();
        }
        return instance;
    }
}





using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static ItemInfo;

/// <summary>
/// Singleton object in the scene.
/// </summary>
public class RecipeInfo : Singleton<RecipeInfo> {
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
        return NamesToItemInfos[name];
    }

    public Dictionary<ItemName, ItemInfo> NamesToItemInfos; 
    public RecipeInfo() {}

    public void Awake()
    {
        base.Awake();
        
        NamesToItemInfos = new();
        // TODO Replace potentially since runtime cost and build gets screwed up
        var items = Resources.LoadAll("", typeof(ItemInfo));

        foreach (var rawItemInfo in items)
        {
            var itemInfo = rawItemInfo as ItemInfo;
            NamesToItemInfos[itemInfo.itemName] = itemInfo;
        }
    }
}





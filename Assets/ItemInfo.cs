using System;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.UI;
/// <summary>
/// Scriptable Object that holds all of the item information
/// </summary>
[CreateAssetMenu(menuName = "Scriptable Objects/Item Info")]
public class ItemInfo : ScriptableObject
{
    public enum ItemType{ // All possible types of items
        Weapon,
        Resource,
        Container,
        Empty
    };

    public enum ItemName { // All possible names of items
        Spear,
        Empty
    };

    [SerializeField] public ItemType itemType;// type of item

    [SerializeField] public ItemName itemName; // name of item

    [SerializeField] public Image itemImage; // image icon for item in inventory

    [SerializeField] public bool isCraftable; // whether or not the item can be crafted

    [SerializeField] public bool isIngredient; // whether or not the item can be used as an ingredient for crafting

    [SerializeField] public string description; // description of the item

    // Maybe include reference to gameobject for instantiating?


    public void log() { 
        Debug.Log("Item Type: " +  itemType);
        Debug.Log("Item Name: " + itemName);
        Debug.Log("Is Craftable: " + isCraftable);
        Debug.Log("Is Ingredient: " + isIngredient);
        Debug.Log("Description: " + description);
    }
}


using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.UI;
using static ItemInfo;
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
        Trap,
        Empty
    };

    public enum ItemName { // All possible names of items
        Spear,
        Rock,
        RockSpear,
        StunTrap,
        Apple,
        CaptureOrb,
        Empty
    };

    [SerializeField] public ItemType itemType;// type of item

    [SerializeField] public ItemName itemName; // name of item

    [SerializeField] public Image itemImage; // image icon for item in inventory

    [SerializeField] public bool isCraftable; // whether or not the item can be crafted

    [SerializeField] public bool isIngredient; // whether or not the item can be used as an ingredient for crafting

    [SerializeField] public int maxStackCount = 1; // max number of this item in a stack

    [SerializeField] public string description; // description of the item

    [SerializeReference] public IPlayerActionStrategy playerActionStrategy; // strategy pattern for player actions with the item

    // Maybe include reference to gameobject for instantiating?
    // needed for placing items in the world
    public GameObject itemPrefab; // prefab for the item in the world
    public GameObject itemPlacementPrefab; // prefab for the item placement variant when previewing placement

    public void log() { 
        Debug.Log("Item Type: " +  itemType);
        Debug.Log("Item Name: " + itemName);
        Debug.Log("Is Craftable: " + isCraftable);
        Debug.Log("Is Ingredient: " + isIngredient);
        Debug.Log("Max Stack Count: " + maxStackCount);
        Debug.Log("Description: " + description);
    }
}


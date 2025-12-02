using CrashKonijn.Goap.Editor;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingMenu : Singleton<CraftingMenu>
{
    private Canvas canvas;
    [SerializeField] public GameObject buttonTemplate; // prefab
    [SerializeField] public Transform contentPanel;
    [SerializeField] public TextMeshProUGUI outputName;
    [SerializeField] public TextMeshProUGUI description;
    [SerializeField] public TextMeshProUGUI ingredient1Count;
    [SerializeField] public TextMeshProUGUI ingredient2Count;
    [SerializeField] public TextMeshProUGUI ingredient1Name;
    [SerializeField] public TextMeshProUGUI ingredient2Name;
    [SerializeField] public Button craftButton;
    [SerializeField] public Image ingredient1Image;
    [SerializeField] public Image ingredient2Image;
    [SerializeField] public Image outputImage;

    [SerializeField] public Recipe test;

    private Recipe selected;


    protected override void Awake()
    {
        base.Awake();
        craftButton.onClick.AddListener(() => Craft());
        canvas = GetComponentInChildren<Canvas>();
        canvas.enabled = false;
    }

    public void Craft() {
        if (craftButton.interactable) {
            Debug.Log("Crafting " + selected.ingredient1.itemName + " + " + selected.ingredient2.itemName);
            // TODO: craft item and update inventory
        }
    }

    public void Select(Recipe recipe) {
        selected = recipe;
        craftButton.interactable = Inventory.Instance.Contains(recipe.ingredient1.itemName, recipe.count1) &&
            Inventory.Instance.Contains(recipe.ingredient2.itemName, recipe.count2);
        ingredient1Name.text = recipe.ingredient1.itemName.ToString();
        ingredient2Name.text = recipe.ingredient2.itemName.ToString();
        ingredient1Count.text = "x" + recipe.count1;
        ingredient2Count.text = "x" + recipe.count2;
        outputName.text = recipe.output.itemName.ToString();
        description.text = recipe.output.description;
        Debug.Log("Selected " + selected.ingredient1.itemName + " + " + selected.ingredient2.itemName);
        // TODO: update images
    }

    public void AddRecipe(Recipe recipe) {
        GameObject button = Instantiate(buttonTemplate, contentPanel);
        button.GetComponentInChildren<TMP_Text>().text = recipe.output.itemName.ToString();
        // TODO: add image to button
        button.GetComponent<Button>().onClick.AddListener(() => Select(recipe));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backslash)) {
            AddRecipe(test);
            Debug.Log("Added " + test.ingredient1.itemName + " + " + test.ingredient2.itemName);
        }
        if (Input.GetKeyDown(KeyCode.P)) {
            canvas.enabled = !canvas.enabled;
        }
    }
}

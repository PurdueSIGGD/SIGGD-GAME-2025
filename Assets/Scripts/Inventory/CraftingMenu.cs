using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingMenu : Singleton<CraftingMenu>
{
    private Canvas canvas;
    [SerializeField] public GameObject buttonTemplate; // prefab
    [SerializeField] public TextMeshProUGUI plusTemplate; // prefab
    [SerializeField] public GameObject ingredientTemplate; // prefab
    [SerializeField] public Transform contentPanel;
    [SerializeField] public Transform ingredientPanel;
    [SerializeField] public TextMeshProUGUI outputName;
    [SerializeField] public TextMeshProUGUI description;
    public List<GameObject> ingredientGroups = new List<GameObject>();
    public List<TextMeshProUGUI> pluses = new List<TextMeshProUGUI>();
    [SerializeField] public Button craftButton;
    [SerializeField] public Image outputImage;

    [SerializeField] public List<Recipe> testRecipes;
    public const int SPACING_DISTRIBUTION = 300;

    private Recipe selected;


    protected override void Awake()
    {
        base.Awake();
        craftButton.onClick.AddListener(() => Craft());
        canvas = GetComponentInChildren<Canvas>();
    }

    void Start()
    {
        foreach (Recipe recipe in testRecipes) // Test recipe, remove later
        {
            AddRecipe(recipe);
        }
        Disable();
    }

    public void ShowCraftingMenu(bool enabled) {
        if (enabled)
        {
            Enable();
        }
        else
        {
            Disable();
        }
    }

    public bool IsCanvasActive()
    {
        return canvas.enabled;
    }

    public void Enable()
    {
        canvas.enabled = true;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        PlayerInput.Instance.DebugToggleInput(true);
    }

    public void Disable()
    {
        description.enabled = false;
        outputName.enabled = false;
        outputImage.enabled = false;
        craftButton.gameObject.SetActive(false);
        ingredientPanel.gameObject.SetActive(false);
        for (int i = 0; i < ingredientGroups.Count; i++)
        {
            Destroy(ingredientGroups[i]);
        }
        ingredientGroups.Clear();
        for (int i = 0; i < pluses.Count; i++)
        {
            Destroy(pluses[i].gameObject);
        }
        pluses.Clear();
        canvas.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PlayerInput.Instance.DebugToggleInput(false);
    }

    public bool IsCraftable() {
        for (int i = 0; i < selected.ingredients.Count; i++) {
            if (!Inventory.Instance.Contains(selected.ingredients[i].itemName, selected.counts[i])) {
                return false;
            }
        }
        return true;
    }

    public void Craft() {
        if (craftButton.interactable) {
            Debug.Log("Crafting " + selected.output.itemName);
            Inventory.Instance.Craft(selected);
            // update whether the button is interactable since inventory was updated
            craftButton.interactable = IsCraftable();
        }
    }

    public void Select(Recipe recipe) {
        Disable();
        selected = recipe;
        Enable();
        description.enabled = true;
        outputName.enabled = true;
        outputImage.enabled = true;
        craftButton.gameObject.SetActive(true);
        ingredientPanel.gameObject.SetActive(true);
        outputName.text = recipe.output.itemName.ToString();
        description.text = recipe.output.description;
        outputImage.sprite = recipe.output.itemImage;
        craftButton.interactable = IsCraftable();
        // update spacing of horizontal layout group based on number of ingredients in the recipe
        ingredientPanel.gameObject.GetComponent<HorizontalLayoutGroup>().spacing = SPACING_DISTRIBUTION / recipe.ingredients.Count;
        // make list of icons and text boxes
        for (int i = 0; i < recipe.ingredients.Count; i++) {
            GameObject ingredientGroup = Instantiate(ingredientTemplate, ingredientPanel);
            ingredientGroup.transform.Find("Ingredient Name").GetComponent<TMP_Text>().text = recipe.ingredients[i].itemName.ToString();
            ingredientGroup.transform.Find("Ingredient Count").GetComponent<TMP_Text>().text = "x " + recipe.counts[i];
            ingredientGroup.transform.Find("Ingredient Image").GetComponent<Image>().sprite = recipe.ingredients[i].itemImage;
            ingredientGroups.Add(ingredientGroup);
            if (i != recipe.ingredients.Count - 1) 
            {
                TextMeshProUGUI plus = Instantiate(plusTemplate, ingredientPanel);
                pluses.Add(plus);
            }
        }
        Debug.Log("Selected " + selected.output.itemName);
    }

    public void AddRecipe(Recipe recipe) {
        GameObject button = Instantiate(buttonTemplate, contentPanel);
        button.GetComponentInChildren<TMP_Text>().text = recipe.output.itemName.ToString();
        button.GetComponentInChildren<Image>().sprite = recipe.output.itemImage;
        button.GetComponent<Button>().onClick.AddListener(() => Select(recipe));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backslash)) {
            AddRecipe(testRecipes[0]);
            Debug.Log("Added " + testRecipes[0].output.itemName);
        }
    }
}

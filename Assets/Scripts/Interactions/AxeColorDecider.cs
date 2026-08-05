using UnityEngine;

public class AxeColorDecider : MonoBehaviour
{
    public Material axeDefaultMaterial;
    public Material Material2;

    private Renderer cachedRenderer;

    private void OnEnable()
    {
        Inventory.OnHotbarSelectionChanged += AxeColor;
        Inventory.OnHotbarContentsChanged += OnHotbarContentsChanged;
    }

    private void OnDisable()
    {
        Inventory.OnHotbarSelectionChanged -= AxeColor;
        Inventory.OnHotbarContentsChanged -= OnHotbarContentsChanged;
    }

    private void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
        if (cachedRenderer == null)
        {
            Debug.LogWarning("AxeColorDecider: no Renderer found on GameObject.");
        }
    }

    private void AxeColor(int hotbarslot)
    {
        // Selection index changed -> update material for newly selected item
        UpdateAxeMaterial();
    }

    private void OnHotbarContentsChanged()
    {
        // Hotbar contents changed (swap/add/remove) -> re-check the currently selected item
        UpdateAxeMaterial();
    }

    private void UpdateAxeMaterial()
    {
        if (cachedRenderer == null) return;

        var selectedItem = Inventory.Instance.GetSelectedItem();
        if (selectedItem == null || selectedItem.itemName == ItemInfo.ItemName.Empty)
        {
            // No selected item -> choose a fallback (use default material)
            if (axeDefaultMaterial != null)
                cachedRenderer.material = axeDefaultMaterial;
            Debug.Log("AxeColorDecider: no selected item or empty -> using default material");
            return;
        }

        // Update material based on item type
        switch (selectedItem.itemName)
        {
            case ItemInfo.ItemName.Axe:
                if (axeDefaultMaterial != null) cachedRenderer.material = axeDefaultMaterial;
                Debug.Log("AxeColorDecider: applied axe material");
                break;

            case ItemInfo.ItemName.RockSpear:
                if (Material2 != null) cachedRenderer.material = Material2;
                Debug.Log("AxeColorDecider: applied rock spear material");
                break;

            //default:
                // Unknown item -> fallback to default
               // if (axeDefaultMaterial != null) cachedRenderer.material = axeDefaultMaterial;
               // Debug.Log("AxeColorDecider: applied fallback/default material");
               // break;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ensure material state is correct at start
        UpdateAxeMaterial();
    }

}

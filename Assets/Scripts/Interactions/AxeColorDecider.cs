using System.Collections;
using UnityEngine;

public class AxeColorDecider : MonoBehaviour
{
    [Tooltip("Number of frames to wait after a hotbar content change before updating material.")]
    [SerializeField] private int delayFrames = 5;

    public Material axeDefaultMaterial;
    public Material Material2;

    private Renderer cachedRenderer;
    private Coroutine pendingUpdate;

    private void OnEnable()
    {
        Inventory.OnHotbarSelectionChanged += AxeColor;
        Inventory.OnHotbarContentsChanged += OnHotbarContentsChanged;

        // Ensure correct material immediately when this component becomes enabled
        UpdateAxeMaterial();
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
        // Selection index changed -> update material immediately
        UpdateAxeMaterial();
    }

    private void OnHotbarContentsChanged()
    {
        // Content changes may trigger multiple internal re-packs / selection adjustments
        // and other subscribers (UI) may mutate state over frames. Wait a small number
        // of frames so Inventory's internal mutations settle and the selected slot is stable.
        if (pendingUpdate != null) StopCoroutine(pendingUpdate);
        pendingUpdate = StartCoroutine(DelayedUpdateFrames(Mathf.Max(0, delayFrames)));
    }

    private IEnumerator DelayedUpdateFrames(int frames)
    {
        for (int i = 0; i < frames; i++)
            yield return null;

        UpdateAxeMaterial();
        pendingUpdate = null;
    }

    private void UpdateAxeMaterial()
    {
        if (cachedRenderer == null) return;

        var selectedItem = Inventory.Instance.GetSelectedItem();
        int selIndex = Inventory.Instance.GetSelected();
        Debug.Log($"AxeColorDecider: UpdateAxeMaterial selectedIndex={selIndex}, selectedItem={(selectedItem == null ? "null" : selectedItem.itemName.ToString())}");

        if (selectedItem == null || selectedItem.itemName == ItemInfo.ItemName.Empty)
        {
            if (axeDefaultMaterial != null)
                cachedRenderer.material = axeDefaultMaterial;
            Debug.Log("AxeColorDecider: no selected item or empty -> using default material");
            return;
        }

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

            default:
                // For any other item, explicitly revert to default to avoid leaving a stale material
                if (axeDefaultMaterial != null) cachedRenderer.material = axeDefaultMaterial;
                Debug.Log("AxeColorDecider: selected item not handled -> using default material");
                break;
        }
    }

    // Ensure correct material at start
    void Start()
    {
        UpdateAxeMaterial();
    }
}

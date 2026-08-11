using System.Collections;
using UnityEngine;

public class MushroomColorDecider : MonoBehaviour
{
    [Tooltip("Number of frames to wait after a hotbar content change before updating material.")]
    [SerializeField] private int delayFrames = 5;

    public Material itemDefaultMaterial;
    public Material Material2;
    public Material Material3;
    public Material Material4;
    public Material Material5;

      

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
            Debug.LogWarning("MushroomColorDecider: no Renderer found on GameObject.");
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

        // Guard Inventory.Instance in case initialization hasn't happened yet or player isn't holding anything.
        var inv = Inventory.Instance;
        if (inv == null)
        {
            if (itemDefaultMaterial != null) cachedRenderer.material = itemDefaultMaterial;
            Debug.Log("MushroomColorDecider: Inventory.Instance is null -> using default material");
            return;
        }

        var selectedItem = inv.GetSelectedItem();
        int selIndex = inv.GetSelected();
        Debug.Log($"MushroomColorDecider: UpdateAxeMaterial selectedIndex={selIndex}, selectedItem={(selectedItem == null ? "null" : selectedItem.itemName.ToString())}");

        if (selectedItem == null || selectedItem.itemName == ItemInfo.ItemName.Empty)
        {
            if (itemDefaultMaterial != null)
                cachedRenderer.material = itemDefaultMaterial;
            Debug.Log("MushroomColorDecider: no selected item or empty -> using default material");
            return;
        }

        switch (selectedItem.itemName)
        {
            case ItemInfo.ItemName.DarkPurpleMushroom:
                if (itemDefaultMaterial != null) cachedRenderer.material = itemDefaultMaterial;
                Debug.Log("MushroomColorDecider: dark purple mush material applied");
                break;

            case ItemInfo.ItemName.BlueMushroom:
                if (Material2 != null) cachedRenderer.material = Material2;
                Debug.Log("MushroomColorDecider: blue mush material applied");
                break;

            case ItemInfo.ItemName.YellowMushroom:
                if (Material3 != null) cachedRenderer.material = Material3;
                Debug.Log("MushroomColorDecider: yellow mush material applied");
                break;


            default:
                // For any other item, explicitly revert to default to avoid leaving a stale material
                if (itemDefaultMaterial != null) cachedRenderer.material = itemDefaultMaterial;
                Debug.Log("MushroomColorDecider: selected item not handled -> using default material");
                break;
        }
    }

    // Ensure correct material at start
    void Start()
    {
        UpdateAxeMaterial();
    }
}

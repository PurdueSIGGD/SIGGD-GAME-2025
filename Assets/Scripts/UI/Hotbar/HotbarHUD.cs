using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the in-game hotbar HUD.
///
/// Layout: items grow left from a right-side anchor (mirroring the inventory panel).
/// Only occupied slots are shown. A selection indicator image moves to highlight the
/// currently selected slot, and a large icon on the right reflects the selected item.
///
/// Slot button clicks select that hotbar index immediately (only if occupied).
/// Scroll and number keys are already handled by Inventory and call back via
/// Inventory.OnHotbarSelectionChanged / OnHotbarContentsChanged.
/// </summary>
public class HotbarHUD : MonoBehaviour
{
    [Header("Slot Spawning")]
    [Tooltip("Prefab with a HotbarItemUI component. Slots are laid out left of the anchor.")]
    [SerializeField] private GameObject slotPrefab;
    [Tooltip("The RectTransform used as the right-side anchor. Slots grow leftward from here.")]
    [SerializeField] private RectTransform slotAnchor;
    [Tooltip("Pixel distance between slot centres.")]
    [SerializeField] private float slotSpacing = 80f;

    [Header("Selection Indicator")]
    [Tooltip("Instance (not prefab) of the selection border in the scene. Shared across slots.")]
    [SerializeField] private HotbarSelectionIndicator selectionIndicator;

    [Header("Selected Item Display")]
    [Tooltip("Image on the right that shows the currently selected item icon.")]
    [SerializeField] private Image selectedItemImage;

    private readonly List<HotbarItemUI> activeSlots = new();
    // Set to true when a rebuild is requested but Inventory isn't ready yet
    private bool pendingRebuild;

    void OnEnable()
    {
        Inventory.OnHotbarContentsChanged += Rebuild;
        Inventory.OnHotbarSelectionChanged += UpdateSelection;
    }

    void OnDisable()
    {
        Inventory.OnHotbarContentsChanged -= Rebuild;
        Inventory.OnHotbarSelectionChanged -= UpdateSelection;
    }

    void Start()
    {
        Rebuild();
    }

    void LateUpdate()
    {
        if (pendingRebuild)
        {
            pendingRebuild = false;
            Rebuild();
        }
    }

    /// <summary>
    /// Destroys all slot GameObjects and respawns them to match the current hotbar state.
    /// Called whenever hotbar contents change.
    /// </summary>
    public void Rebuild()
    {
        if (Inventory.Instance == null)
        {
            // Inventory not ready yet – retry next frame
            pendingRebuild = true;
            return;
        }

        foreach (var slot in activeSlots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        activeSlots.Clear();

        InventorySlot[] hotbar = Inventory.Instance.GetHotbarSlots();
        int spawnIndex = 0;

        // Iterate right-to-left through occupied slots so slot[0] ends up furthest left
        for (int i = 0; i < hotbar.Length; i++)
        {
            InventorySlot data = hotbar[i];
            if (data == null || data.count == 0 || data.itemInfo == null ||
                data.itemInfo.itemName == ItemInfo.ItemName.Empty)
                continue;

            GameObject go = Instantiate(slotPrefab, slotAnchor.parent);
            HotbarItemUI itemUI = go.GetComponent<HotbarItemUI>();
            itemUI.SetItem(data.itemInfo, data.count, i);

            RectTransform rt = go.GetComponent<RectTransform>();
            // Grow leftward from the anchor: spawnIndex 0 is the rightmost slot
            rt.anchoredPosition = slotAnchor.anchoredPosition + Vector2.left * (spawnIndex * slotSpacing);

            // Wire up click: select this hotbar index immediately
            var capturedIndex = i;
            var btn = go.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => Inventory.Instance.Select(capturedIndex));

            activeSlots.Add(itemUI);
            spawnIndex++;
        }

        UpdateSelection(Inventory.Instance.GetSelected());
    }

    /// <summary>
    /// Moves the selection indicator to the slot matching <paramref name="hotbarIndex"/>
    /// and updates the large selected-item image.
    /// </summary>
    public void UpdateSelection(int hotbarIndex)
    {
        if (Inventory.Instance == null) return;

        ItemInfo selected = Inventory.Instance.GetSelectedItem();

        // Update the large icon on the right
        if (selectedItemImage != null)
        {
            if (selected != null && selected.itemName != ItemInfo.ItemName.Empty && selected.itemImage != null)
            {
                selectedItemImage.sprite = selected.itemImage;
                selectedItemImage.enabled = true;
            }
            else
            {
                selectedItemImage.enabled = false;
            }
        }

        // Move the selection indicator
        if (selectionIndicator == null) return;

        HotbarItemUI target = activeSlots.Find(s => s.HotbarIndex == hotbarIndex);
        if (target != null)
        {
            selectionIndicator.SetVisible(true);
            selectionIndicator.MoveTo(target.GetComponent<RectTransform>().anchoredPosition);
        }
        else
        {
            selectionIndicator.SetVisible(false);
        }
    }
}

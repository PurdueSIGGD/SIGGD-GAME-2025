using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attached to each dynamically-spawned hotbar slot in the in-game HUD.
/// Displays one item's icon. The slot's RectTransform is positioned by HotbarHUD.
/// </summary>
public class HotbarItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI countText;

    private int hotbarIndex;

    /// <summary>
    /// Initialises this slot with the item to display and its logical hotbar index.
    /// </summary>
    public void SetItem(ItemInfo itemInfo, int count, int index)
    {
        hotbarIndex = index;

        if (itemInfo != null && itemInfo.itemName != ItemInfo.ItemName.Empty && itemInfo.itemImage != null)
        {
            iconImage.sprite = itemInfo.itemImage;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }

        if (countText != null)
        {
            countText.text = count > 1 ? count.ToString() : "";
        }
    }

    /// <summary>
    /// The logical hotbar index this slot represents.
    /// </summary>
    public int HotbarIndex => hotbarIndex;
}


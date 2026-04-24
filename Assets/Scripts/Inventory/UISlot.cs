using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISlot : MonoBehaviour
{
    public ItemInfo itemInfo { get; private set; }
    
    public int count = 0;
    [HideInInspector] public int index;

    [SerializeField] TextMeshProUGUI textDisplay;
    [SerializeField] Image imageDisplay;
    [SerializeField] Sprite emptySprite;
    private Button button;

    void Awake()
    {
        textDisplay = GetComponentInChildren<TextMeshProUGUI>();
        button = GetComponent<Button>();
        button.onClick.AddListener(Clicked);
    }

    void Start()
    {
        // Inventory handles all initialisation for UISlots to avoid overwriting slots with Empty
    }

    /// <summary>
    /// Writes item data into this UI slot so UpdateSlot can display it correctly.
    /// Called by the linked InventorySlot before UpdateSlot.
    /// </summary>
    public void SetData(ItemInfo info, int itemCount)
    {
        itemInfo = info;
        count = itemCount;
    }

    public void Clicked() {
        Debug.Log("Clicked " + index);
        Inventory.Instance.SwapSelect(index);
    }

    public void SetColor(Color color) {
        button.GetComponent<Image>().color = color;
    }


    public void UpdateSlot()
    {
        Debug.Log($"Slot {index}: itemInfo={itemInfo}, sprite={itemInfo?.itemImage}, imageDisplay={imageDisplay}");
        if (itemInfo != null && itemInfo.name != "Empty")
        {
            //if (!textDisplay) textDisplay = GetComponentInChildren<TextMeshProUGUI>(); // double check
            //textDisplay.text = itemInfo.name + "(" + count + ")";
            if (textDisplay) textDisplay.text = count.ToString();
            if (imageDisplay)
            {
                imageDisplay.sprite = itemInfo.itemImage;
                imageDisplay.color = Color.white;
            }
        }
        else
        {
            if (textDisplay) textDisplay.text = "";
            Debug.Log("changing image to empty");
            if (imageDisplay)
            {
                imageDisplay.sprite = emptySprite;
                imageDisplay.color = Color.clear;
            }
        }
    }
}
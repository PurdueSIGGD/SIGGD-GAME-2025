using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISlot : MonoBehaviour
{
    public ItemInfo itemInfo = null;
    
    public int count = 0;
    [HideInInspector] public int index;

    [SerializeField] TextMeshProUGUI textDisplay;
    [SerializeField] Image imageDisplay;
    private Button button;

    void Awake()
    {
        textDisplay = GetComponentInChildren<TextMeshProUGUI>();
        button = GetComponent<Button>();
        button.onClick.AddListener(Clicked);
    }

    void Start()
    {
        // Inventory will handle all of the initialization for UISlots to avoid overwriting slots with Empty

        //Debug.Log(RecipeInfo.Instance == null ? "recipeinfo null" : "recipeinfo not null");
        //Debug.Log((itemInfo == null ? "iteminfo is before" : "ItemInfo is not null before") + " " + index);
        //itemInfo = RecipeInfo.Instance.NamesToItemInfos[ItemInfo.ItemName.Empty];
        //Debug.Log(itemInfo == null ? "iteminfo is null" : "ItemInfo is not null now");
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
        if (itemInfo)
        {
            //if (!textDisplay) textDisplay = GetComponentInChildren<TextMeshProUGUI>(); // double check
            //textDisplay.text = itemInfo.name + "(" + count + ")";
            if (textDisplay) textDisplay.text = itemInfo.name + "(" + count + ")";
            if (imageDisplay)
            {
                imageDisplay.sprite = itemInfo.itemImage;
                imageDisplay.color = Color.white;
            }

        }
        else {
            if (textDisplay) textDisplay.text = "empty";
            if (imageDisplay)
            {
                imageDisplay.sprite = null;
                imageDisplay.color = Color.clear;
            }
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISlot : MonoBehaviour
{
    public ItemInfo itemInfo = null;
    
    public int count = 0;
    [HideInInspector] public int index;

    private TextMeshProUGUI textDisplay;
    private Button button;

    void Awake()
    {
        textDisplay = GetComponentInChildren<TextMeshProUGUI>();
        button = GetComponent<Button>();
        button.onClick.AddListener(Clicked);
    }

    void Start()
    {
        Debug.Log(RecipeInfo.Instance == null ? "recipeinfo null" : "recipeinfo not null");
        itemInfo = RecipeInfo.Instance.NamesToItemInfos[ItemInfo.ItemName.Empty];
        Debug.Log(itemInfo == null ? "iteminfo is null" : "ItemInfo is not null now");
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
            if (!textDisplay) textDisplay = GetComponentInChildren<TextMeshProUGUI>(); // double check
            textDisplay.text = itemInfo.name + "(" + count + ")";
        }
        else {
            textDisplay.text = "empty";
        }
    }
}

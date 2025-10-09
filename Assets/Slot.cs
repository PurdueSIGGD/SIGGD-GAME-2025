using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public ItemInfo ItemInfo;
    public int count;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        Debug.Log("This slot, containing " + count + "x " + ItemInfo.itemName + " was pressed.");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

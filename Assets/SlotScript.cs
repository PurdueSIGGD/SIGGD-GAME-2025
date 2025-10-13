using System;
using UnityEngine;
using UnityEngine.UI;

public class SlotScript : MonoBehaviour
{
    // These serializable fields can't be replaced with non-Monobehavior class Slot
    public ItemInfo ItemInfo;
    public int Count;

    [NonSerialized]
    public int Index;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        Debug.Log("This slot, containing " + Count + "x " + ItemInfo.itemName + " was pressed.");
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void SetSlot(Slot slot)
    {
        slot.itemInfo = ItemInfo;
        slot.count = Count;
    }
}
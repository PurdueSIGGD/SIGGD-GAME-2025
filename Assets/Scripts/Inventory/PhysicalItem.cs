using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// Script for the physical item that can be picked up
/// </summary>
public class PhysicalItem : MonoBehaviour, IPointerClickHandler
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] public ItemInfo itemInfo;
    [SerializeField] public Inventory inventory;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Pickup() {
        // remove physical gameobject and add item to inventory
        inventory.Add(itemInfo, 3);
        Destroy(this.gameObject);
        inventory.PrintInventory();
    }

    void OnPointerClick(PointerEventData pointerEventData) {
        Debug.Log("Clicked " + itemInfo.itemName);
        Pickup();
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        OnPointerClick(eventData);
    }

}

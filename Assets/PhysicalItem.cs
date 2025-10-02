using UnityEngine;
/// <summary>
/// Script for the physical item that can be picked up
/// </summary>
public class PhysicalItem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] public ItemInfo itemInfo;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void pickup() { 
        // remove physical gameobject and add item to inventory
    }
}

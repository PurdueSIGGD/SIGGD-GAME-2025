using UnityEngine;
using UnityEngine.UI;

public class hotbar : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField]
    private Button[] Slots = new Button[3];

    void Start()
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            var button = Slots[i].GetComponent<Button>();
            var buttonIndex = i;
            button.onClick.AddListener(() => OnSlotSelected(buttonIndex));
        }
    }

    void OnSlotSelected(int slotIndex)
    {
        Debug.Log("slot number " + slotIndex + " was chosen");
    }

    // Update is called once per frame
    void Update()
    {

    }
}

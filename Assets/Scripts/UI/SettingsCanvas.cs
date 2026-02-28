using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsCanvas : MonoBehaviour
{
    public GameObject settingsContent;
    public GameObject inputTemplate;
    public InputActionAsset inputActionAsset;

    private Vector3 GetBottomLeftCorner(GameObject obj, out float height)
    {
        RectTransform tempRT = obj.GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        tempRT.GetWorldCorners(corners);
        height = corners[0].y - corners[1].y;
        return corners[0];
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Vector3 position = GetBottomLeftCorner(inputTemplate, out float height);
        foreach (InputAction action in inputActionAsset)
        {
            GameObject opts = Instantiate(inputTemplate, settingsContent.transform);
            opts.GetComponentInChildren<TMP_Text>().text = action.name;
            // position.y += height;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

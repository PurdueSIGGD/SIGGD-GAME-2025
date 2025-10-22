using UnityEngine;

public class checkForInput : MonoBehaviour
{
    public GameObject myGameObject;
    public GameObject myGameObject2;
    public bool isEnabled;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   
    void Start()
    {
        myGameObject.SetActive(false);
        myGameObject2.SetActive(false);
        isEnabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isEnabled = !isEnabled;
            myGameObject.SetActive(isEnabled);
            myGameObject2.SetActive(isEnabled);
        }
    }
}

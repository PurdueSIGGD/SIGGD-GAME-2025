using UnityEngine;

public class ExpandOnHover : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        float x = gameObject.transform.position.x;
        float y = gameObject.transform.position.y;
        float z = gameObject.transform.position.z;
        if (mousePos.x > x && mousePos.x < x + 160 && mousePos.y > y && mousePos.y < y + 30)
        {
            Enlarge();
        }
    }
    public void Enlarge()
    {

    }
}

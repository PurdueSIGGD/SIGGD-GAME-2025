using UnityEngine;
public class Firefly : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Material material = Resources.Load("Assets", typeof(Material)) as Material;
        material.SetColor("Black", new Color(0, 0, 0));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class OrionEasterEgg : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    string filePath = "Assets/Resources/OrionEasterEgg.txt";

   

void Start()
    {
        if (File.Exists(filePath))
        {
            Debug.LogError("file exists somehow");
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

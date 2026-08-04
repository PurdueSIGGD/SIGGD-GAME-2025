using System;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class OrionEasterEgg : MonoBehaviour
{
    string filePath;

    void Start()
    {
        filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Echoes of Isovios",
            "save.json"
        );

        if (File.Exists(filePath))
        {
            Debug.Log("Echoes of Isovios save found!");
            
        }
        else
        {
            Debug.Log("Save file not found: " + filePath);
            //Destroy(gameObject);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class MusicBoxIndex : MonoBehaviour
{
    [SerializeField] private int index;
    public static List<MusicBoxIndex> musicBoxes;

    private void Awake()
    {
        if (musicBoxes == null)
        {
            musicBoxes = new List<MusicBoxIndex>();
            musicBoxes.Add(null);
            musicBoxes.Add(null);
            musicBoxes.Add(null);
            musicBoxes.Add(null);
            musicBoxes.Add(null);
        }

        musicBoxes[index] = this;
        Debug.Log("Index: " + index + " succesfully applied");
        Debug.Log("MusicBoxes: " + musicBoxes[index].name);
    }

    public void Used()
    {
        Debug.Log("Used " + name);
        Destroy(gameObject);    
    }
}

using UnityEngine;
using System;

public class playRandomVoiceLine : MonoBehaviour
{
    public GameObject player;
    public System.Random rnd = new System.Random();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            int randIndex = rnd.Next(0, AudioLogManager.Instance.names.Count);
            AudioLogManager.Instance.playAudioLog(AudioLogManager.Instance.names[randIndex], player);
        }
    }
}

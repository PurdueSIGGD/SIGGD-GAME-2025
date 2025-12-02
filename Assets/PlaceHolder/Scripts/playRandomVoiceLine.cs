using UnityEngine;
using System;

public class playRandomVoiceLine : MonoBehaviour
{
    public System.Random rnd = new System.Random();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            int randIndex = rnd.Next(0, AudioLogManager.Instance.names.Count);
            AudioLogManager.Instance.PlayAudioLog(AudioLogManager.Instance.names[randIndex], PlayerID.Instance.gameObject);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class MusicChangeTrigger : MonoBehaviour
{
    [Header("Area")]
    [SerializeField] private MusicArea area;
    
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            Debug.Log("crossfading musci to " + area);
            AudioManager.Instance.SetMusicArea(area);
        }
    }
}

using UnityEngine;

public class testCombatDrumShift : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("crossfading to new track");
            MusicManager.Instance.CrossFadeMusic("ForestAmbianceAfterFirstApex", 1f);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("crossfading to new track");
            MusicManager.Instance.CrossFadeMusic("MainMenu", 1f);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("playing cassette");
            StartCoroutine(MusicManager.Instance.playCassette("Cassette5"));
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log("Pausing Music");
            MusicManager.Instance.PauseMusic(true);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            Debug.Log("Unpausing Music");
            MusicManager.Instance.PlayMusic(true);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            Debug.Log("playing radio noise");
            AudioManager.Instance.PlayOneShot("radionoise");
        }
    }
}

using UnityEngine;

public class touchBoxCrossFadeTest : MonoBehaviour
{
    public string musicYouAreCrossfadingTo = "";
    public float lengthOfCrossFade = 1f;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("crossfading music");
        MusicManager.Instance.CrossFadeMusic(musicYouAreCrossfadingTo, lengthOfCrossFade);
    }

    private void OnTriggerExit(Collider other)
    {
        MusicManager.Instance.CrossFadeMusic("LevelMusic", lengthOfCrossFade);
    }
}

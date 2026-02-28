using UnityEngine;

public class touchBoxCrossFadeTest : MonoBehaviour
{
    public string musicYouAreCrossfadingTo = "";
    public float lengthOfCrossFade = 1f;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("crossfading music");
        StartCoroutine(AudioManager.Instance.MusicCrossFade(musicYouAreCrossfadingTo, "LevelMusic", lengthOfCrossFade));
    }

    private void OnTriggerExit(Collider other)
    {
        StartCoroutine(AudioManager.Instance.MusicCrossFade("LevelMusic", musicYouAreCrossfadingTo, lengthOfCrossFade));
    }
}

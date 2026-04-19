using UnityEngine;

public class touchBoxCrossFadeTest : MonoBehaviour
{
    public string musicYouAreCrossfadingTo = "";
    public float lengthOfCrossFade = 1f;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("crossfading music");
        StartCoroutine(MusicManager.Instance.MusicCrossFade(musicYouAreCrossfadingTo, lengthOfCrossFade));
    }

    private void OnTriggerExit(Collider other)
    {
        StartCoroutine(MusicManager.Instance.MusicCrossFade("LevelMusic", lengthOfCrossFade));
    }
}

using UnityEngine;

public class touchBoxCrossFadeTest : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("crossfading music");
        StartCoroutine(AudioManager.Instance.MusicCrossFade("Vain Redemption", "LevelMusic", 1f));
    }
}

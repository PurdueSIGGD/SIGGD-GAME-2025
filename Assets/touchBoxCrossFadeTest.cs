using UnityEngine;

public class touchBoxCrossFadeTest : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        AudioManager.Instance.MusicCrossFade(levelMusic);
    }
}

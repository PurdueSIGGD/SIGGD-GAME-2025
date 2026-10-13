using UnityEngine;

public class PlaySFX : MonoBehaviour
{
    [SerializeField] private string sound;
    [SerializeField] private bool destroyObjectOnLeave;
    [SerializeField] private GameObject prefab;
    
    void Start()
    {
        AudioManager.Instance.PlayOneShot(sound);
    }

    private void OnDestroy()
    {
        if (destroyObjectOnLeave)
        {
            Destroy(prefab);
        }
    }
}

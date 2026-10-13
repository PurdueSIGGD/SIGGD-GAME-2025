using System.Collections;
using UnityEngine;

public class PlaySFX : MonoBehaviour
{
    [SerializeField] private string sound;
    [SerializeField] private bool destroyObjectOnLeave;
    [SerializeField] private GameObject prefab;
    [SerializeField] private float time;
    
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(time);
        AudioManager.Instance.PlayOneShotNoAsync(sound);
    }

    private void OnDestroy()
    {
        if (destroyObjectOnLeave)
        {
            Destroy(prefab);
        }
    }
}

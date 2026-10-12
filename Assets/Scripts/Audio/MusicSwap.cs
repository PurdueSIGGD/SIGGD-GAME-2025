using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class MusicSwap : MonoBehaviour
{
    [Tooltip("FMOD event key to crossfade to when the player enters this collider.")]
    [SerializeField] private string enterMusicKey = "";

    [Tooltip("FMOD event key to crossfade to when the player exits this collider (set to normal area music key).")]
    [SerializeField] private string exitMusicKey = "";

    [Tooltip("Duration, in seconds, for the crossfade.")]
    [SerializeField] private float crossfadeDuration = 2f;

    [Tooltip("If true, only objects tagged 'Player' will trigger the music swap.")]
    [SerializeField] private bool onlyPlayer = true;

    [Header("Optional Unity Events")]
    public UnityEvent onEnter;
    public UnityEvent onExit;

    private void Reset()
    {
        // Try to ensure the collider is a trigger so this component works out of the box.
        Collider c = GetComponent<Collider>();
        if (c != null)
        {
            c.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (onlyPlayer && !other.CompareTag("Player"))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(enterMusicKey))
        {
            return;
        }

        if (MusicManager.Instance == null)
        {
            Debug.LogWarning("[MusicSwap] MusicManager.Instance is null. Cannot crossfade music on enter.");
            return;
        }

        MusicManager.Instance.CrossFadeMusic(enterMusicKey, crossfadeDuration);
        onEnter?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (onlyPlayer && !other.CompareTag("Player"))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(exitMusicKey))
        {
            return;
        }

        if (MusicManager.Instance == null)
        {
            Debug.LogWarning("[MusicSwap] MusicManager.Instance is null. Cannot crossfade music on exit.");
            return;
        }

        MusicManager.Instance.CrossFadeMusic(exitMusicKey, crossfadeDuration);
        onExit?.Invoke();
    }
}

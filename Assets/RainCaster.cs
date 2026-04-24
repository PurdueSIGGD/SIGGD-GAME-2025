using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class RainCaster : MonoBehaviour
{
    [SerializeField] bool shouldPlay = true;
    [SerializeField] ParticleSystem rainParticles;
    [SerializeField, MinMaxSlider(0, 1000)] Vector2 rainInterval;
    [SerializeField, MinMaxSlider(0, 1000)] Vector2 rainDuration;
    [SerializeField] float rainChanceOnStart = 0.2f;
    [SerializeField] float rainHeight = 40f;

    Coroutine currentRain;
    Coroutine currentWait;

    void Start()
    {
        if (!shouldPlay) return;

        rainParticles = Instantiate(rainParticles, transform.position, Quaternion.identity);
        if (Random.value < rainChanceOnStart && currentRain == null)
        {
            currentRain = StartCoroutine(PlayRain());
        }
    }

    private void LateUpdate()
    {
        if (rainParticles != null)
        {
            rainParticles.transform.position = new(PlayerID.Instance.transform.position.x, transform.position.y + rainHeight, PlayerID.Instance.transform.position.z);
        }
    }

    private IEnumerator PlayRain()
    {
        if (!shouldPlay) yield break;
        if (currentWait != null)
        {
            StopCoroutine(currentWait);
        }
        if (rainParticles.isPlaying)
        {
            yield break;
        }

        rainParticles.Play(true);
        float waitTime = Random.Range(rainDuration.x, rainDuration.y);
        yield return new WaitForSeconds(waitTime);
        rainParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        currentRain = null;
        currentWait = StartCoroutine(WaitForRain());
    }

    private IEnumerator WaitForRain()
    {
        if (!shouldPlay) yield break;
        if (rainParticles.isPlaying || currentRain != null)
        {
            yield break;
        }
        float waitTime = Random.Range(rainInterval.x, rainInterval.y);
        yield return new WaitForSeconds(waitTime);
        currentWait = null;
        currentRain = StartCoroutine(PlayRain());
    }
}

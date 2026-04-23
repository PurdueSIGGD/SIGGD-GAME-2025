using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Blinking : MonoBehaviour
{
    [SerializeField] float minBlinkInterval = 0.1f;
    [SerializeField] float maxBlinkInterval = 0.3f;
    [SerializeField] float minTimeHeld = 0.1f;
    [SerializeField] float maxTimeHeld = 0.2f;
    [SerializeField] Light component;

    private void Start()
    {
        StartCoroutine(BlinkCoroutine());
    }

    private IEnumerator BlinkCoroutine()
    {
        while (true)
        {
            component.enabled = true;
            yield return new WaitForSeconds(Random.Range(minTimeHeld, maxTimeHeld));
            component.enabled = false;
            yield return new WaitForSeconds(Random.Range(minBlinkInterval, maxBlinkInterval));
        }
    }
}

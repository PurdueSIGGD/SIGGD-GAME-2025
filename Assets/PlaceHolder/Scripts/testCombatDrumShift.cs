using UnityEngine;

public class testCombatDrumShift : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("crossfading to new track");
            MusicManager.Instance.CrossFadeMusic("Area1ForestAmbiance", 1f);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("shifting combat volume");
            MusicManager.Instance.ToggleComabatVolume();
        }
    }
}

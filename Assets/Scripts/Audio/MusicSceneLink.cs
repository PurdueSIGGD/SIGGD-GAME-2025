using FMOD;
using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
public class MusicSceneLink : MonoBehaviour
{
    // this class is what music you want playing as soon as the scene boots up

    public string sceneMusicKey = "";
    void Start()
    {
        StartCoroutine(WaitForEventsLoadedBeforeSceneReset());
    }

    private IEnumerator WaitForEventsLoadedBeforeSceneReset()
    {
        yield return new WaitUntil(() => FMODEvents.Instance.Initialized);

        MusicManager.Instance.SetCurTrack(sceneMusicKey);
        MusicManager.Instance.PlayMusic(false);
    }
}

using FMOD;
using FMOD.Studio;
using System.Collections;
using UnityEngine;

public class PlayerHummingSound : MonoBehaviour
{

    private EventInstance sfx;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //FMODEvents.Instance.GetEventInstance("RadiationButNoDamage", instance => { sfx = instance; });
        //ATTRIBUTES_3D attr = AudioManager.Instance.ConfigAttributes3D(GetComponent<Rigidbody>().position, GetComponent<Rigidbody>().linearVelocity, transform.forward, Vector3.up);
        //sfx.set3DAttributes(attr);
        //sfx.start();
        //StartCoroutine(LateStart());
    }

    private IEnumerator LateStart()
    {
        yield return new WaitUntil(() => FMODEvents.Instance.Initialized);
        yield return null;
        FMODEvents.Instance.GetEventInstance("radiationbutnodamage", instance => { sfx = instance; });
        ATTRIBUTES_3D attr = AudioManager.Instance.ConfigAttributes3D(GetComponent<Rigidbody>().position, GetComponent<Rigidbody>().linearVelocity, transform.forward, Vector3.up);
        sfx.set3DAttributes(attr);
        sfx.start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StopHumming()
    {
        sfx.stop(STOP_MODE.ALLOWFADEOUT);
    }
}

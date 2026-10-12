using System;
using Unity.VisualScripting;
using UnityEngine;
using FMODUnity;
using SIGGD.Save;
using SIGGD.Save.Modules;


public class VillageDestructionExternalEvent : ExternalEventTriggerer
{
    public GameObject village;
    public GameObject destroyedVillage;

    public static VillageDestructionExternalEvent Instance;
    
    //[SerializeField] private EventReference explosionSound;
    public static readonly string ConsoleBroken = "ConsoleDamaged";


    private void Awake()
    {
        Instance = this;
    }

    public override void TriggerExternalEvent()
    {
        Debug.Log("External event triggered!");

        if (village != null && destroyedVillage != null)
        {
            village.SetActive(false);
            destroyedVillage.SetActive(true);
            SaveManager.Instance.Get<SceneItemsModule>().VillageDestroyed = true;

            //if (explosionSound.IsNull)
            //    return;
            //else
                //RuntimeManager.PlayOneShot(explosionSound, transform.position);
                
            
            AudioManager.Instance.PlayOneShotNoAsync(ConsoleBroken, transform.position);
            

            //We don't actually have to kill the villagers we can just spawn in some corpses as part of the destroyed village prefab. 
            //Instead just delete them




        }
        else
        {
            Debug.LogWarning("Village or destroyed village GameObject is not assigned.");
        }
    }

    public void AlreadyDestroy()
    {
        village.SetActive(false);
        destroyedVillage.SetActive(true);
    }
}

using Unity.VisualScripting;
using UnityEngine;
using FMODUnity;


public class VillageDestructionExternalEvent : ExternalEventTriggerer
{
    public GameObject village;
    public GameObject destroyedVillage;
    //[SerializeField] private EventReference explosionSound;
    public static readonly string ConsoleBroken = "ConsoleDamaged";



    public override void TriggerExternalEvent()
    {
        Debug.Log("External event triggered!");

        if (village != null && destroyedVillage != null)
        {
            village.SetActive(false);
            destroyedVillage.SetActive(true);

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
}

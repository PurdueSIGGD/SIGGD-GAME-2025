using Unity.VisualScripting;
using UnityEngine;

public class VillageDestructionExternalEvent : ExternalEventTriggerer
{
    public GameObject village;
    public GameObject destroyedVillage;

    public GameObject villageQuests;
    public GameObject destroyedVillageQuests;

    public GameObject[] Villagers; 

    public override void TriggerExternalEvent()
    {
        Debug.Log("External event triggered!");

        if (village != null && destroyedVillage != null)
        {
            village.SetActive(false);
            destroyedVillage.SetActive(true);



            //We don't actually have to kill the villagers we can just spawn in some corpses as part of the destroyed village prefab. 
            //Instead just delete them

            
            foreach (GameObject villager in Villagers)
            {
                villager.SetActive(false);
                
            }

        }
        else
        {
            Debug.LogWarning("Village or destroyed village GameObject is not assigned.");
        }
    }
}

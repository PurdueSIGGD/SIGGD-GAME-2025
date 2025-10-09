using UnityEngine;
using UnityEngine.Rendering;

public class PlayerStats : MonoBehaviour
{
    public float maxHunger = 100f;
    public float currentHunger;
    public float hungerDecayRate = 1f;

    private GameObject player;



    void Start()
    {
        currentHunger = maxHunger;
        player = GetComponent<GameObject>();


    }

    void Update()
    {
        //hunger goes down and takes health when starving
        if (currentHunger > 0)
        {
            currentHunger -= hungerDecayRate * Time.deltaTime;

            Debug.Log(currentHunger);
        } else {

        //player is starving stuff
            Debug.Log("Starving");
            player.TakeDamage(10, this.gameObject, "");
        }

    }

    public void ChangeHunger(float ammount)
    {
        currentHunger += ammount;
    }
}




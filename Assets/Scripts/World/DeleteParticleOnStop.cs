using UnityEditorInternal;
using UnityEngine;

public class DeleteParticleOnStop : MonoBehaviour
{
    private ParticleSystem parent;
    
    public float waitTime = 5;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Grabs a reference to the component particle system not the game object.
        parent = this.transform.GetComponentInParent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        //This just waits for a couple seconds so that the particle system isn't instantly destroyed upon loading it in.
        if(waitTime <= 0)
        {
            //After waiting for the wait time, it waits again until all particles are gone before destroying the gameobject the component particle system + this scripts parent belong to.
            if(parent.particleCount <= 0)
            {
                Destroy(parent.gameObject);
            }
        }
        else
        {
            //Reduces the wait time to act as a timer of sorts.
            waitTime = waitTime - Time.deltaTime;
        }
    }
}

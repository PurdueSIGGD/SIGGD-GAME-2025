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
    //If you want to delete the particle system check stop mode & set it to destroy.
    //It will delete the particle system once all particles are emitted and are no more.
    private void OnParticleSystemStopped()
    {
        //This is kind of janky. Doesn't seem to be emitting the full 50 particles. 
        parent.Play();
    }
}

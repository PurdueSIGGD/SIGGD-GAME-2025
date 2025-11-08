using UnityEngine;

public class SpawnFireflyTrigger : MonoBehaviour
{
    //Note that you don't need to add in a firefly lined particle prefab into the scene. Just adding this collision trigger
    //Will be more than enough as it will handle everything else.
    public ParticleSystem fireflyParticles;
    private ParticleSystem addingParticles;
    private Quaternion faceAnotherObject;
    private Vector3 differencePos;
    public int displacementNum;
    private Transform playerCameraTransform;
    private Vector3 collisionEnterPosition;
    public bool particleFaceDirection = true;
    public int particleNum;
    private bool isEmitting;
    private void OnTriggerEnter(Collider other)
    {
        collisionEnterPosition = other.transform.position;
        //Essentially checks to see if it is emitting already & if the collider was from the player and if the first is false and 2nd is true then it goes through the if statement
        if (!isEmitting && other.gameObject.CompareTag("Player"))
        {
            addingParticles = Instantiate(fireflyParticles);


            //Gets a reference to the players Camera.
            playerCameraTransform = Camera.main.transform;

            //Need this for the particles to face either to or away from the player.
            float angle;
            Vector3 axis;
            playerCameraTransform.rotation.ToAngleAxis(out angle, out axis);
                  
            //This is to place the particle system within the camera's view angle and at distance that's specified by a variable within the inspector.
            //Note that this works because forward is a direction and not a vector and multiplying it by the displacement num gets a location.
            //Also note that this is where it can make or break the system. If you set too high of a displacement num in the inspector
            //Then you get a particle system that's too far away from the camera. The opposite is true as well.
            var displacedSum = playerCameraTransform.forward * displacementNum;
            
            //Ensures that the particles aren't placed behind the player (removing this line means particles spawn behind the players)
            //COULD BE USED WITH PARTICLEFACEDIRECTION == FALSE. 
            displacedSum = -displacedSum;
            
            //So that it doesn't become a beam from the sky.
            displacedSum.y = 0;
            
            addingParticles.transform.position = collisionEnterPosition;
            //With the base position set in the line before, it moves the system according to the displaced sum variable.
            addingParticles.transform.Translate(displacedSum);

            //To make it either face the camera (player) or away from it.
            if (particleFaceDirection == false)
            {
                differencePos = collisionEnterPosition - addingParticles.transform.position;
            }
            else
            {
                differencePos = addingParticles.transform.position - collisionEnterPosition;
            }

            faceAnotherObject = new Quaternion();
            faceAnotherObject.SetLookRotation(differencePos);
            addingParticles.transform.rotation = faceAnotherObject;
             
            if(particleNum > 0)
            {
                //I don't know if we will be needing the upcoming lines of code but I guess it's ok to have on hand in case we truly need it.
                //So I found this in a discussion post but apparently this is how you access any variable in the main of the particle system.
                //The way this works to my knowledge is it looks at the data for the main settings of the particle system components of the particle
                //system objects that this code spawns in and overrides the previous data, data from the prefab, with new data that comes from the
                //object this script is attached to. I don't know for sure if that's actually how it works but it's a good way of making sense of it.
                ParticleSystem componentParticle = addingParticles.GetComponent<ParticleSystem>();
                ParticleSystem.MainModule m = componentParticle.main;
                //One more thing, I've currently set the bursts to shoot out 100 particles all at once and if the max particles that you set is higher
                //than that then it will not send them all out in 1 burst. To counteract this set it to a higher number but for now it will be set to 100
                //because I don't think we will be needing more.
                m.maxParticles = particleNum;
            }
            
        }
    }
}

using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.UIElements;

public class SpawnFireflyTrigger : MonoBehaviour
{
    //Note that you don't need to add in a firefly lined particle prefab into the scene. Just adding this collision trigger
    //Will be more than enough as it will handle everything else.
    public ParticleSystem fireflyParticles;
    private ParticleSystem addingParticles;
    private Quaternion faceAnotherObject;
    private Vector3 differencePos;
    public int displacementNum;
    private Vector3 displacementVector;
    private Transform playerCameraTransform;
    private Vector3 collisionEnterPosition;
    public bool particleFaceDirection = true;

    private bool isEmitting;
    private void OnTriggerEnter(Collider other)
    {
        collisionEnterPosition = other.transform.position;
        if (!isEmitting && other.gameObject.CompareTag("Player"))
        {
            addingParticles = Instantiate(fireflyParticles, Camera.main.transform.position, Camera.main.transform.rotation);

            // lock rotation to stop particles going into the floor/sky
            addingParticles.transform.localEulerAngles = new Vector3(0, addingParticles.transform.localEulerAngles.y, addingParticles.transform.localEulerAngles.z);
            // spawn particles a little bit ahead of player
            addingParticles.transform.position = addingParticles.transform.position + Camera.main.transform.forward * 2; // arbitary num, should serialize later
            isEmitting = true;


            // hey, don't mean to comment out your code, but as I am testing, the particles are spawning somewhere else randomly
            // I'm running out of time to go through everyone's stuff, so I wrote a quick version of your code above instead
            // feel free to uncomment your code and test through it



            ////Gets a reference to the players Camera.
            //playerCameraTransform = Camera.main.transform;

            ////This is how far away the particle system is placed. This is in addition to the forward vector of the camera which already has some numbers to it.
            ////We will need these stuff when account for what direction the camera is truly facing in. Working with only the camera's forward motion will not get us anywhere as we need it's rotation as well.
            //float angle;
            //Vector3 axis;
            //playerCameraTransform.rotation.ToAngleAxis(out angle, out axis);
            //if(particleFaceDirection == false)
            //{
            //    displacementVector = new Vector3(displacementNum, 0, displacementNum);
            //}
            //else
            //{
            //    displacementVector = new Vector3(-displacementNum, 0, -displacementNum);
            //}
            ////We do this so that it reduces the distance at which the particle is placed.
            //displacementVector = -displacementVector;
            ////When we have the angle + forward vector of the camera (which I think is the forward vector of the player body) it becomes the true direction the camera is facing to.
            //Vector3 trueDirection = playerCameraTransform.forward * angle;
            
            ////So that we don't account for sky particles. 
            //trueDirection.y = 0;
            
            ////This is so that we aren't outside of the field of view or are placed in an awkward position. Optimally it should (almost) face straight ahead at the player.
            //if(trueDirection.z < 0)
            //{
            //    displacementVector.z = -displacementVector.z;
            //}
            //else if (trueDirection.x < 0)
            //{
            //    displacementVector.x = -displacementVector.x;
            //}
            //trueDirection += displacementVector;

            //addingParticles.transform.position = trueDirection ;
            
            ////This gets the distance between the player position and the particle position, the thing we're rotating. One of the if statements rotates it to face the camera while the other away from it.
            //if (particleFaceDirection == false)
            //{
            //    differencePos = collisionEnterPosition - addingParticles.transform.position;
            //}
            //else
            //{
            //    differencePos =  addingParticles.transform.position - collisionEnterPosition;
            //}

            //faceAnotherObject = new Quaternion();
            //faceAnotherObject = Quaternion.LookRotation(differencePos);

            //addingParticles.transform.rotation = faceAnotherObject;
        }
    }
}

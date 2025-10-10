using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.UIElements;

public class collision_trigger : MonoBehaviour
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
    private void OnTriggerEnter(Collider other)
    {
        collisionEnterPosition = other.transform.position;
        if (other.gameObject.name == "Player")
        {
            addingParticles = Instantiate(fireflyParticles);
            
            //Gets a reference to the players Camera.
            playerCameraTransform = other.transform.GetChild(0);

            //This is how far away the particle system is placed. This is in addition to the forward vector of the camera which already has some numbers to it.
            //We will need these stuff when account for what direction the camera is truly facing in. Working with only the camera's forward motion will not get us anywhere as we need it's rotation as well.
            float angle;
            Vector3 axis;
            playerCameraTransform.rotation.ToAngleAxis(out angle, out axis);
            if(particleFaceDirection == false)
            {
                displacementVector = new Vector3(displacementNum, 0, displacementNum);
            }
            else
            {
                displacementVector = new Vector3(-displacementNum, 0, -displacementNum);
            }
            //We do this so that it reduces the distance at which the particle is placed.
            displacementVector = -displacementVector;
            //When we have the angle + forward vector of the camera (which I think is the forward vector of the player body) it becomes the true direction the camera is facing to.
            Vector3 trueDirection = playerCameraTransform.forward * angle;
            
            //So that we don't account for sky particles. 
            trueDirection.y = 0;
            
            //This is so that we aren't outside of the field of view or are placed in an awkward position. Optimally it should (almost) face straight ahead at the player.
            /*if(trueDirection.z > trueDirection.x)
            {
                displacementVector.x = 0;
            }
            else
            {
                displacementVector.z = 0;
            }*/

            if(trueDirection.z < 0)
            {
                displacementVector.z = -displacementVector.z;
            }
            else if (trueDirection.x < 0)
            {
                displacementVector.x = -displacementVector.x;
            }
            trueDirection += displacementVector;

            addingParticles.transform.position = trueDirection ;
            
            //This gets the distance between the player position and the particle position, the thing we're rotating. One of the if statements rotates it to face the camera while the other away from it.
            if (particleFaceDirection == false)
            {
                differencePos = collisionEnterPosition - addingParticles.transform.position;
            }
            else
            {
                differencePos =  addingParticles.transform.position - collisionEnterPosition;
            }

            faceAnotherObject = new Quaternion();
            faceAnotherObject = Quaternion.LookRotation(differencePos);

            addingParticles.transform.rotation = faceAnotherObject;
        }
    }
}

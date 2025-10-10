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
    private void OnTriggerEnter(Collider other)
    {
        collisionEnterPosition = other.transform.position;
        if (other.gameObject.name == "Player")
        {
            addingParticles = Instantiate(fireflyParticles);
            
            //Gets a reference to the players Camera.
            playerCameraTransform = other.transform.GetChild(0);
            
            //This is how far away the particle system is placed. This is in addition to the forward vector of the camera which already has some numbers to it.
            displacementVector = new Vector3(displacementNum, 0, displacementNum);
            
            Vector3 trueDirection = playerCameraTransform.forward;
            
            //So that we don't account for sky particles. 
            trueDirection.y = 0;
            
            //This is so that we aren't outside of the field of view or are placed in an awkward position. Optimally it should (almost) face straight ahead at the player.
            if(trueDirection.z > trueDirection.x)
            {
                displacementVector.x = 0;
            }
            else
            {
                displacementVector.z = 0;
            }

            trueDirection += displacementVector;

            addingParticles.transform.position = trueDirection ;
            
            //This gets the distance between the player position and the particle position, the thing we're rotating.
            differencePos = collisionEnterPosition - addingParticles.transform.position;
            
            faceAnotherObject = new Quaternion();
            faceAnotherObject = Quaternion.LookRotation(differencePos);

            addingParticles.transform.rotation = faceAnotherObject;
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.VFX;

public class RadioactiveParticlesVFXManager : MonoBehaviour
{
    private const string BOUNDING_BOX_SIZE = "Bounding Box Size";
    private const string BOUNDING_BOX_POSITION = "Bounding Box Position";
    private const string BOUNDING_BOX_ORIENTATION = "Bounding Box Orientation";

    VisualEffect particlesVFX = null;

    [SerializeField]
    GameObject boundingBox; // A box that is always oriented towards the camera plane
    //[SerializeField]
    //Vector3 boundingBoxLocalScale;


    private Boolean isPlaying = false;

    private void Awake()
    {
        particlesVFX = GetComponent<VisualEffect>();
    }
    private void Start()
    {
        //boundingBox.transform.localScale = boundingBoxLocalScale;
        //boundingBoxLocalScale = particlesVFX.GetVector3("Bounoding Box Size")
    }

    private void LateUpdate()
    {
        if (isPlaying) {
            // Orient the bounding box to camera
            boundingBox.transform.LookAt(Camera.main.transform);

            // Set the properties in VFX graph
            particlesVFX.SetVector3(BOUNDING_BOX_POSITION, boundingBox.transform.position);
            particlesVFX.SetVector3(BOUNDING_BOX_ORIENTATION, boundingBox.transform.rotation.eulerAngles);
            //particlesVFX.SetVector3(BOUNDING_BOX_SIZE, boundingBox.transform.lossyScale);
        }
    }

    /// <summary>
    /// Call this function to start playing the particles
    /// </summary>
    void OnPlay()
    {
        if (!isPlaying)
        {
            particlesVFX.Reinit();
            particlesVFX.Play();
            isPlaying = true;
        }
    }


    /// <summary>
    /// Call this function to stop playing the particles
    /// </summary>
    void OnStop()
    {
        if (isPlaying)
        {
            particlesVFX?.Stop();
            isPlaying = false;
        }
    }

    
}

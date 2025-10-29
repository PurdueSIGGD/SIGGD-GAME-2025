using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMOD;
using FMODUnity;
using UnityEditor.SearchService;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed;

    [HideInInspector] public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    private EventInstance footsteps;

    [HideInInspector] public Rigidbody rb;

<<<<<<< HEAD
    // the async Start() is needed for getting the event instances set right
    private async void Start()
    {
=======
    private async void Start()
    {
        footsteps = await FMODEvents.instance.GetEventInstance("Footsteps");
        //music.start();
>>>>>>> dev
        rb = GetComponent<Rigidbody>();

        // as long as you format it like this and have it in a async Start() it should all work
        footsteps = await FMODEvents.instance.getEventInstance("Footsteps");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
<<<<<<< HEAD
            FMODEvents.instance.playOneShot("maledeath", this.transform.position);
=======
            FMODEvents.instance.playOneShot("maleDeath", this.transform.position);
>>>>>>> dev
        }

        if (transform.position.y < -50)
            transform.position = new Vector3 { x = 0, y = 5, z = 0 };

        // Testing
        if (Input.GetKeyDown(KeyCode.F))
        {
            UnityEngine.Debug.Log("it worked");
            AudioLogManager.Instance.playAudioLog("Footsteps", gameObject); // pass in the object that the audio log is gonna play at
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            UnityEngine.Debug.Log("Interuppting");
            AudioLogManager.Instance.StopCurrentAudio();
        }
    }

    private void FixedUpdate()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // this is the basic way to make any sort of continued sound events
        /*
        if (!footsteps.isValid())
        {
            footsteps = FMODEvents.instance.getEventInstance("Footsteps");
        }
        */
        

        UpdateSound();
    }

    private void UpdateSound()
    {
        if (rb.linearVelocity.magnitude != 0)
        {
            // NOTE: 3d attributes need to be set in order to play instances in 3d
            ATTRIBUTES_3D attr = AudioManager.Instance.configAttributes3D(rb.position, rb.linearVelocity, rb.linearVelocity / rb.linearVelocity.magnitude, rb.transform.up);
            footsteps.set3DAttributes(attr);

            PLAYBACK_STATE playbackState;
            footsteps.getPlaybackState(out playbackState);

            if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                footsteps.start();
            }
        }
        else
        {
            footsteps.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }
}

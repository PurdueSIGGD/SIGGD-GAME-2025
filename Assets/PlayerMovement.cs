using UnityEngine;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMOD;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    private EventInstance footsteps;
    private EventInstance music;

    private bool pauseMusic = false;

    public Rigidbody rb;

    private void Start()
    {
        //footsteps = AudioManager.instance.CreateEventInstance(FMODEvents.instance.soundEvents["Footsteps"]);
        //music = AudioManager.instance.CreateEventInstance(FMODEvents.instance.soundEvents["backgroundMusic"]);

        music.start();

        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        /*
        if (!footsteps.isValid())
        {
            footsteps = AudioManager.instance.CreateEventInstance(FMODEvents.instance.soundEvents["Footsteps"]);
            UnityEngine.Debug.Log("footsteps are made they should work?");
        }
        */


        if (Input.GetKeyDown(KeyCode.E))
        {
            //AudioManager.instance.PlayOneShot(FMODEvents.instance.enemyDeath, this.transform.position);

            //AudioManager.playSound("enemyDeath", this.transform.position);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            pauseMusic = !pauseMusic;
            UnityEngine.Debug.Log("toggle music: " + pauseMusic);

            music.setPaused(pauseMusic);
        }

        if (transform.position.y < -50)
            transform.position = new Vector3 { x = 0, y = 5, z = 0 };
    }

    private void FixedUpdate()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        UpdateSound();
    }

    private void UpdateSound()
    {
        if (rb.linearVelocity.magnitude != 0)
        {
            // NOTE: 3d attributes need to be set in order to play instances in 3d
            ATTRIBUTES_3D attr = AudioManager.instance.configAttributes3D(rb.position, rb.linearVelocity, rb.linearVelocity / rb.linearVelocity.magnitude, Vector3.up);
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

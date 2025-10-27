using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMOD;
using FMODUnity;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed;

    [HideInInspector] public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    private EventInstance footsteps;
    private EventInstance music;

    [HideInInspector] public Rigidbody rb;

    private async void Start()
    {
<<<<<<< HEAD:Assets/PlayerMovement.cs
        footsteps = await FMODEvents.instance.GetEventInstance("Footsteps");

        music = await FMODEvents.instance.initializeMusic("LevelMusic");
=======
        footsteps = AudioManager.Instance.CreateEventInstance(FMODEvents.instance.footsteps);
>>>>>>> dev:Assets/Scripts/Player/PlayerMovement.cs

        //music.start();

        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
<<<<<<< HEAD:Assets/PlayerMovement.cs
            FMODEvents.instance.playOneShot("maledeath", this.transform.position);
=======
            AudioManager.Instance.PlayOneShot(FMODEvents.instance.enemyDeath, this.transform.position);
>>>>>>> dev:Assets/Scripts/Player/PlayerMovement.cs
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

        if (!footsteps.isValid())
        {
            UnityEngine.Debug.Log("not valid");
        }
        else
        {
            UnityEngine.Debug.Log("valid");
        }

        UpdateSound();
    }

    private void UpdateSound()
    {
        if (rb.linearVelocity.magnitude != 0)
        {
            // NOTE: 3d attributes need to be set in order to play instances in 3d
            ATTRIBUTES_3D attr = AudioManager.Instance.configAttributes3D(rb.position, rb.linearVelocity, rb.linearVelocity / rb.linearVelocity.magnitude, Vector3.up);
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

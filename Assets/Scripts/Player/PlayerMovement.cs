using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMOD;

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

    private async void Start()
    {
        footsteps = await FMODEvents.instance.GetEventInstance("Footsteps");
        //music.start();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            FMODEvents.instance.playOneShot("maleDeath", this.transform.position);
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
            footsteps.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }
}

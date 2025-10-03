using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMOD;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    private EventInstance footsteps;

    public Rigidbody rb;

    private void Start()
    {
        footsteps = AudioManager.instance.CreateEventInstance(FMODEvents.instance.footsteps);

        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.E))
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.enemyDeath, this.transform.position);

            //AudioManager.playSound("enemyDeath", this.transform.position);
        }
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
            VECTOR pos = new VECTOR { x = rb.position.x, y = rb.position.y, z = rb.position.z };
            VECTOR vel = new VECTOR { x = rb.linearVelocity.x, y = rb.linearVelocity.y, z = rb.linearVelocity.z };
            VECTOR forward = new VECTOR { x = rb.linearVelocity.x / rb.linearVelocity.magnitude, y = rb.linearVelocity.y / rb.linearVelocity.magnitude, z = rb.linearVelocity.z / rb.linearVelocity.magnitude };
            VECTOR up = new VECTOR { x = 0, y = 1, z = 0 };
            ATTRIBUTES_3D attr = new ATTRIBUTES_3D { position = pos, velocity = vel, forward = forward, up = up };
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

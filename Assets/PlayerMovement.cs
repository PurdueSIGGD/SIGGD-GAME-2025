using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    private EventInstance playerFootSteps;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.E))
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.enemyDeath, this.transform.position);
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
            Debug.Log("can play sound");
            PLAYBACK_STATE playbackState;
            playerFootSteps.getPlaybackState(out playbackState);

            if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                playerFootSteps.start();
            }
        }
        else
        {
            playerFootSteps.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }
}

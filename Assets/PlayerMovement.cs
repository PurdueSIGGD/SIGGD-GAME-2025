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

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    private EventInstance footsteps;

    public Rigidbody rb;

    private void Start()
    {
        //footsteps = AudioManager.instance.CreateEventInstance(FMODEvents.getEvent("footsteps"));

        FMODEvents.instance.AssignEventTo("footsteps", (instance) =>
        {
            footsteps = instance;
            UnityEngine.Debug.Log("Footsteps assigned. Valid? " + footsteps.isValid());
        });

        //music.start();

        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.enemyDeath, this.transform.position);
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
        if (footsteps.isValid()) // make sure the instance exists
        {
            Vector3 velocity = rb.linearVelocity;

            if (velocity.magnitude > 0.1f) // moving
            {
                // Update 3D attributes
                ATTRIBUTES_3D attr = RuntimeUtils.To3DAttributes(gameObject);
                footsteps.set3DAttributes(attr);

                // Check if it's already playing
                PLAYBACK_STATE state;
                footsteps.getPlaybackState(out state);

                if (state != PLAYBACK_STATE.PLAYING)
                {
                    footsteps.start();
                }
            }
            else // stopped
            {
                PLAYBACK_STATE state;
                footsteps.getPlaybackState(out state);

                if (state == PLAYBACK_STATE.PLAYING)
                {
                    footsteps.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                }
            }
        }
    }
}

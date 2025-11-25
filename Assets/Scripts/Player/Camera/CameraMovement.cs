using Unity.Collections;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private PlayerStateMachine stateMachine;

    private Vector3 unmodifiedCameraPosition;

    private GameObject player;
    private Vector3 playerPos;

    private Vector3 offset = new(0, 0, 0);
    [SerializeField] private Vector3 fixedPos = new(0, 5, -10);

    [Header("offsets")]
    [SerializeField] private Vector3 standingOffset = new(0, 0.75f, 0);
    [SerializeField] private Vector3 crouchedOffset = new(0, -0.05f, 0);
    [SerializeField] private float offsetUpdateSpeed = 10f;

    private bool followPlayer = true;

    void Awake()
    {
        player = PlayerID.Instance.gameObject;
        stateMachine = PlayerID.Instance.stateMachine;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            followPlayer = !followPlayer;

            if (followPlayer)
                Debug.Log("camera set to follow player");
            else
            {
                fixedPos = transform.position;
                Debug.Log("camera set to fixed position");
            }
        }
    }

    void LateUpdate()
    {
        if (player == null)
        {
            Debug.LogError("player is null");
            return;
        }

        // set offset based on crouched or not
        Vector3 targetOffset = !stateMachine.IsCrouched ? standingOffset : crouchedOffset;
        offset = Vector3.Lerp(offset, targetOffset, offsetUpdateSpeed * Time.deltaTime);

        playerPos = player.transform.position;
        if (followPlayer)
        {
            unmodifiedCameraPosition = transform.position = playerPos + offset;
        }
        else
        {
            unmodifiedCameraPosition = transform.position = fixedPos;
        }
    }

    public Vector3 GetUnmodCameraPos()
    {
        return unmodifiedCameraPosition;
    } 
}

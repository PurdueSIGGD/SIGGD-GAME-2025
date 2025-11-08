using Unity.Collections;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private Vector3 unmodifiedCameraPosition;

    private GameObject player;
    private Vector3 playerPos;

    [SerializeField] private Vector3 offset = new(0, 0, 0);
    [SerializeField] private Vector3 fixedPos = new(0, 5, -10);

    private bool followPlayer = true;

    void Awake()
    {
        player = PlayerID.Instance.gameObject;
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

using Unity.Collections;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private GameObject player;
    private Vector3 playerPos;

    private Vector3 offset = new Vector3(0, 0, 0);
    private Vector3 fixedPos = new Vector3 { x = 0, y = 5, z = -10};

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
            transform.position = playerPos + offset;
        }
        else
        {
            transform.position = fixedPos;
        }
    }

}

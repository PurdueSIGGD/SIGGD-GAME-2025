using Unity.Collections;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private GameObject player;


    private Vector3 offset = new Vector3(0, 5, -5);
    private Vector3 fixedPos = new Vector3 { x = 0, y = 5, z = -10};
    private Quaternion rotation = Quaternion.AngleAxis(30, Vector3.right);

    private bool followPlayer = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            followPlayer = !followPlayer;

            if (followPlayer)
                Debug.Log("camera set to follow player");
            else
                Debug.Log("camera set to fixed position");
        }
    }

    void LateUpdate()
    {
        if (player == null)
        {
            Debug.LogError("player is null");
            return;
        }

        Vector3 playerPos = player.GetComponent<Transform>().position;

        if (followPlayer)
        {
            transform.position = playerPos + offset;
            transform.rotation = rotation;
        }
        else
        {
            transform.position = fixedPos;
            transform.rotation = rotation;
        }
    }

}

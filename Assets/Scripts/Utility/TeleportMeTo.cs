using UnityEngine;

#if UNITY_EDITOR
public class TeleportMeTo : MonoBehaviour
{
    [SerializeField] private Transform[] positions;
    private int index;

    void Update() 
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            index++;
            if (index >= positions.Length)
                index = 0;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerID.Instance.gameObject.transform.position = positions[index].position;
        }
    }

    void OnDrawGizmos() 
    {
        if (positions == null)
            return;
        foreach (var pos in positions)
        {
            Gizmos.DrawSphere(pos.position, 0.5f);
        }
    }
}
#endif
using UnityEngine;
using UnityEngine.Serialization;


/**
 * <summary>
 * A ScriptableObject to hold various parameters for rigidbody movements.
 * </summary>
 */
[CreateAssetMenu(fileName = "Move Data", menuName = "ScriptableObjects/Move Data", order = 1)]
public class MoveData : ScriptableObject
{
    [Header("Lateral Movement Parameters")]
    
    [Tooltip("Standard movement speed.")]
    public float walkSpeed = 10f;
    
    [Tooltip("Sprint movement speed.")]
    public float sprintSpeed = 15f;
    
    [Tooltip("Acceleration rate for reaching target speed.")]
    public float runAccelAmount = 7f;
    
    [Tooltip("Deceleration rate for stopping movement.")]
    public float runDecelAmount = 7f;

}

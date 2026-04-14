using UnityEngine;

public class look_at_player : MonoBehaviour
{
   public Transform headBone;
    public Transform Player;

    bool isActive = false;
    
    public float rotateSpeed = 5f;
    public float maxAngle = 60f;
    void Update() {
        Debug.DrawLine(headBone.position, Player.position, Color.red);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void LateUpdate() {
        if (Player == null || !isActive || headBone == null) return;
        Vector3 direction = Player.position - headBone.position;
        direction.y = 0f;
        if (direction == Vector3.zero) return;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        
        Quaternion newRotation = Quaternion.Slerp(
            headBone.rotation,
            targetRotation,
            Time.deltaTime * rotateSpeed
        );

        Quaternion bodyRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        float angleFromBody = Quaternion.Angle(transform.rotation, newRotation);
        if (angleFromBody > maxAngle) {
            newRotation = Quaternion.RotateTowards(
                bodyRotation, 
                newRotation, 
                maxAngle
            );
        }
        headBone.rotation = newRotation;
    }
    public void setActive(bool active) {
        isActive = active;
    }
}

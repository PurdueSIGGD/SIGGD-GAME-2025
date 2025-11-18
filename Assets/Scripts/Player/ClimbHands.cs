using UnityEngine;

public class ClimbHands : MonoBehaviour
{
    private ClimbAction climbScript;

    [Header("Armature")]
    [SerializeField] private GameObject armatureObject;

    [SerializeField] private Transform[] handTargetTransforms = new Transform[2];
    [SerializeField] private Transform[] handRestPositions = new Transform[2];
    [SerializeField] private Vector3 handRotationOffset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        climbScript = PlayerID.Instance.gameObject.GetComponent<ClimbAction>();
    }

    // Update is called once per frame
    void Update()
    {
        bool isClimbing = climbScript.IsClimbing();
        armatureObject.SetActive(isClimbing);

        if (isClimbing == true)
        {
            bool[] attachedHands = climbScript.GetAttachedHands();
            bool[] reachingHands = climbScript.GetHeldDownHands();
            Transform[] climbHandTransforms = climbScript.GetHandTransforms();

            Quaternion offsetRotation = Quaternion.Euler(handRotationOffset);

            for (int i = 0; i < 2; i++) {
                Transform ht = handTargetTransforms[i];
                if (attachedHands[i] == true) {
                    // hand reaches for climb position
                    ht.position = climbHandTransforms[i].position;
                    ht.rotation = climbHandTransforms[i].rotation * offsetRotation;
                } else if (reachingHands[i] == true) {
                    Vector3 reachPosition = transform.forward * 50f + transform.position;
                    // hand reaches for climb position
                    ht.position = reachPosition;
                    ht.rotation = offsetRotation;
                } else {
                    // hand reaches for rest position
                    ht.position = handRestPositions[i].position;
                    ht.rotation = handRestPositions[i].rotation * offsetRotation;
                }
            }

            /*leftHandTarget.position = climbHandTransforms[0].position;
            leftHandTarget.rotation = climbHandTransforms[0].rotation * offsetRotation;

            rightHandTarget.position = climbHandTransforms[1].position;
            rightHandTarget.rotation = climbHandTransforms[1].rotation * offsetRotation;*/
        } else
        {

        }
    }
}

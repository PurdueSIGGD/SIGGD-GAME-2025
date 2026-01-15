using UnityEngine;

public class ClimbHands : MonoBehaviour
{
    private ClimbAction climbScript;

    [Header("Instantiated Stuff")]
    [SerializeField] private Transform[] armRoots;
    [Range(0.5f, 10f)] [SerializeField] private float armRootScale = 3f;

    [Header("Armature")]
    [SerializeField] private GameObject armatureObject;

    [SerializeField] private Transform handRootTransform;
    [SerializeField] private Transform handRootTransformSocket;

    [Header("Arrays of size 2 that describe target IK, resting, and hidden positions for hands")]
    [SerializeField] private Transform[] handTargetTransforms = new Transform[2];
    [SerializeField] private Transform[] handRestPositions = new Transform[2];
    [SerializeField] private Transform[] handHiddenPositions = new Transform[2];

    [Header("Interpolation Speeds")]
    [Tooltip("Speed that the hand target generally moves at")]
    [SerializeField] private float interpolationSpeed = 10f;
    [Tooltip("Speed that the hand target moves at when attached to a wall")]
    [SerializeField] private float interpolationSpeed_attached = 30f;
    [Tooltip("Speed that the hand target moves at when 'hidden'")]
    [SerializeField] private float interpolationSpeed_hidden = 10f;

    [Header("Offsets")]
    [SerializeField] private Vector3 handRotationOffset = new Vector3(0, 0, 20);
    [SerializeField] private Vector3 handRotationOffsetReaching = new Vector3(0, 180, 30);
    [SerializeField] [Range(0f, 1f)] private float reachHorizontalOffset = 0.5f;
    [SerializeField] private float reachMagnitude = 5f;

    [Header("Hidden Delay")]
    [Tooltip("After this amount of time, the hands disappear when you exit climbing mode.")]
    [SerializeField] private float hiddenDisappearDelay = 3f;
    private float hiddenDisappearTimer = -99f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        climbScript = PlayerID.Instance.gameObject.GetComponent<ClimbAction>();

        for (int i = 0; i < handTargetTransforms.Length; i++)
        {
            handTargetTransforms[i].SetParent(null);
        }

        for (int i = 0; i < armRoots.Length; i++)
        {
            armRoots[i].localScale = Vector3.one * armRootScale;
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool isClimbing = climbScript.IsClimbing();

        if (isClimbing == true) {
            hiddenDisappearTimer = 0;
        } else {
            hiddenDisappearTimer -= Time.deltaTime;
        }

        armatureObject.SetActive(hiddenDisappearTimer > -hiddenDisappearDelay);

        if (isClimbing == true) {
            bool[] attachedHands = climbScript.GetAttachedHands();
            bool[] reachingHands = climbScript.GetHeldDownHands();
            Transform[] climbHandTransforms = climbScript.GetHandTransforms();


            for (int i = 0; i < 2; i++)
            {
                Quaternion offsetRotation = Quaternion.Euler(new Vector3(handRotationOffset.x, handRotationOffset.y, handRotationOffset.z * (i == 1 ? 1 : -1)));

                Transform ht = handTargetTransforms[i];

                // by default, hand goes for its rest position
                Vector3 targetHandPosition = handRestPositions[i].position;
                Quaternion targetHandRotation = handRestPositions[i].rotation * offsetRotation;
                float interpSpeed = interpolationSpeed;


                if (attachedHands[i] == true)
                {
                    // hand reaches for climb position
                    targetHandPosition = climbHandTransforms[i].position;
                    targetHandRotation = climbHandTransforms[i].rotation * offsetRotation;
                    interpSpeed = interpolationSpeed_attached;
                }
                else if (reachingHands[i] == true)
                {
                    Vector3 reachPosition = handRootTransformSocket.forward * reachMagnitude
                        + handRootTransformSocket.right * reachHorizontalOffset * (i == 1 ? 1 : -1)
                        + handRootTransformSocket.position;
                    // hand reaches for climb position
                    targetHandPosition = reachPosition;
                    targetHandRotation = handRootTransformSocket.rotation
                        * Quaternion.Euler(new Vector3(handRotationOffsetReaching.x, handRotationOffsetReaching.y, handRotationOffsetReaching.z * (i == 1 ? 1 : -1)));
                }


                ht.position = Vector3.Lerp(ht.position, targetHandPosition, Time.deltaTime * interpSpeed);
                ht.rotation = Quaternion.Lerp(ht.rotation, targetHandRotation, Time.deltaTime * interpSpeed);

            }
        } else {
            for (int i = 0; i < 2; i++)
            {
                Quaternion offsetRotation = Quaternion.Euler(new Vector3(handRotationOffset.x, handRotationOffset.y, handRotationOffset.z * (i == 1 ? 1 : -1)));

                Transform ht = handTargetTransforms[i];

                Vector3 targetHandPosition = handHiddenPositions[i].position;
                Quaternion targetHandRotation = handHiddenPositions[i].rotation * offsetRotation;
                float interpSpeed = interpolationSpeed_hidden;


                ht.position = Vector3.Lerp(ht.position, targetHandPosition, Time.deltaTime * interpSpeed);
                ht.rotation = Quaternion.Lerp(ht.rotation, targetHandRotation, Time.deltaTime * interpSpeed);

            }
        }
    }
}

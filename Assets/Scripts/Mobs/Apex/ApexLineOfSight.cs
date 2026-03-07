using UnityEngine;

/// <summary>
/// Independent line-of-sight component for the Apex. Rather than following the
/// head bone, this transform is the authority — it positions itself at the head
/// bone's world position each frame and its rotation is driven externally (by the
/// state machine). The head bone is then made to match the LOS rotation so the
/// mesh visually follows the scan direction.
/// </summary>
public class ApexLineOfSight : MonoBehaviour
{
    #region Inspector Fields

    [Header("Head Tracking")]
    [Tooltip("The head bone that will visually follow this LOS transform's rotation.")]
    [SerializeField] private Transform headBone;
    [Tooltip("Rotation offset applied when writing back to the head bone, to compensate for rig axis mismatches " +
             "(e.g. set Y to 180 if the head faces backward).")]
    [SerializeField] private Vector3 headBoneCorrectionEuler = Vector3.zero;

    [Header("Detection Settings")]
    [Tooltip("How far the Apex can see (horizontal radius of the cylinder).")]
    [SerializeField] private float viewRadius = 20f;
    [Tooltip("Half-angle of the horizontal vision cone in degrees (e.g. 60 = 120 degree FOV).")]
    [Range(0f, 180f)]
    [SerializeField] private float halfAngle = 60f;
    [Tooltip("How far above this transform's Y position a target can be and still be seen.")]
    [SerializeField] private float heightAbove = 3f;
    [Tooltip("How far below this transform's Y position a target can be and still be seen.")]
    [SerializeField] private float heightBelow = 1.5f;
    [Tooltip("Layers that block line of sight (walls, terrain, etc).")]
    [SerializeField] private LayerMask obstacleMask;
    [Tooltip("Layers that contain potential targets.")]
    [SerializeField] private LayerMask targetMask;

    #endregion

    #region Public API

    /// <summary>The target currently visible to the Apex, or null if none.</summary>
    public ApexTarget VisibleTarget { get; private set; }

    /// <summary>
    /// Sets the LOS local rotation directly. The state machine calls this each
    /// frame to drive the sweep. The head bone will follow automatically.
    /// </summary>
    public void SetLocalRotation(Quaternion localRotation)
    {
        transform.localRotation = localRotation;
    }

    /// <summary>Resets the LOS (and therefore the head bone) back to the neutral local rotation.</summary>
    public void ResetRotation()
    {
        transform.localRotation = Quaternion.identity;
    }

    #endregion

    #region Unity Callbacks

    private void LateUpdate()
    {
        // Keep this transform anchored at the head bone's world position each frame,
        // but preserve our own rotation (which the state machine drives).
        if (headBone != null)
            transform.position = headBone.position;

        // Drive the head bone to match our rotation, with a correction for the rig's axis orientation.
        if (headBone != null)
            headBone.rotation = transform.rotation * Quaternion.Euler(headBoneCorrectionEuler);

        VisibleTarget = CheckForTargets();
    }

    #endregion

    #region Detection

    private ApexTarget CheckForTargets()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        foreach (Collider col in hits)
        {
            if (col == null) continue;
            ApexTarget candidate = col.GetComponent<ApexTarget>();
            if (candidate == null) continue;

            Vector3 toTarget = col.transform.position - transform.position;

            // Height band check — reject anything outside the vertical window.
            if (toTarget.y > heightAbove || toTarget.y < -heightBelow) continue;

            // Horizontal angle check — flatten to XZ plane before measuring angle.
            Vector3 flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            Vector3 flatToTarget = new Vector3(toTarget.x, 0f, toTarget.z).normalized;
            if (Vector3.Angle(flatForward, flatToTarget) > halfAngle) continue;

            // Obstacle raycast from eye position toward the target.
            float dist = Vector3.Distance(transform.position, col.transform.position);
            Vector3 dirToTarget = toTarget.normalized;
            if (Physics.Raycast(transform.position, dirToTarget, dist, obstacleMask)) continue;

            return candidate;
        }

        return null;
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        Vector3 flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 leftBound = Quaternion.Euler(0f, -halfAngle, 0f) * flatForward * viewRadius;
        Vector3 rightBound = Quaternion.Euler(0f, halfAngle, 0f) * flatForward * viewRadius;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBound);
        Gizmos.DrawLine(transform.position, transform.position + rightBound);
        Gizmos.DrawLine(transform.position + leftBound, transform.position + rightBound);

        Vector3 aboveOffset = Vector3.up * heightAbove;
        Vector3 belowOffset = Vector3.down * heightBelow;
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.35f);
        Gizmos.DrawLine(transform.position + aboveOffset, transform.position + aboveOffset + leftBound);
        Gizmos.DrawLine(transform.position + aboveOffset, transform.position + aboveOffset + rightBound);
        Gizmos.DrawLine(transform.position + belowOffset, transform.position + belowOffset + leftBound);
        Gizmos.DrawLine(transform.position + belowOffset, transform.position + belowOffset + rightBound);
        Gizmos.DrawLine(transform.position + leftBound + aboveOffset, transform.position + leftBound + belowOffset);
        Gizmos.DrawLine(transform.position + rightBound + aboveOffset, transform.position + rightBound + belowOffset);
    }

    #endregion
}

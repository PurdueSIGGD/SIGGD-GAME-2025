using UnityEngine;

/// <summary>
/// Broadcasts quest completion when target tranform is rendered to player camera
/// </summary>
public class OnRenderConditionStrategy : IQuestConditionStrategy
{
    private Camera playerCam;
    [Tooltip("Should be the transform of the parent GameObject")]
    [SerializeField] Transform targetTransform;
    [Tooltip("These layers will be ignored while checking for render")]
    [SerializeField] LayerMask ignoreMask;

    private bool found = false;
    private Plane[] frustumPlanes;
    private Bounds boxBounds;
    protected override void OnInitialize()
    {
        base.OnInitialize();
        frustumPlanes = new Plane[6];
        boxBounds = new Bounds(targetTransform.position, Vector3.one);
        playerCam = PlayerID.Instance.cam.GetComponentInChildren<Camera>();
    }
    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (found) return; // pause execution on found

        GeometryUtility.CalculateFrustumPlanes(playerCam, frustumPlanes);
        if (GeometryUtility.TestPlanesAABB(frustumPlanes, boxBounds))
        {
            if (!Physics.Linecast(playerCam.transform.position, targetTransform.position, ~ignoreMask))
            {
                if (!Physics.Linecast(playerCam.transform.position, targetTransform.position, ~ignoreMask))
                found = true;
                Broadcast(Broadcaster);
            }
        }
    }

    public override bool Evaluate()
    {
        return found;
    }

    public override bool StopIfTriggered()
    {
        return true;
    }
}

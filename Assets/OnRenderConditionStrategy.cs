using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.UI;

public class OnRenderConditionStrategy : IQuestConditionStrategy
{
    private Camera playerCam;
    [Tooltip("Should be the transform of the parent GameObject")]
    [SerializeField] Transform targetTransform;
    [Tooltip("These layers will be ignored while checking for render")]
    [SerializeField] LayerMask ignoreMask;
    private bool found = false;
    Bounds boxBounds;
    protected override void OnInitialize()
    {
        base.OnInitialize();
        boxBounds = new Bounds(targetTransform.position, Vector3.one);
        playerCam = PlayerID.Instance.cam.GetComponentInChildren<Camera>();
    }
    protected override void OnUpdate()
    {
        base.OnUpdate();
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCam);
        if (GeometryUtility.TestPlanesAABB(planes, boxBounds)) {
            if (ignoreMask == default)
            {
                if (!Physics.Linecast(playerCam.transform.position, targetTransform.position))
                {
                    found = true;
                }
            }
            else
            {
                if (!Physics.Linecast(playerCam.transform.position, targetTransform.position, ~ignoreMask))
                {
                    found = true;
                }
            }
            
        }
        Broadcast(Broadcaster);
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

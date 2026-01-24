using UnityEngine;

public class OnRenderConditionStrategy : IQuestConditionStrategy
{
    [SerializeField] MeshRenderer mesh;
    private Camera playerCam;
    private bool found = false;
    protected override void OnInitialize()
    {
        base.OnInitialize();
        playerCam = PlayerID.Instance.cam.GetComponentInChildren<Camera>();
        Debug.Log("set cam");
    }
    protected override void OnUpdate()
    {
        base.OnUpdate();
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCam);
        found = GeometryUtility.TestPlanesAABB(planes, mesh.bounds);
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

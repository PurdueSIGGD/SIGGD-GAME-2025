using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.UI;

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
        //int mask1 = LayerMask.GetMask("Player");
        LayerMask mask = LayerMask.GetMask("Player");
        if (GeometryUtility.TestPlanesAABB(planes, mesh.bounds)) {
            if (!Physics.Linecast(playerCam.transform.position + playerCam.transform.forward, mesh.transform.position))
            {
                
                found = true;
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

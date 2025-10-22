using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using SIGGD.Goap.Interfaces;
using UnityEngine;
using SIGGD.Goap;

namespace SIGGD.Goap
{
    [GoapId("AttackPlayer-2c954894-c298-4582-9c7d-7f3e4c6728d6")]
    public class AttackPlayerAction : GoapActionBase<CommonData>
    {
        // This method is called when the action is created
        // This method is optional and can be removed
        public override void Created()
        {
        }

        // This method is called every frame before the action is performed
        // If this method returns false, the action will be stopped
        // This method is optional and can be removed
        public override bool IsValid(IActionReceiver agent, CommonData data)
        {
            return true;
        }

        // This method is called when the action is started
        // This method is optional and can be removed
        public override void Start(IMonoAgent agent, CommonData data)
        {
            data.Timer = 2f;
        }

        // This method is called once before the action is performed
        // This method is optional and can be removed
        public override void BeforePerform(IMonoAgent agent, CommonData data)
        {
        }

        // This method is called every frame while the action is running
        // This method is required
        public override IActionRunState Perform(IMonoAgent agent, CommonData data, IActionContext context)
        {
            data.Timer -= context.DeltaTime;
            if (Vector3.Distance(data.Target.Position, agent.Transform.position) <= 5)
            {
                data.animator.SetTrigger("Attack");
            }
            return ActionRunState.Completed;
        }



    }
}
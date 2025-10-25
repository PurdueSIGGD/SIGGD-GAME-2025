using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Agent.Runtime;
using UnityEngine;
using SIGGD.Goap.Interfaces;
using SIGGD.Goap.Behaviours;

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
            return data.Target != null;
        }

        // This method is called when the action is started
        // This method is optional and can be removed
        public override void Start(IMonoAgent agent, CommonData data)
        {
            data.Timer = 20f;
        }
        public override void BeforePerform(IMonoAgent agent, CommonData data)
        {
            if (Vector3.Distance(data.Target.Position, agent.Transform.position) <= 15)
            {
                data.am.StartAttackSequence(data.Target.Position);
                data.am.isLunging = true;
            }
        }

        public override IActionRunState Perform(IMonoAgent agent, CommonData data, IActionContext context)
        {
            if (!data.am.isLunging) return ActionRunState.Completed;
            return ActionRunState.Continue;
        }
        public override void Stop(IMonoAgent agent, CommonData data)
        {
        }
        public override void End(IMonoAgent agent, CommonData data)
        {
            this.Disable(agent, ActionDisabler.ForTime(2f));

        }
    }


}
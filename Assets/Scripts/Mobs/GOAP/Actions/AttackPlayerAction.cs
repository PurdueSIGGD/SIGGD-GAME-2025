using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Agent.Runtime;
using UnityEngine;
using SIGGD.Goap.Interfaces;
using System.Runtime.InteropServices.WindowsRuntime;

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
            return data.pm.CanSeePlayer;
        }

        // This method is called when the action is started
        // This method is optional and can be removed
        public override void Start(IMonoAgent agent, CommonData data)
        {
            data.Timer = 20f;
            data.mv.EnableSprint();
        }
        public override void BeforePerform(IMonoAgent agent, CommonData data)
        {
            float distance = Vector3.Distance(data.Target.Position, agent.Transform.position);
            if (distance <= 100 && distance > 0)
            {
                if (!data.am.isLunging)
                {
                    data.am.StartAttackSequence(agent);
                    data.am.SetTarget(data.Target as TransformTarget);
                    data.am.isLunging = true;
                }
            }
        }

        public override IActionRunState Perform(IMonoAgent agent, CommonData data, IActionContext context)
        {
            if (!data.am.isLunging)
            {
                return ActionRunState.Completed;
            }
            return ActionRunState.Continue;
        }
        public override void Stop(IMonoAgent agent, CommonData data)
        {
        }
        public override void End(IMonoAgent agent, CommonData data)
        {
            data.mv.DisableSprint();
            //this.Disable(agent, ActionDisabler.ForTime(0.5f));

        }
    }


}
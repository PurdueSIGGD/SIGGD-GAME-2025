using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Agent.Runtime;
using UnityEngine;
using SIGGD.Goap.Interfaces;

namespace SIGGD.Goap
{
    [GoapId("KillPrey-28433bcc-6bfe-451a-887f-32d47769d859")]
    public class KillPreyAction : GoapActionBase<CommonData>
    {
        public override void Created()
        {
        }

        public override bool IsValid(IActionReceiver agent, CommonData data)
        {
            return data.Target != null;
        }


        public override void Start(IMonoAgent agent, CommonData data)
        {
            data.Timer = 20f;
        }
        public override void BeforePerform(IMonoAgent agent, CommonData data)
        {
        }

        public override IActionRunState Perform(IMonoAgent agent, CommonData data, IActionContext context)
        {
            data.Timer -= context.DeltaTime;
            if (Vector3.Distance(data.Target.Position, agent.Transform.position) <= 5)
            {
                data.animator.SetTrigger("Attack");
            }
            return ActionRunState.Completed;
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
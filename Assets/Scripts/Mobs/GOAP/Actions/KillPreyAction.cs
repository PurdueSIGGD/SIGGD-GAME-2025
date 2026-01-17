using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Agent.Runtime;
using UnityEngine;
using SIGGD.Goap.Interfaces;
using SIGGD.Mobs;

namespace SIGGD.Goap
{
    [GoapId("KillPrey-28433bcc-6bfe-451a-887f-32d47769d859")]
    public class KillPreyAction : GoapActionBase<CommonData> , IActionRunState, IInjectable
    {
        public override void Created()
        {
        }

        public override bool IsValid(IActionReceiver agent, CommonData data)
        {

            return true;
        }


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
            this.Disable(agent, ActionDisabler.ForTime(0.0f));
               
        }

        public void Update(IAgent agent, IActionContext context)
        {
            throw new System.NotImplementedException();
        }

        public bool ShouldStop(IAgent agent)
        {
            throw new System.NotImplementedException();
        }

        public bool ShouldPerform(IAgent agent)
        {
            throw new System.NotImplementedException();
        }

        public bool IsCompleted(IAgent agent)
        {
            throw new System.NotImplementedException();
        }

        public bool MayResolve(IAgent agent)
        {
            throw new System.NotImplementedException();
        }

        public bool IsRunning()
        {
            throw new System.NotImplementedException();
        }

        public void Inject(GoapInjector injector)
        {
            throw new System.NotImplementedException();
        }
    }
}
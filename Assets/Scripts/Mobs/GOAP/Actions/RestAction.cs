using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using System.Threading;
using CrashKonijn.Agent.Runtime;
using UnityEngine;

namespace SIGGD.Goap
{
    [GoapId("Rest-04e4a6ed-6f2a-4909-b11c-8e4859f05b07")]
    public class RestAction : GoapActionBase<RestAction.Data>
    {
        // This method is called when the action is created
        // This method is optional and can be removed
        public override void Created()
        {
        }

        // This method is called every frame before the action is performed
        // If this method returns false, the action will be stopped
        // This method is optional and can be removed
        public override bool IsValid(IActionReceiver agent, Data data)
        {
            return true;
        }

        // This method is called when the action is started
        // This method is optional and can be removed
        public override void Start(IMonoAgent agent, Data data)
        {
            data.Timer = 10f;
        }

        // This method is called once before the action is performed
        // This method is optional and can be removed
        public override void BeforePerform(IMonoAgent agent, Data data)
        {
        }

        // This method is called every frame while the action is running
        // This method is required
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            data.Timer -= context.DeltaTime;
            if (data.Timer < 0 || data.hm.CurrentHealth >= data.hm.MaxHealth) return ActionRunState.Completed;
            data.hm.Heal(context.DeltaTime * 2f, agent.gameObject, "rested and healed");
            return ActionRunState.ContinueOrResolve;
        }

        // This method is called when the action is completed
        // This method is optional and can be removed
        public override void Complete(IMonoAgent agent, Data data)
        {
        }

        // This method is called when the action is stopped
        // This method is optional and can be removed
        public override void Stop(IMonoAgent agent, Data data)
        {
        }

        // This method is called when the action is completed or stopped
        // This method is optional and can be removed
        public override void End(IMonoAgent agent, Data data)
        {
        }

        // The action class itself must be stateless!
        // All data should be stored in the data class
        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            [GetComponent]
            public EntityHealthManager hm { get; set; }

            public float Timer { get; set; }

        }
    }
}
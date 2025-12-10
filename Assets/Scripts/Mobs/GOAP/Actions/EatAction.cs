using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Agent.Runtime;
using UnityEngine;
using SIGGD.Mobs;

namespace SIGGD.Goap
{
    [GoapId("Eat-f2b62a8b-2164-4925-b561-419a43bc3c20")]
    public class EatAction : GoapActionBase<EatAction.Data>
    {
        public override void Created()
        {

        }

        public override bool IsValid(IActionReceiver agent, Data data)
        {
            return data.Target != null;
        }
        public override void Start(IMonoAgent agent, Data data)
        {

        }
        public override void BeforePerform(IMonoAgent agent, Data data)
        {
        }
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            return ActionRunState.WaitThenComplete(4f);

        }
        public override void Complete(IMonoAgent agent, Data data)
        {
            if (data.Target is not TransformTarget transformTarget) {
                Debug.Log("not eatable");
                return;
            }
            // nutrition check either here or in hunger behaviour
            data.HungerBehaviour.ReduceHunger(60);
            GameObject.Destroy(transformTarget.Transform.gameObject);
    }

        public override void Stop(IMonoAgent agent, Data data)
        {
        }

        public override void End(IMonoAgent agent, Data data)
        {

        }
        public class Data : IActionData
        {
            [GetComponent]
            public HungerBehaviour HungerBehaviour { get; set; }
            public ITarget Target { get; set; }
            public float Timer { get; set; }
        }
        public bool ShouldPerform(IAgent agent) => true;
    }
}
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using SIGGD.Goap.Interfaces;
using SIGGD.Goap.Config;

namespace SIGGD.Goap.Sensors
{
    public class PlayerTargetSensor : LocalTargetSensorBase, IInjectable
    {
        private Collider[] colliders = new Collider[1];
        private BaseStatConfig statConfig;
        private LayerMask playerLayer;
        public override void Created()
        {
            playerLayer = LayerMask.GetMask("Player");
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            if (Physics.OverlapSphereNonAlloc(agent.Transform.position, 70, colliders, playerLayer) > 0)
            {
                return new TransformTarget(colliders[0].transform);
            }
            return null;
        }

        public override void Update()
        {

        }
        public void Inject(GoapInjector injector)
        {
            statConfig = injector.BaseStatConfig;
        }

    }
}
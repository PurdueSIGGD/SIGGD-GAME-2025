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
        private BaseStats stats;
        public override void Created()
        {
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            var perceptionManager = references.GetCachedComponent<PerceptionManager>();
            if (perceptionManager == null) return null;

            if (perceptionManager.CanSeePlayer && perceptionManager.PlayerTarget != null)
            {
                return new TransformTarget(perceptionManager.PlayerTarget);
            }
            return null;
        }

        public override void Update()
        {

        }
        public void Inject(GoapInjector injector)
        {
            stats = injector.BaseStats;
        }

    }
}
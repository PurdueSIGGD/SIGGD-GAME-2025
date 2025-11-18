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
        private PerceptionManager perceptionManager;
        public override void Created()
        {
        }

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            perceptionManager = references.GetCachedComponent<PerceptionManager>();
            if (perceptionManager.CanSeePlayer)
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
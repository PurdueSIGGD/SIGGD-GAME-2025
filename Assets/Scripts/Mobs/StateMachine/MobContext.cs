using SIGGD.Mobs.Hyena;
using SIGGD.Mobs.PackScripts;
using UnityEngine;
using UnityEngine.AI;

namespace SIGGD.Mobs.StateMachine
{
    public class MobContext
    {
        public Transform Transform;
        public Rigidbody Rigidbody;
        public NavMeshAgent NavAgent;
        public Movement Movement;
        public AgentData AgentData;
        public HungerBehaviour Hunger;
        public PackBehavior Pack;
        public MobType Type;
        public Animator Animator;

        // Optional — set only on mobs that have these components
        public PerceptionManager Perception;
        public HyenaAttackManager AttackManager;
        public AgentHuntBehaviour HuntBehaviour;
        public PreyBehaviour PreyBehaviour;
        public Smell Smell;
    }

    public enum MobType
    {
        Hyena,
        Prey,
        Apex,
        Villager
    }
}

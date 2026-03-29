using SIGGD.Mobs.PackScripts;
using UnityEngine;

namespace SIGGD.Mobs.StateMachine.States
{
    public class FollowPackState : IMobState
    {
        private readonly MobContext ctx;
        private PackBehavior alphaTarget;

        public FollowPackState(MobContext context)
        {
            ctx = context;
        }

        public void Enter()
        {
            ctx.Movement.DisableSprint();
            UpdateAlpha();
        }

        public void Update()
        {
            UpdateAlpha();
        }

        public void FixedUpdate()
        {
            if (alphaTarget == null) return;

            Vector3 alphaPos = alphaTarget.transform.position;
            float dist = Vector3.Distance(ctx.Rigidbody.position, alphaPos);

            if (dist < 3f) return;

            Vector3 dir = NavSteering.GetSteeringDirection(
                ctx.NavAgent, ctx.Rigidbody.position, alphaPos, 0.1f);
            ctx.Movement.MoveTowards(dir, 1.0f, 3f, false);
        }

        public void Exit()
        {
            alphaTarget = null;
        }

        public bool HasValidAlpha => alphaTarget != null && alphaTarget != ctx.Pack;

        private void UpdateAlpha()
        {
            if (ctx.Pack == null) { alphaTarget = null; return; }

            var pack = ctx.Pack.GetPack();
            if (pack == null) { alphaTarget = null; return; }

            var alpha = pack.GetAlpha();
            if (alpha == null || alpha == ctx.Pack)
            {
                alphaTarget = null;
                return;
            }
            alphaTarget = alpha;
        }
    }
}

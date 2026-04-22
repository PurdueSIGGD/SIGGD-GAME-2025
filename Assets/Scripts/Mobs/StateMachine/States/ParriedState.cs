using System.Collections;
using UnityEngine;

namespace SIGGD.Mobs.StateMachine
{
    public class ParriedState : IMobState
    {
        private MobContext ctx;
        private Vector3 direction;
        private float knockbackForce;
        private float knockbackDuration;
        
        private Coroutine knockbackRoutine;

        public bool finished = false;
        
        public ParriedState(MobContext context, float knockbackForce, float knockbackDuration)
        {
            finished = false;
            ctx = context;
            this.knockbackForce = knockbackForce;
            this.knockbackDuration = knockbackDuration;
        }

        public void SetDirection(Vector3 direction)
        {
            this.direction = direction;
        }
        
        public void Enter()
        {
            finished = false;
            knockbackRoutine ??= ctx.Movement.StartCoroutine(ApplyKnockback());
        }

        private void Finish()
        {
            if (knockbackRoutine != null)
            {
                ctx.Movement.StopCoroutine(knockbackRoutine);
                knockbackRoutine = null;
            }

            finished = true;
        }

        private IEnumerator ApplyKnockback()
        {
            ctx.NavAgent.enabled = false;
            
            Vector3 knockbackDirection = -direction.normalized * knockbackForce;
            
            ctx.Rigidbody.linearVelocity = Vector3.zero;
            
            ctx.Rigidbody.AddForce(knockbackDirection, ForceMode.VelocityChange);
            
            yield return new WaitForSeconds(knockbackDuration);
            
            ctx.Rigidbody.linearVelocity = Vector3.zero;
            
            ctx.NavAgent.enabled = true;
            
            Finish();
        }

        public void Update()
        {
        }

        public void FixedUpdate()
        {
        }

        public void Exit()
        {
            knockbackRoutine = null;
            direction = Vector3.zero;
            finished = false;
        }
    }
}
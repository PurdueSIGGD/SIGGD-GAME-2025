using UnityEngine;
using UnityEngine.AI;

namespace SIGGD.Mobs.StateMachine.States
{
    public class SeekFoodState : IMobState
    {
        private readonly MobContext ctx;

        private Transform foodTarget;
        private float eatTimer;
        private bool isEating;
        private float searchCooldown;

        private const float EatDuration = 4f;
        private const float EatDistance = 2.5f;
        private const float HungerReduction = 60f;
        private const float SearchInterval = 2f;

        public SeekFoodState(MobContext context)
        {
            ctx = context;
        }

        public void Enter()
        {
            isEating = false;
            eatTimer = 0f;
            searchCooldown = 0f;
            ctx.Movement.DisableSprint();
            FindClosestFood();
        }

        public void Update()
        {
            if (isEating)
            {
                eatTimer -= Time.deltaTime;
                if (eatTimer <= 0f)
                {
                    ConsumeFood();
                }
                return;
            }

            if (foodTarget == null || !foodTarget.gameObject.activeInHierarchy)
            {
                searchCooldown -= Time.deltaTime;
                if (searchCooldown <= 0f)
                {
                    FindClosestFood();
                    searchCooldown = SearchInterval;
                }
                return;
            }

            float dist = Vector3.Distance(ctx.Rigidbody.position, foodTarget.position);
            if (dist <= EatDistance)
            {
                isEating = true;
                eatTimer = EatDuration;
            }
        }

        public void FixedUpdate()
        {
            if (isEating || foodTarget == null) return;

            Vector3 dir = NavSteering.GetSteeringDirection(
                ctx.NavAgent, ctx.Rigidbody.position, foodTarget.position, 0.1f);
            ctx.Movement.MoveTowards(dir, 1.0f, 3f, false);
        }

        public void Exit()
        {
            foodTarget = null;
            isEating = false;
        }

        public bool HasFood => foodTarget != null;

        private void FindClosestFood()
        {
            foodTarget = ctx.Smell != null ? ctx.Smell.ClosestFood : null;
        }

        private void ConsumeFood()
        {
            if (foodTarget != null && ctx.Hunger != null)
            {
                ctx.Hunger.ReduceHunger(HungerReduction);
                Object.Destroy(foodTarget.gameObject);
            }
            foodTarget = null;
            isEating = false;
        }
    }
}

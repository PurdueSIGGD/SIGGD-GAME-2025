using UnityEngine;

namespace Extensions.StateMachine
{
    public abstract class State
    {
        public bool doNotRemove = false;

        public virtual void OnStateEnter(MonoBehaviour parent)
        {
            OnEnter();
        }

        public virtual void OnEnter()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnFixedUpdate()
        {
        }

        public virtual void OnLateUpdate()
        {
        }

        public virtual void OnExit()
        {
        }

        public virtual void OnInterrupt()
        {
        }

        public virtual void OnResume()
        {
        }

        public virtual void OnTriggerEnter(Collider other)
        {
        }

        public virtual void OnCollisionEnter(Collision collision)
        {
        }
    }
}
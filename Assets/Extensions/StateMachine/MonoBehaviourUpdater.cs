using System;
using UnityEngine;

namespace Extensions.StateMachine
{
    public class MonoBehaviourUpdater : MonoBehaviour
    {
        private MonoBehaviourUpdatable updatable;

        public void SetUpdatable(MonoBehaviourUpdatable updatable)
        {
            this.updatable = updatable;
        }

        private void Update()
        {
            updatable?.Update();
        }

        private void FixedUpdate()
        {
            updatable?.FixedUpdate();
        }

        private void LateUpdate()
        {
            updatable?.LateUpdate();
        }

        public void RemoveUpdatable()
        {
            updatable = null;
        }

        public bool HasUpdatable()
        {
            return updatable != null;
        }

        private void OnTriggerEnter(Collider other)
        {
            updatable?.OnTriggerEnter(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            updatable?.OnCollisionEnter(collision);
        }
    }

    public abstract class MonoBehaviourUpdatable
    {
        private MonoBehaviourUpdater updater;
        private GameObject gameObject;

        protected MonoBehaviourUpdatable()
        {
            gameObject = new GameObject();
            gameObject.name = GetType().Name;

            updater = gameObject.AddComponent<MonoBehaviourUpdater>();
            updater.SetUpdatable(this);
        }

        protected MonoBehaviourUpdatable(GameObject g)
        {
            gameObject = g;
            updater = g.AddComponent<MonoBehaviourUpdater>();
            updater.SetUpdatable(this);
        }

        public virtual void Update()
        {
        }

        public virtual void FixedUpdate()
        {
        }

        public virtual void LateUpdate()
        {
        }

        public virtual void OnTriggerEnter(Collider other)
        {
        }

        public virtual void OnCollisionEnter(Collision collision)
        {
        }

        public void Destroy()
        {
            GameObject.Destroy(gameObject);
        }
    }
}

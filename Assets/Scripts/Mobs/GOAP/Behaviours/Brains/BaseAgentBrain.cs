using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using SIGGD.Mobs;
using Unity.VisualScripting;
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;

namespace SIGGD.Goap.Behaviours
{
    public abstract class BaseAgentBrain : MonoBehaviour
    {
        protected AgentBehaviour agent;
        protected GoapActionProvider provider;
        protected GoapBehaviour goap;
        protected string agentType;

        protected virtual void Awake()
        {
            this.goap = FindFirstObjectByType<GoapBehaviour>();
            this.agent = this.GetComponent<AgentBehaviour>();
            this.provider = this.GetComponent<GoapActionProvider>();
        }
        protected virtual void OnEnable()
        {
            this.provider.Events.OnActionStart += this.OnActionStart;
            this.provider.Events.OnActionStop += this.OnActionStop;
            this.provider.Events.OnActionEnd += this.OnActionEnd;
            this.provider.Events.OnActionComplete += this.OnActionCompleted;
            this.provider.Events.OnGoalStart += this.OnGoalStart;
            this.provider.Events.OnGoalCompleted += this.OnGoalCompleted;
            this.provider.Events.OnNoActionFound += this.OnNoActionFound;
        }
        protected virtual void OnDisable()
        {
            this.provider.Events.OnActionStart -= this.OnActionStart;
            this.provider.Events.OnActionStop -= this.OnActionStop;
            this.provider.Events.OnActionEnd -= this.OnActionEnd;
            this.provider.Events.OnActionComplete -= this.OnActionCompleted;
            this.provider.Events.OnGoalStart -= this.OnGoalStart;
            this.provider.Events.OnGoalCompleted -= this.OnGoalCompleted;
            this.provider.Events.OnNoActionFound -= this.OnNoActionFound;
        }
        public void SetAgentType(string mobId)
        {
            this.agentType = mobId;
            this.provider.AgentType = this.goap.GetAgentType(mobId);
        }
        protected abstract void Start();

        protected virtual void OnActionStart(IAction action)
        {
        }
        protected virtual void OnActionStop(IAction action)
        {
        }
        protected virtual void OnActionEnd(IAction action)
        {
        }
        protected virtual void OnActionCompleted(IAction action)
        {

        }
        protected virtual void OnGoalCompleted(IGoal goal)
        {

        }
        protected virtual void OnGoalStart(IGoal goal)
        {

        }
        protected virtual void OnNoActionFound(IGoalRequest request)
        {

        }
        public string GetAgentType()
        {
            return agentType;
        }
    }
}

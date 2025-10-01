using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using SIGGD.Goap.Config;
using UnityEngine;

namespace SIGGD.Goap.Interfaces
{
    public class GoapInjector : MonoBehaviour, IGoapInjector
    {
        public HungerConfigSO HungerConfigSO;
        public void Inject(IAction action)
        {
            if (action is IInjectable injectable)
                injectable.Inject(this);
        }

        public void Inject(IGoal goal)
        {
            if (goal is IInjectable injectable)
                injectable.Inject(this);
        }

        public void Inject(ISensor sensor)
        {
            if (sensor is IInjectable injectable)
                injectable.Inject(this);
        }
    }

}
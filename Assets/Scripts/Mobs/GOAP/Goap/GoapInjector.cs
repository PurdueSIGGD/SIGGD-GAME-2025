using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using SIGGD.Goap.Config;
using SIGGD.Mobs;
using UnityEngine;
using UnityEngine.AI;

namespace SIGGD.Goap.Interfaces
{
    public class GoapInjector : MonoBehaviour, IGoapInjector
    {
        public BaseStats BaseStats;
        
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
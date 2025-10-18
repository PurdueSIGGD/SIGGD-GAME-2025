using System;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using SIGGD.Goap.Behaviours;
using SIGGD.Mobs;
using UnityEngine;

namespace SIGGD.Mobs.PackScripts
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Mobs/PackBehaviorData")]
    public class PackBehaviorData : ScriptableObject
    {
        [SerializeField]
        public float AgentTypeVisionRange { get; }
        // maximum radius around which the implementing agent can see agents of its type for the sake of forming packs

        [SerializeField]
        public float JoinPackRange { get; }
        // maximum radius around which the implementing agent can join the pack of the target agent

        [SerializeField]
        public int NumSearchRingSplits { get; }
        // number of evenly spaced intervals of the JoinPackRange search radius

        [SerializeField]
        public float LeavePackRange { get; }
        // minimum radius from the alpha at which the implementing agent must leave its pack

        [SerializeField]
        public float CloseEnoughToAlphaDist { get; }
        // distance from alpha at which the implementing agent will complete its follow alpha goal
    }
}
using System;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using SIGGD.Mobs;
using UnityEngine;

namespace SIGGD.Mobs.PackScripts
{
    [Serializable]
    [CreateAssetMenu(menuName = "Scriptable Objects/Mobs/PackBehaviorData")]
    public class PackBehaviorData : ScriptableObject
    {
        [SerializeField]
        public float AgentTypeVisionRange;
        // maximum radius around which the implementing agent can see agents of its type for the sake of forming packs

        // [SerializeField]
        // public float JoinPackRange;
        // // maximum radius around which the implementing agent can join the pack of the target agent

        [SerializeField]
        public int NumSearchRingSplits;
        // number of evenly spaced intervals of the JoinPackRange search radius

        [SerializeField]
        public float LeavePackRange;
        // minimum distance from the closest member in the pack at which the implementing agent must leave its pack
        // LeavePackRange should be greater than JoinPackRange by a decent amount to avoid constantly joining and leaving a pack

        [SerializeField]
        public float CloseEnoughToAlphaDist;
        // distance from alpha at which the implementing agent will complete its follow alpha goal

        [SerializeField]
        public int MaxPackSize;
        // maximum pack size
    }
}
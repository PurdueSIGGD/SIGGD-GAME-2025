using System.Collections;
using UnityEngine;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Agent.Core;
using SIGGD.Mobs.Hyena;
using SIGGD.Mobs;


namespace SIGGD.Goap
{

    public class CommonData : IActionData
    {
        public ITarget Target { get; set; }
        public float Timer { get; set; }

        [GetComponent]
        public Animator animator { get; set; }

        [GetComponent]
        public HyenaAttackManager am { get; set; }

        [GetComponent]
        public Movement mv { get; set; }
        public PerceptionManager pm { get; set; }

        [GetComponent]
        public Movement mv { get; set; }
    }
}
using System.Collections;
using UnityEngine;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Agent.Core;

namespace SIGGD.Goap
{

    public class CommonData : IActionData
    {
        public ITarget Target { get; set; }
        public float Timer { get; set; }

        [GetComponent]
        public Animator animator { get; set; }
        
    }
}
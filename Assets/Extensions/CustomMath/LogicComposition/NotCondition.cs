using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extensions.CustomMath.LogicComposition
{
    [Serializable]
    public class NotCondition<TContext> : ICondition<TContext>
    {
        [SerializeReference] 
        public ICondition<TContext> child;

        public bool Evaluate(TContext context)
        {
            return !child.Evaluate(context);
        }

        public override string ToString()
        {
            return "* NOT Condition *";
        }
    }
}
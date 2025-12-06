using System.Collections.Generic;
using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extensions.CustomMath.LogicComposition
{
    [Serializable]
    public class OrCondition<TContext> : ICondition<TContext>
    {
        [SerializeReference] 
        public List<ICondition<TContext>> children = new();

        public bool Evaluate(TContext context)
        {
            foreach (var child in children)
            {
                if (child.Evaluate(context))
                    return true;
            }
            return false;
        }
        
        public override string ToString()
        {
            return "* OR Condition *";
        }
    }
}
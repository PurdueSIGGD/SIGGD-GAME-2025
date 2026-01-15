using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extensions.CustomMath.LogicComposition
{
    [Serializable]
    public class AndCondition<TContext> : ICondition<TContext>
    {
        [SerializeReference] 
        public List<ICondition<TContext>> children = new();

        public bool Evaluate(TContext context)
        {
            foreach (var child in children)
            {
                if (!child.Evaluate(context))
                    return false;
            }
            return true;
        }
        
        public override string ToString()
        {
            return "* AND Condition *";
        }
    }
}
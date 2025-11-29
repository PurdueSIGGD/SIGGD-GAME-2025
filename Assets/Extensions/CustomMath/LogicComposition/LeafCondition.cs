using System;

namespace Extensions.CustomMath.LogicComposition
{
    [Serializable]
    public class LeafCondition<TContext> : ICondition<TContext>
    {
        public Func<TContext, bool> check;

        public LeafCondition(Func<TContext, bool> check)
        {
            this.check = check;
        }

        public bool Evaluate(TContext context)
        {
            return check?.Invoke(context) ?? false;
        }
    }
}
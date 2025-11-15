using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace Extensions.CustomMath.LogicComposition
{
    /**
     * Interface for defining a condition that can be evaluated against a given context.
     *
     * @tparam TContext The type of context the condition evaluates against.
     */
    public interface ICondition<in TContext>
    {
        bool Evaluate(TContext context);
        
        #region Odin Dropdown Helpers
        
        public static IEnumerable<ValueDropdownItem> GetBroadcastStrategies()
        {
            // Top-priority composites first
            yield return new ValueDropdownItem("* AND Condition *", new AndCondition<QuestOutcome>());
            yield return new ValueDropdownItem("* OR Condition *", new OrCondition<QuestOutcome>());
            yield return new ValueDropdownItem("* NOT Condition *", new NotCondition<QuestOutcome>());

            // Dynamically add all other condition types
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(ICondition<QuestOutcome>).IsAssignableFrom(t)
                            && !t.IsAbstract
                            && t != typeof(AndCondition<QuestOutcome>)
                            && t != typeof(OrCondition<QuestOutcome>)
                            && t != typeof(NotCondition<QuestOutcome>));

            foreach (var t in allTypes)
            {
                yield return new ValueDropdownItem(t.Name, (ICondition<QuestOutcome>)Activator.CreateInstance(t));
            }
        }
        
        #endregion
    }
}
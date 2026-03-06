using System.Collections.Generic;
using System.Reflection;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * The CutsceneActionAdapter class serves as an adapter for invoking cutscene actions on a target object. It uses reflection to discover and invoke methods marked with the CutsceneActionAttribute on the target object.
     * This adapter allows for dynamic invocation of cutscene actions based on their display names, providing flexibility in how cutscene actions are defined and executed.
     * </summary>
     */
    public class CutsceneActionAdapter
    {
        private object target;
        private Dictionary<string, MethodInfo> actions;

        /**
         * <summary>
         * Initializes a new instance of the CutsceneActionAdapter class for the specified target object. The constructor uses reflection to discover all methods on the target object that are marked with the CutsceneActionAttribute and stores them in a dictionary for later invocation.
         * </summary>
         * <param name="target">The object on which cutscene actions will be invoked.</param>
         */
        public CutsceneActionAdapter(object target)
        {
            this.target = target;
            actions = CutsceneReflectionUtility.GetCutsceneActions(target);
        }

        /**
         * <summary>
         * Retrieves the names of all available cutscene actions that can be invoked on the target object. This allows for dynamic discovery of cutscene actions, enabling features such as UI generation or debugging tools to list available actions.
         * </summary>
         * <returns>An enumerable collection of cutscene action names.</returns>
         */
        public IEnumerable<string> GetActionNames()
        {
            return actions.Keys;
        }

        /**
         * <summary>
         * Retrieves the MethodInfo for a specific cutscene action based on its display name. This allows for direct access to the method information of a cutscene action, which can be useful for advanced scenarios such as custom invocation or analysis.
         * </summary>
         * <param name="actionName">The display name of the cutscene action to retrieve.</param>
         * <returns>The MethodInfo corresponding to the specified cutscene action name.</returns>
         */
        public MethodInfo GetMethod(string actionName)
        {
            return actions[actionName];
        }

        /**
         * <summary>
         * Invokes a cutscene action on the target object based on its display name and the provided arguments. This method uses reflection to invoke the corresponding method for the specified action name, allowing for dynamic execution of cutscene actions.
         * If the specified action name does not correspond to any available cutscene action, an error message is logged.
         * </summary>
         * <param name="actionName">The display name of the cutscene action to invoke.</param>
         * <param name="args">An array of arguments to pass to the cutscene action method.</param>
         */
        public void Invoke(string actionName, object[] args)
        {
            if (!actions.ContainsKey(actionName))
            {
                UnityEngine.Debug.LogError($"Cutscene action {actionName} not found");
                return;
            }

            actions[actionName].Invoke(target, args);
        }
    }

}
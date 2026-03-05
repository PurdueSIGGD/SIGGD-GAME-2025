using System;
using System.Collections.Generic;
using System.Reflection;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * Utility class for reflection-based operations related to cutscene actions. It provides methods to retrieve cutscene actions from a target object based on the presence of the CutsceneActionAttribute.
     * This utility allows for dynamic discovery of cutscene actions, enabling flexibility and extensibility in the cutscene system.
     * </summary>
     */
    public static class CutsceneReflectionUtility
    {
        
        /**
         * <summary>
         * Retrieves a dictionary of cutscene actions from the specified target object. The dictionary maps the display name of each cutscene action to its corresponding MethodInfo.
         * This method uses reflection to scan the target object's methods for those decorated with the CutsceneActionAttribute, allowing for dynamic discovery of cutscene actions.
         * </summary>
         * <param name="target">The object from which to retrieve cutscene actions.</param>
         * <returns>A dictionary mapping cutscene action display names to their corresponding MethodInfo.</returns>
         */
        public static Dictionary<string, MethodInfo> GetCutsceneActions(object target)
        {
            var dict = new Dictionary<string, MethodInfo>();
            var methods = target.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var m in methods)
            {
                var attr = m.GetCustomAttribute<CutsceneActionAttribute>();
                if (attr == null) continue;

                dict.Add(attr.DisplayName, m);
            }

            return dict;
        }
    }

}
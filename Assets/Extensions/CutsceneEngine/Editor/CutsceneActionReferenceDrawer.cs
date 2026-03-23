using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Extensions.CutsceneEngine.Editor
{
    /**
     * <summary>
     * Custom property drawer for CutsceneActionReference that provides:
     * - Type selection dropdown for ICutsceneAction implementations
     * - Inline property drawer for selected action type
     * - Clean, compact UI in inspectors
     * </summary>
     */
    [CustomPropertyDrawer(typeof(CutsceneActionReference))]
    public class CutsceneActionReferenceDrawer : PropertyDrawer
    {
        private static Type[] actionTypes;
        private static string[] actionTypeNames;
        private static bool typesInitialized = false;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!typesInitialized)
            {
                InitializeActionTypes();
            }
            
            EditorGUI.BeginProperty(position, label, property);
            
            var actionProp = property.FindPropertyRelative("Action");
            
            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            
            // Calculate rects
            var dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            
            // Determine current action type
            Type currentType = null;
            int currentIndex = 0;
            
            if (actionProp.managedReferenceValue != null)
            {
                currentType = actionProp.managedReferenceValue.GetType();
                currentIndex = Array.IndexOf(actionTypes, currentType);
                if (currentIndex < 0) currentIndex = 0;
            }
            
            // Type selection dropdown
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(dropdownRect, currentIndex, actionTypeNames);
            
            if (EditorGUI.EndChangeCheck())
            {
                Type newType = actionTypes[newIndex];
                actionProp.managedReferenceValue = Activator.CreateInstance(newType);
                property.serializedObject.ApplyModifiedProperties();
            }
            
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
        
        private static void InitializeActionTypes()
        {
            var actionTypeList = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch
                    {
                        return Array.Empty<Type>();
                    }
                })
                .Where(t => typeof(ICutsceneAction).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .OrderBy(t => t.Name)
                .ToArray();
            
            actionTypes = actionTypeList;
            actionTypeNames = actionTypes.Select(t => FormatTypeName(t.Name)).ToArray();
            typesInitialized = true;
        }
        
        private static string FormatTypeName(string typeName)
        {
            if (typeName.EndsWith("Action"))
                typeName = typeName.Substring(0, typeName.Length - 6);
            
            return System.Text.RegularExpressions.Regex.Replace(typeName, "([A-Z])", " $1").Trim();
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Extensions.CutsceneEngine.Editor
{
    /**
     * <summary>
     * Custom inspector for CutsceneActionClip that provides an intuitive interface for:
     * - Selecting action types from a dropdown
     * - Choosing actor methods from discovered [CutsceneAction] attributes
     * - Auto-generating parameter fields based on method signatures
     * - Validating actor compatibility and parameters
     * </summary>
     */
    [CustomEditor(typeof(CutsceneActionClip))]
    public class CutsceneActionClipInspector : UnityEditor.Editor
    {
        private CutsceneActionClip clip;
        private SerializedProperty actionProp;
        private SerializedProperty explicitTargetProp;
        
        private string[] actionTypeNames;
        private Type[] actionTypes;
        private int selectedActionTypeIndex = 0;
        
        // For ActorMethodAction
        private string[] availableMethodNames;
        private ICutsceneActor boundActor;
        
        private static readonly GUIStyle headerStyle = new GUIStyle();
        private static readonly GUIStyle helpBoxStyle = new GUIStyle();
        
        private void OnEnable()
        {
            clip = (CutsceneActionClip)target;
            actionProp = serializedObject.FindProperty("action");
            explicitTargetProp = serializedObject.FindProperty("explicitTarget");
            
            // Cache all action types
            CacheActionTypes();
            
            // Try to find bound actor
            TryFindBoundActor();
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // Initialize styles
            if (headerStyle.font == null)
            {
                headerStyle.fontSize = 14;
                headerStyle.fontStyle = FontStyle.Bold;
                headerStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                headerStyle.padding = new RectOffset(0, 0, 5, 5);
                
                helpBoxStyle.wordWrap = true;
                helpBoxStyle.padding = new RectOffset(10, 10, 5, 5);
                helpBoxStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Cutscene Action Configuration", headerStyle);
            EditorGUILayout.Space(3);
            
            // Show bound actor info
            DrawBoundActorInfo();
            
            EditorGUILayout.Space(5);
            
            // Explicit Target Override
            EditorGUILayout.PropertyField(explicitTargetProp, new GUIContent("Explicit Target", 
                "Override the bound actor. If set, this actor will be used instead of the track binding."));
            
            EditorGUILayout.Space(5);
            
            // Action Type Selection
            DrawActionTypeSelector();
            
            EditorGUILayout.Space(10);
            
            // Action-specific configuration
            if (clip.action?.Action != null)
            {
                DrawActionConfiguration();
            }
            else
            {
                EditorGUILayout.HelpBox("Select an action type above to configure the action.", MessageType.Info);
            }
            
            serializedObject.ApplyModifiedProperties();
            
            // Validate button
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Validate Action", GUILayout.Height(30)))
            {
                ValidateAction();
            }
        }
        
        private void DrawBoundActorInfo()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            if (boundActor != null)
            {
                EditorGUILayout.LabelField("Bound Actor", EditorStyles.boldLabel);
                var mono = boundActor as MonoBehaviour;
                EditorGUILayout.ObjectField("Actor", mono, typeof(MonoBehaviour), true);
                
                // Show available actions
                var adapter = boundActor.GetCutsceneAdapter();
                var actions = adapter.GetActionNames().ToArray();
                EditorGUILayout.LabelField($"Available Actions: {actions.Length}", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.HelpBox("No actor bound to track. Assign an actor in the Timeline track binding.", MessageType.Warning);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawActionTypeSelector()
        {
            EditorGUILayout.LabelField("Action Type", EditorStyles.boldLabel);
            
            // Determine current action type
            if (clip.action?.Action != null)
            {
                Type currentType = clip.action.Action.GetType();
                selectedActionTypeIndex = Array.IndexOf(actionTypes, currentType);
                if (selectedActionTypeIndex < 0) selectedActionTypeIndex = 0;
            }
            
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup("Type", selectedActionTypeIndex, actionTypeNames);
            
            if (EditorGUI.EndChangeCheck() && newIndex != selectedActionTypeIndex)
            {
                selectedActionTypeIndex = newIndex;
                CreateActionOfType(actionTypes[selectedActionTypeIndex]);
            }
        }
        
        private void DrawActionConfiguration()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Action Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            var action = clip.action.Action;
            Type actionType = action.GetType();
            
            // Special handling for different action types
            if (action is ActorMethodAction methodAction)
            {
                DrawActorMethodActionUI(methodAction);
            }
            else
            {
                // Generic serialized field drawer
                DrawGenericActionFields(action, actionType);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawActorMethodActionUI(ActorMethodAction methodAction)
        {
            if (boundActor == null)
            {
                EditorGUILayout.HelpBox("Cannot configure Actor Method Action: No actor bound to track.", MessageType.Error);
                Log("Cannot Configure Actor Method Action: No Actor Bound to Track");
                return;
            }
            
            var adapter = boundActor.GetCutsceneAdapter();
            if (adapter == null)
            {
                EditorGUILayout.HelpBox("Actor adapter is null. Ensure GetCutsceneAdapter() is properly implemented.", MessageType.Error);
                return;
            }
            
            availableMethodNames = adapter.GetActionNames().ToArray();
            
            if (availableMethodNames.Length == 0)
            {
                EditorGUILayout.HelpBox($"No [CutsceneAction] methods found on {boundActor.GetType().Name}. Add methods with the [CutsceneAction] attribute.", MessageType.Warning);
                return;
            }
            
            // Method selection dropdown
            int currentIndex = Array.IndexOf(availableMethodNames, methodAction.MethodName);
            if (currentIndex < 0 && availableMethodNames.Length > 0) currentIndex = 0;
            
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup("Method", currentIndex, availableMethodNames);
            
            bool methodChanged = EditorGUI.EndChangeCheck();
            
            // If no method selected yet, or method changed, update it
            if (string.IsNullOrEmpty(methodAction.MethodName) || methodChanged)
            {
                Undo.RecordObject(clip, "Change Action Method");
                methodAction.MethodName = availableMethodNames[newIndex];
                
                // ALWAYS regenerate parameters when method changes
                var method = adapter.GetMethod(methodAction.MethodName);
                GenerateParametersForMethod(methodAction, method);
                
                EditorUtility.SetDirty(clip);
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }
            
            // Draw parameters if method is selected
            if (!string.IsNullOrEmpty(methodAction.MethodName))
            {
                EditorGUILayout.Space(5);
                var method = adapter.GetMethod(methodAction.MethodName);
                
                // Ensure parameters are always correct
                if (method != null)
                {
                    var expectedParams = method.GetParameters();
                    if (methodAction.Parameters == null || methodAction.Parameters.Length != expectedParams.Length)
                    {
                        GenerateParametersForMethod(methodAction, method);
                    }
                }
                
                DrawMethodParameters(methodAction, method);
            }
        }
        
        private void DrawMethodParameters(ActorMethodAction methodAction, MethodInfo method)
        {
            if (method == null) return;
            
            var parameters = method.GetParameters();
            
            // Check if this is an OnUpdate method
            var attribute = method.GetCustomAttribute<CutsceneActionAttribute>();
            bool isOnUpdateMethod = attribute?.ExecutionMode == CutsceneActionExecutionMode.OnUpdate;
            
            // For OnUpdate methods, skip the first parameter (normalizedTime) in UI
            int startIndex = isOnUpdateMethod && parameters.Length > 0 && parameters[0].ParameterType == typeof(float) ? 1 : 0;
            int userParamCount = parameters.Length - startIndex;
            
            if (userParamCount == 0)
            {
                if (isOnUpdateMethod)
                {
                    EditorGUILayout.HelpBox("This is a continuous action (OnUpdate). It receives normalizedTime automatically.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("This method has no parameters.", MessageType.Info);
                }
                return;
            }
            
            EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);
            
            // Show execution mode info
            if (isOnUpdateMethod)
            {
                EditorGUILayout.HelpBox("⏱️ Continuous Action: Executes every frame. First parameter (normalizedTime) is provided automatically.", MessageType.Info);
            }
            
            // Ensure parameter array matches user parameters (excluding normalizedTime)
            if (methodAction.Parameters == null || methodAction.Parameters.Length != userParamCount)
            {
                GenerateParametersForMethod(methodAction, method);
            }
            
            for (int i = 0; i < userParamCount; i++)
            {
                var param = parameters[i + startIndex];
                var serializedParam = methodAction.Parameters[i];
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(param.Name, GUILayout.Width(120));
                
                EditorGUI.BeginChangeCheck();
                DrawParameterField(serializedParam, param.ParameterType);
                
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(clip);
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        private void DrawParameterField(SerializedCutsceneParameter param, Type paramType)
        {
            if (paramType == typeof(int))
            {
                param.type = SerializedCutsceneParameter.ParamType.Int;
                param.intValue = EditorGUILayout.IntField(param.intValue);
            }
            else if (paramType == typeof(float))
            {
                param.type = SerializedCutsceneParameter.ParamType.Float;
                param.floatValue = EditorGUILayout.FloatField(param.floatValue);
            }
            else if (paramType == typeof(bool))
            {
                param.type = SerializedCutsceneParameter.ParamType.Bool;
                param.boolValue = EditorGUILayout.Toggle(param.boolValue);
            }
            else if (paramType == typeof(string))
            {
                param.type = SerializedCutsceneParameter.ParamType.String;
                param.stringValue = EditorGUILayout.TextField(param.stringValue);
            }
            else if (paramType == typeof(Vector3))
            {
                param.type = SerializedCutsceneParameter.ParamType.Vector3;
                param.vector3Value = EditorGUILayout.Vector3Field("", param.vector3Value);
            }
            else if (paramType == typeof(GameObject) || paramType.IsSubclassOf(typeof(GameObject)))
            {
                param.type = SerializedCutsceneParameter.ParamType.GameObject;
                param.gameObjectValue = (GameObject)EditorGUILayout.ObjectField(param.gameObjectValue, typeof(GameObject), true);
            }
            else if (paramType.IsEnum)
            {
                // Handle enums (stored as int)
                param.type = SerializedCutsceneParameter.ParamType.Int;
                param.intValue = Convert.ToInt32(EditorGUILayout.EnumPopup((Enum)Enum.ToObject(paramType, param.intValue)));
            }
            else
            {
                EditorGUILayout.LabelField($"Unsupported type: {paramType.Name}");
            }
        }
        
        private void DrawGenericActionFields(ICutsceneAction action, Type actionType)
        {
            // Use reflection to draw all public fields
            var fields = actionType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                EditorGUI.BeginChangeCheck();
                object newValue = DrawFieldForType(field.Name, field.GetValue(action), field.FieldType);
                
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(clip, $"Change {field.Name}");
                    field.SetValue(action, newValue);
                    EditorUtility.SetDirty(clip);
                }
            }
        }
        
        private object DrawFieldForType(string label, object currentValue, Type fieldType)
        {
            if (fieldType == typeof(int))
                return EditorGUILayout.IntField(label, (int)currentValue);
            else if (fieldType == typeof(float))
                return EditorGUILayout.FloatField(label, (float)currentValue);
            else if (fieldType == typeof(bool))
                return EditorGUILayout.Toggle(label, (bool)currentValue);
            else if (fieldType == typeof(string))
                return EditorGUILayout.TextField(label, (string)currentValue);
            else if (fieldType == typeof(Vector3))
                return EditorGUILayout.Vector3Field(label, (Vector3)currentValue);
            else if (fieldType == typeof(GameObject))
                return EditorGUILayout.ObjectField(label, (GameObject)currentValue, typeof(GameObject), true);
            else if (fieldType.IsEnum)
                return EditorGUILayout.EnumPopup(label, (Enum)currentValue);
            else
            {
                EditorGUILayout.LabelField(label, $"Unsupported type: {fieldType.Name}");
                return currentValue;
            }
        }
        
        private void GenerateParametersForMethod(ActorMethodAction methodAction, MethodInfo method)
        {
            if (method == null)
            {
                methodAction.Parameters = new SerializedCutsceneParameter[0];
                return;
            }
            
            var parameters = method.GetParameters();
            
            // Check if this is an OnUpdate method (first param is float normalizedTime)
            var attribute = method.GetCustomAttribute<CutsceneActionAttribute>();
            bool isOnUpdateMethod = attribute?.ExecutionMode == CutsceneActionExecutionMode.OnUpdate;
            
            // For OnUpdate methods, skip the first parameter (normalizedTime)
            int startIndex = isOnUpdateMethod && parameters.Length > 0 && parameters[0].ParameterType == typeof(float) ? 1 : 0;
            int userParamCount = parameters.Length - startIndex;
            
            methodAction.Parameters = new SerializedCutsceneParameter[userParamCount];
            
            for (int i = 0; i < userParamCount; i++)
            {
                methodAction.Parameters[i] = new SerializedCutsceneParameter();
                
                // Set default type based on parameter type
                Type paramType = parameters[i + startIndex].ParameterType;
                if (paramType == typeof(int)) methodAction.Parameters[i].type = SerializedCutsceneParameter.ParamType.Int;
                else if (paramType == typeof(float)) methodAction.Parameters[i].type = SerializedCutsceneParameter.ParamType.Float;
                else if (paramType == typeof(bool)) methodAction.Parameters[i].type = SerializedCutsceneParameter.ParamType.Bool;
                else if (paramType == typeof(string)) methodAction.Parameters[i].type = SerializedCutsceneParameter.ParamType.String;
                else if (paramType == typeof(Vector3)) methodAction.Parameters[i].type = SerializedCutsceneParameter.ParamType.Vector3;
                else if (paramType == typeof(GameObject)) methodAction.Parameters[i].type = SerializedCutsceneParameter.ParamType.GameObject;
            }
        }
        
        private void CacheActionTypes()
        {
            // Find all types implementing ICutsceneAction
            var actionTypeList = new List<Type>();
            
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => typeof(ICutsceneAction).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                        .OrderBy(t => t.Name);
                    
                    actionTypeList.AddRange(types);
                }
                catch
                {
                    // Skip assemblies that can't be loaded
                }
            }
            
            actionTypes = actionTypeList.ToArray();
            actionTypeNames = actionTypes.Select(t => FormatActionTypeName(t.Name)).ToArray();
        }
        
        private string FormatActionTypeName(string typeName)
        {
            // Remove "Action" suffix if present
            if (typeName.EndsWith("Action"))
                typeName = typeName.Substring(0, typeName.Length - 6);
            
            // Add spaces before capital letters
            return System.Text.RegularExpressions.Regex.Replace(typeName, "([A-Z])", " $1").Trim();
        }
        
        private void CreateActionOfType(Type actionType)
        {
            Undo.RecordObject(clip, "Change Action Type");
            
            clip.action = new CutsceneActionReference
            {
                Action = (ICutsceneAction)Activator.CreateInstance(actionType)
            };
            
            EditorUtility.SetDirty(clip);
        }
        
        private void TryFindBoundActor()
        {
            boundActor = null;
            
            // First check explicit target
            if (clip.explicitTarget is ICutsceneActor actor)
            {
                boundActor = actor;
                return;
            }
            
            // Try to find the bound actor from the Timeline track
            // We need to find the PlayableDirector in the scene and check its bindings
            var directors = UnityEngine.Object.FindObjectsOfType<UnityEngine.Playables.PlayableDirector>();
            
            foreach (var director in directors)
            {
                var timeline = director.playableAsset as UnityEngine.Timeline.TimelineAsset;
                if (timeline == null) continue;
                
                foreach (var track in timeline.GetOutputTracks())
                {
                    if (track is CutsceneActionTrack)
                    {
                        // Check if this track contains our clip
                        foreach (var timelineClip in track.GetClips())
                        {
                            if (timelineClip.asset == clip)
                            {
                                // Found our clip! Get the binding for this track
                                var binding = director.GetGenericBinding(track);
                                if (binding is MonoBehaviour mono && mono is ICutsceneActor cutsceneActor)
                                {
                                    boundActor = cutsceneActor;
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private void ValidateAction()
        {
            if (clip.action?.Action == null)
            {
                EditorUtility.DisplayDialog("Validation Error", "No action configured.", "OK");
                return;
            }
            
            string message = "Action is valid!";
            MessageType messageType = MessageType.Info;
            
            // Validate based on action type
            if (clip.action.Action is ActorMethodAction methodAction)
            {
                if (string.IsNullOrEmpty(methodAction.MethodName))
                {
                    message = "No method selected.";
                    messageType = MessageType.Error;
                }
                else if (boundActor == null)
                {
                    message = "No actor bound to track.";
                    messageType = MessageType.Warning;
                }
            }
            
            if (messageType == MessageType.Error || messageType == MessageType.Warning)
            {
                EditorUtility.DisplayDialog("Validation", message, "OK");
            }
            else
            {
                Debug.Log($"CutsceneActionClip: {message}");
            }
        }
        
        private void Log(string message)
        {
            Debug.Log($"CutsceneActionClipInspector: {message}");
        }
    }
}


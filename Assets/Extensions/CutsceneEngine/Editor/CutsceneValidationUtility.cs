using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Extensions.CutsceneEngine.Editor
{
    /**
     * <summary>
     * Validation utility for cutscene actions and configurations.
     * Provides comprehensive checks for:
     * - Actor compatibility
     * - Parameter type matching
     * - Missing references
     * - Track bindings
     * - System availability
     * </summary>
     */
    public static class CutsceneValidationUtility
    {
        public class ValidationResult
        {
            public bool IsValid;
            public List<string> Errors = new List<string>();
            public List<string> Warnings = new List<string>();
            public List<string> Info = new List<string>();
            
            public bool HasIssues => Errors.Count > 0 || Warnings.Count > 0;
        }
        
        /// <summary>
        /// Validates a cutscene action clip configuration.
        /// </summary>
        public static ValidationResult ValidateClip(CutsceneActionClip clip, ICutsceneActor boundActor)
        {
            var result = new ValidationResult { IsValid = true };
            
            if (clip == null)
            {
                result.Errors.Add("Clip is null");
                result.IsValid = false;
                return result;
            }
            
            if (clip.action?.Action == null)
            {
                result.Errors.Add("No action configured");
                result.IsValid = false;
                return result;
            }
            
            // Validate based on action type
            var action = clip.action.Action;
            
            if (action is ActorMethodAction methodAction)
            {
                ValidateActorMethodAction(methodAction, boundActor, result);
            }
            else if (action is MoveActorAction moveAction)
            {
                ValidateMoveActorAction(moveAction, result);
            }
            else if (action is RotateActorAction rotateAction)
            {
                ValidateRotateActorAction(rotateAction, result);
            }
            else if (action is PlayAnimationAction animAction)
            {
                ValidatePlayAnimationAction(animAction, result);
            }
            
            // Check for explicit target override
            if (clip.explicitTarget != null)
            {
                if (!(clip.explicitTarget is ICutsceneActor))
                {
                    result.Errors.Add($"Explicit target {clip.explicitTarget.name} does not implement ICutsceneActor");
                    result.IsValid = false;
                }
                else
                {
                    result.Info.Add($"Using explicit target: {clip.explicitTarget.name}");
                }
            }
            
            result.IsValid = result.Errors.Count == 0;
            return result;
        }
        
        private static void ValidateActorMethodAction(ActorMethodAction action, ICutsceneActor actor, ValidationResult result)
        {
            if (string.IsNullOrEmpty(action.MethodName))
            {
                result.Errors.Add("No method selected");
                return;
            }
            
            if (actor == null)
            {
                result.Warnings.Add("No actor bound to track - cannot validate method");
                return;
            }
            
            var adapter = actor.GetCutsceneAdapter();
            var availableMethods = adapter.GetActionNames().ToArray();
            
            if (!availableMethods.Contains(action.MethodName))
            {
                result.Errors.Add($"Method '{action.MethodName}' not found on actor");
                return;
            }
            
            // Validate parameters
            var method = adapter.GetMethod(action.MethodName);
            var parameters = method.GetParameters();
            
            if (action.Parameters == null || action.Parameters.Length != parameters.Length)
            {
                result.Errors.Add($"Parameter count mismatch: expected {parameters.Length}, got {action.Parameters?.Length ?? 0}");
                return;
            }
            
            for (int i = 0; i < parameters.Length; i++)
            {
                var expectedType = parameters[i].ParameterType;
                var providedParam = action.Parameters[i];
                
                if (!IsParameterTypeCompatible(expectedType, providedParam))
                {
                    result.Warnings.Add($"Parameter '{parameters[i].Name}' type mismatch: expected {expectedType.Name}");
                }
            }
            
            result.Info.Add($"Calling method: {action.MethodName} with {parameters.Length} parameters");
        }
        
        private static void ValidateMoveActorAction(MoveActorAction action, ValidationResult result)
        {
            result.Info.Add($"Moving to {action.Target} (duration controlled by Timeline clip)");
        }
        
        private static void ValidateRotateActorAction(RotateActorAction action, ValidationResult result)
        {
            // Note: Duration is now controlled by Timeline clip length, not the action itself
            result.Info.Add($"Rotating to {action.EulerRotation} (duration controlled by Timeline clip)");
        }
        
        private static void ValidatePlayAnimationAction(PlayAnimationAction action, ValidationResult result)
        {
            if (string.IsNullOrEmpty(action.AnimationId))
            {
                result.Errors.Add("No animation ID specified");
                return;
            }
            
            result.Info.Add($"Playing animation: {action.AnimationId}");
        }
        
        private static bool IsParameterTypeCompatible(System.Type expectedType, SerializedCutsceneParameter param)
        {
            if (expectedType == typeof(int))
                return param.type == SerializedCutsceneParameter.ParamType.Int;
            else if (expectedType == typeof(float))
                return param.type == SerializedCutsceneParameter.ParamType.Float;
            else if (expectedType == typeof(bool))
                return param.type == SerializedCutsceneParameter.ParamType.Bool;
            else if (expectedType == typeof(string))
                return param.type == SerializedCutsceneParameter.ParamType.String;
            else if (expectedType == typeof(Vector3))
                return param.type == SerializedCutsceneParameter.ParamType.Vector3;
            else if (expectedType == typeof(GameObject) || expectedType.IsSubclassOf(typeof(GameObject)))
                return param.type == SerializedCutsceneParameter.ParamType.GameObject;
            else if (expectedType.IsEnum)
                return param.type == SerializedCutsceneParameter.ParamType.Int;
            
            return false;
        }
        
        /// <summary>
        /// Validates that an actor has all required components for cutscene participation.
        /// </summary>
        public static ValidationResult ValidateActor(ICutsceneActor actor)
        {
            var result = new ValidationResult { IsValid = true };
            
            if (actor == null)
            {
                result.Errors.Add("Actor is null");
                result.IsValid = false;
                return result;
            }
            
            var mono = actor as MonoBehaviour;
            if (mono == null)
            {
                result.Errors.Add("Actor is not a MonoBehaviour");
                result.IsValid = false;
                return result;
            }
            
            // Check transform
            var transform = actor.GetTransform();
            if (transform == null)
            {
                result.Errors.Add("Actor GetTransform() returns null");
                result.IsValid = false;
            }
            
            // Check adapter
            try
            {
                var adapter = actor.GetCutsceneAdapter();
                if (adapter == null)
                {
                    result.Errors.Add("Actor GetCutsceneAdapter() returns null");
                    result.IsValid = false;
                }
                else
                {
                    var methods = adapter.GetActionNames().ToArray();
                    result.Info.Add($"Found {methods.Length} cutscene actions");
                }
            }
            catch (System.Exception ex)
            {
                result.Errors.Add($"Error getting adapter: {ex.Message}");
                result.IsValid = false;
            }
            
            result.IsValid = result.Errors.Count == 0;
            return result;
        }
        
        /// <summary>
        /// Generates a formatted report string from validation results.
        /// </summary>
        public static string GenerateReport(ValidationResult result)
        {
            var report = new System.Text.StringBuilder();
            
            if (result.IsValid && !result.HasIssues)
            {
                report.AppendLine("✓ Validation passed with no issues");
            }
            else if (!result.IsValid)
            {
                report.AppendLine("✗ Validation failed");
            }
            else
            {
                report.AppendLine("⚠ Validation passed with warnings");
            }
            
            report.AppendLine();
            
            if (result.Errors.Count > 0)
            {
                report.AppendLine("ERRORS:");
                foreach (var error in result.Errors)
                {
                    report.AppendLine($"  • {error}");
                }
                report.AppendLine();
            }
            
            if (result.Warnings.Count > 0)
            {
                report.AppendLine("WARNINGS:");
                foreach (var warning in result.Warnings)
                {
                    report.AppendLine($"  • {warning}");
                }
                report.AppendLine();
            }
            
            if (result.Info.Count > 0)
            {
                report.AppendLine("INFO:");
                foreach (var info in result.Info)
                {
                    report.AppendLine($"  • {info}");
                }
            }
            
            return report.ToString();
        }
    }
}


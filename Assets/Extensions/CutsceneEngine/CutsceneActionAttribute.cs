using System;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * Marks a method as a cutscene action that can be invoked from Timeline.
     /// 
     /// Usage:
     /// - [CutsceneAction("Display Name")] - One-shot action (executes once on clip start)
     /// - [CutsceneAction("Display Name", CutsceneActionExecutionMode.OnUpdate)] - Continuous action
     /// 
     /// For OnUpdate methods, first parameter must be float normalizedTime.
     /// </summary>
     */
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CutsceneActionAttribute : Attribute
    {
        /**
         * <summary>
         * The display name of the cutscene action, shown in the cutscene editor UI.
         /// </summary>
         */
        public string DisplayName;
        
        /**
         * <summary>
         /// When this action should be executed (OnEnter, OnUpdate, or OnExit).
         /// Default is OnEnter (one-shot execution).
         /// </summary>
         */
        public CutsceneActionExecutionMode ExecutionMode;
        
        /**
         * <summary>
         /// Indicates whether this action can be called during regular gameplay.
         /// Deprecated - kept for backwards compatibility.
         /// </summary>
         */
        [Obsolete("AllowDuringGameplay is deprecated. All cutscene actions are now timeline-controlled.")]
        public bool AllowDuringGameplay;
        
        /**
         * <summary>
         /// Indicates whether this action is intended only for cinematics.
         /// Deprecated - kept for backwards compatibility.
         /// </summary>
         */
        [Obsolete("CinematicOnly is deprecated. All cutscene actions are now timeline-controlled.")]
        public bool CinematicOnly;

        /**
         * <summary>
         /// Marks a method as a one-shot cutscene action (executes once when clip starts).
         /// </summary>
         /// <param name="displayName">The name shown in the cutscene editor</param>
         */
        public CutsceneActionAttribute(string displayName)
        {
            DisplayName = displayName;
            ExecutionMode = CutsceneActionExecutionMode.OnEnter;
            AllowDuringGameplay = true;
            CinematicOnly = false;
        }
        
        /**
         * <summary>
         /// Marks a method as a cutscene action with specified execution mode.
         /// </summary>
         /// <param name="displayName">The name shown in the cutscene editor</param>
         /// <param name="executionMode">When the method should be called (OnEnter, OnUpdate, or OnExit)</param>
         */
        public CutsceneActionAttribute(string displayName, CutsceneActionExecutionMode executionMode)
        {
            DisplayName = displayName;
            ExecutionMode = executionMode;
            AllowDuringGameplay = true;
            CinematicOnly = false;
        }
        
        /**
         * <summary>
         /// Legacy constructor for backwards compatibility.
         /// </summary>
         */
        [Obsolete("Use CutsceneActionAttribute(string, CutsceneActionExecutionMode) instead.")]
        public CutsceneActionAttribute(string displayName, bool allowDuringGameplay = true, bool cinematicOnly = false)
        {
            DisplayName = displayName;
            AllowDuringGameplay = allowDuringGameplay;
            CinematicOnly = cinematicOnly;
            ExecutionMode = CutsceneActionExecutionMode.OnEnter;
        }
    }
    
    /**
     * <summary>
     /// Defines when a cutscene action method should be executed.
     /// </summary>
     */
    public enum CutsceneActionExecutionMode
    {
        /// <summary>
        /// Execute once when clip starts (default for simple actions).
        /// Method signature: void MethodName(params...)
        /// </summary>
        OnEnter,
        
        /// <summary>
        /// Execute every frame while clip is active (for continuous actions).
        /// Method signature: void MethodName(float normalizedTime, params...)
        /// First parameter MUST be float normalizedTime (0.0 to 1.0).
        /// </summary>
        OnUpdate,
        
        /// <summary>
        /// Execute once when clip ends (for cleanup).
        /// Method signature: void MethodName(params...)
        /// </summary>
        OnExit
    }
}


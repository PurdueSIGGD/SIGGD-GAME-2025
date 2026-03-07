using System;
using System.Reflection;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * Invokes methods marked with [CutsceneAction] on an actor.
     /// Supports all three execution modes: OnEnter, OnUpdate, and OnExit.
     /// 
     /// The execution mode is determined by the CutsceneActionAttribute on the method.
     /// - OnEnter: Method called once when clip starts
     /// - OnUpdate: Method called every frame (first param must be float normalizedTime)
     /// - OnExit: Method called once when clip ends
     /// </summary>
     */
    [Serializable]
    public class ActorMethodAction : CutsceneActionBase
    {
        public string MethodName;
        public SerializedCutsceneParameter[] Parameters;

        [NonSerialized]
        private MethodInfo cachedMethod;
        
        [NonSerialized]
        private CutsceneActionExecutionMode cachedExecutionMode;
        
        [NonSerialized]
        private bool methodInfoResolved;

        public override void OnEnter(ICutsceneActor actor, CutsceneContext context)
        {
            ResolveMethodInfo(actor);
            
            if (cachedExecutionMode == CutsceneActionExecutionMode.OnEnter)
            {
                InvokeMethod(actor, 0f);
            }
        }

        public override void OnUpdate(ICutsceneActor actor, CutsceneContext context, float normalizedTime, float deltaTime)
        {
            if (!methodInfoResolved)
                ResolveMethodInfo(actor);
            
            if (cachedExecutionMode == CutsceneActionExecutionMode.OnUpdate)
            {
                InvokeMethod(actor, normalizedTime);
            }
        }

        public override void OnExit(ICutsceneActor actor, CutsceneContext context)
        {
            if (!methodInfoResolved)
                ResolveMethodInfo(actor);
            
            if (cachedExecutionMode == CutsceneActionExecutionMode.OnExit)
            {
                InvokeMethod(actor, 0f);
            }
        }

        private void ResolveMethodInfo(ICutsceneActor actor)
        {
            if (methodInfoResolved) return;
            
            if (string.IsNullOrEmpty(MethodName))
            {
                methodInfoResolved = true;
                return;
            }

            var adapter = actor?.GetCutsceneAdapter();
            if (adapter == null)
            {
                methodInfoResolved = true;
                return;
            }

            cachedMethod = adapter.GetMethod(MethodName);
            
            // Get execution mode from attribute
            if (cachedMethod != null)
            {
                var attribute = cachedMethod.GetCustomAttribute<CutsceneActionAttribute>();
                cachedExecutionMode = attribute?.ExecutionMode ?? CutsceneActionExecutionMode.OnEnter;
            }
            
            methodInfoResolved = true;
        }

        private void InvokeMethod(ICutsceneActor actor, float normalizedTime)
        {
            if (cachedMethod == null) return;

            var adapter = actor.GetCutsceneAdapter();
            if (adapter == null) return;

            // Build arguments based on execution mode
            object[] args;
            
            if (cachedExecutionMode == CutsceneActionExecutionMode.OnUpdate)
            {
                // OnUpdate methods have normalizedTime as first parameter
                var paramValues = Parameters != null 
                    ? Array.ConvertAll(Parameters, p => p.GetValue())
                    : Array.Empty<object>();
                
                args = new object[paramValues.Length + 1];
                args[0] = normalizedTime;
                Array.Copy(paramValues, 0, args, 1, paramValues.Length);
            }
            else
            {
                // OnEnter and OnExit don't have normalizedTime parameter
                args = Parameters != null 
                    ? Array.ConvertAll(Parameters, p => p.GetValue())
                    : Array.Empty<object>();
            }

            adapter.Invoke(MethodName, args);
        }
    }
}




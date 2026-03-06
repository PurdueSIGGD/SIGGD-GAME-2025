using UnityEngine;

namespace Extensions.CutsceneEngine
{
    /**
     * A simple text actor used as a default actor.
     */
    public class CutsceneActorProxy : MonoBehaviour, ICutsceneActor
    {
        private CutsceneActionAdapter adapter;
        
        private void Awake()
        {
            InitializeAdapter();
        }
        
        private void InitializeAdapter()
        {
            if (adapter == null)
            {
                adapter = new CutsceneActionAdapter(this);
            }
        }
        
        [CutsceneAction("WaveHello", CutsceneActionExecutionMode.OnEnter)]
        public void WaveHello()
        {
            Debug.Log($"Hello from CutsceneActorProxy: {name}!");
        }
        
        [CutsceneAction("SaySomething")]
        public void SaySomething(string message)
        {
            Debug.Log($"CutsceneActorProxy {name} says: {message}");
        }

        [CutsceneAction("WaveHelloContinuous", CutsceneActionExecutionMode.OnUpdate)]
        public void WaveHelloContinuous(float normalizedTime)
        {
            
        }
        
        #region ICutsceneActor implementation

        public Transform GetTransform() => transform;

        public CutsceneActionAdapter GetCutsceneAdapter()
        {
            InitializeAdapter();
            return adapter;
        }

        public void OnCutsceneEnter()
        {
            Debug.Log($"Entered CutsceneActorProxy: {name}!");
        }

        public void OnCutsceneExit()
        {
            Debug.Log($"Exited CutsceneActorProxy: {name}!");
        }
        
        #endregion
    }

}
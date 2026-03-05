using UnityEngine;
using System.Collections.Generic;


namespace Extensions.CutsceneEngine
{

    /**
     * <summary>
     * The CutsceneActionDefinition class represents a definition for a cutscene action, which can be created as a ScriptableObject in Unity.
     * It contains the name of the action and a list of parameters that define the inputs required for the action.
     * This class allows for the creation of reusable cutscene actions that can be easily configured and used within the cutscene system.
     * </summary>
     */
    [CreateAssetMenu(menuName = "Cutscene/Action Definition")]
    public class CutsceneActionDefinition : ScriptableObject
    {
        [Tooltip("The name of the cutscene action, which should correspond to a method decorated with the CutsceneActionAttribute.")]
        public string actionName;
        
        [Tooltip("A list of parameters that define the inputs required for the cutscene action. These parameters can be used to configure the action when it is executed.")]
        public List<SerializedCutsceneParameter> parameters;
    }

}
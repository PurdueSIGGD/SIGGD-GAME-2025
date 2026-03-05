using System;
using UnityEngine;

namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * The CutsceneActionReference class is a serializable wrapper for referencing cutscene actions in the Unity Editor.
     * It allows for the serialization of cutscene actions, which can be used to create references to specific actions that can be executed during cutscenes.
     * This class is particularly useful for creating lists of actions or referencing actions in other ScriptableObjects or MonoBehaviours.
     * </summary>
     */
    [Serializable]
    public class CutsceneActionReference
    {
        [SerializeReference]
        public ICutsceneAction Action;
    }
}
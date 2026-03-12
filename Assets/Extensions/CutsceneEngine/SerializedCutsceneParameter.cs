using System;
using UnityEngine;


namespace Extensions.CutsceneEngine
{
    /**
     * <summary>
     * The SerializedCutsceneParameter class represents a parameter that can be serialized for cutscenes,
     * primarily used in the Unity Editor because of serialization limitations.
     * </summary>
     */
    [Serializable]
    public class SerializedCutsceneParameter
    {
        public enum ParamType
        {
            Int,
            Float,
            Bool,
            String,
            Vector3,
            GameObject
        }

        public ParamType type;

        public int intValue;
        public float floatValue;
        public bool boolValue;
        public string stringValue;
        public Vector3 vector3Value;
        public GameObject gameObjectValue;

        /**
         * <summary>
         * Retrieves the value of the parameter based on its type. This method uses a switch expression to return the appropriate value based on the ParamType of the parameter.
         * It allows for dynamic retrieval of the parameter value, enabling cutscene actions to access the correct data type when executing.
         * </summary>
         * <returns>The value of the parameter as an object, which can be cast to the appropriate type based on the ParamType.</returns>
         */
        public object GetValue()
        {
            return type switch
            {
                ParamType.Int => intValue,
                ParamType.Float => floatValue,
                ParamType.Bool => boolValue,
                ParamType.String => stringValue,
                ParamType.Vector3 => vector3Value,
                ParamType.GameObject => gameObjectValue,
                _ => null
            };
        }
    }
}
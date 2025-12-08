using System;
using UnityEngine;

namespace MobCensus
{
    /// <summary>
    /// Raw data structure for storing mob citizen information.
    /// Is the actual data for mobs that gets serialized to the save file.
    /// </summary>
    [Serializable]
    public class MobCitizenDataRaw
    {
        [SerializeField] string mobId;
        [SerializeField] Vector3 position;
        [SerializeField] Vector3 rotation;
        [SerializeField] float health;

        public MobCitizenDataRaw()
        {
            this.mobId = "";
            this.position = Vector3.zero;
            this.rotation = Vector3.zero;
            this.health = 0f;
        }

        public string GetMobId() { return mobId; }
        public Vector3 GetPosition() { return position; }
        public Vector3 GetRotation() { return rotation; }
        public float GetHealth() { return health; }
        public string SetMobId(string mobId) { return this.mobId = mobId; }
        public Vector3 SetPosition(Vector3 position) { return this.position = position; }
        public Vector3 SetRotation(Vector3 rotation) { return this.rotation = rotation; }
        public float SetHealth(float health) { return this.health = health; }
    }
}
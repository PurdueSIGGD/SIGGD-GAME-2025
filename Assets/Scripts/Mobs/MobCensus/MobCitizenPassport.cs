using System;
using UnityEngine;

namespace MobCensus
{
    /// <summary>
    /// Attached to mob instances to interface with the MobCensus system
    /// </summary>
    public class MobCitizenPassport : MonoBehaviour
    {
        MobCensusManager census;
        MobCitizenData citizenDataReference = null;
        EntityHealthManager healthManager;

        void Start()
        {
            census = FindFirstObjectByType<MobCensusManager>();
            healthManager = GetComponent<EntityHealthManager>();
        }

        public void SetCitizenDataReference(MobCitizenData reference)
        {
            citizenDataReference = reference;
        }

        /// <summary>
        /// Write the data currently on the mob to the census
        /// Only works if the mob already has a citizen data reference
        /// </summary>
        public void WriteMobCitizenData()
        {
            if (citizenDataReference == null)
            {
                Debug.LogError("MobCitizenPassport: No citizen data reference set.");
                return;
            }
            MobCitizenDataRaw rawData = citizenDataReference.GetRawDataReference();
            rawData.SetPosition(transform.position);
            rawData.SetRotation(transform.eulerAngles);
            if (healthManager != null)
            {
                rawData.SetHealth(healthManager.CurrentHealth);
            }
        }
        /// <summary>
        /// Read the data from the census to the current mob instance
        /// Only works if the mob already has a citizen data reference
        /// </summary>
        public void ReadMobCitizenData(MobCitizenDataRaw rawData = null)
        {
            if (citizenDataReference == null && rawData == null)
            {
                Debug.LogError("MobCitizenPassport: No citizen data reference set.");
                return;
            }
            transform.position = rawData.GetPosition();
            transform.eulerAngles = rawData.GetRotation();
            if (healthManager != null)
            {
                healthManager.CurrentHealth = rawData.GetHealth();
            }
        }

        /// <summary>
        /// Remotely removes this mob's citizen data from the census
        /// Should only be called when the mob is dead/destroyed/permanently removed from the world
        /// </summary>
        public void EmmigrateFromPandora()
        {
            census.RemoveCitizen(citizenDataReference);
            citizenDataReference = null;
        }

    }
}
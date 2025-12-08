using System;
using UnityEngine;

namespace Census
{
    /// <summary>
    /// Attached to mob instances to interface with the MobCensus system
    /// </summary>
    public class MobCitizenPassport : MonoBehaviour
    {
        MobCensus census;
        MobCitizenData citizenDataReference = null;
        EntityHealthManager healthManager;

        void Start()
        {
            census = FindFirstObjectByType<MobCensus>();
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
            citizenDataReference.GetRawDataReference().SetPosition(transform.position);
            citizenDataReference.GetRawDataReference().SetRotation(transform.eulerAngles);
            if (healthManager != null)
            {
                citizenDataReference.GetRawDataReference().SetHealth(healthManager.CurrentHealth);
            }
        }
        /// <summary>
        /// Read the data from the census to the current mob instance
        /// Only works if the mob already has a citizen data reference
        /// </summary>
        public void ReadMobCitizenData()
        {
            if (citizenDataReference == null)
            {
                Debug.LogError("MobCitizenPassport: No citizen data reference set.");
                return;
            }
            transform.position = citizenDataReference.GetRawDataReference().GetPosition();
            transform.eulerAngles = citizenDataReference.GetRawDataReference().GetRotation();
            if (healthManager != null)
            {
                healthManager.CurrentHealth = citizenDataReference.GetRawDataReference().GetHealth();
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
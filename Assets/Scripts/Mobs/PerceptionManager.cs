using System;
using UnityEngine;
using SIGGD.Mobs;
using SIGGD.Mobs.StateMachine;
using System.Collections.Generic;

[RequireComponent(typeof(Smell))]
[RequireComponent(typeof(FieldOfView))]
public class PerceptionManager : MonoBehaviour
{
    private Smell smell;
    private FieldOfView fov;
    public List<GameObject> preyTargets = new List<GameObject>();
    public List<GameObject> predatorTargets = new List<GameObject>();
    public event Action<Transform> OnPlayerDetected;
    public Transform PlayerTarget { get; private set; }
    public bool CanSeePlayer { get; private set; }

    void Start()
    {
        smell = GetComponent<Smell>();
        fov = GetComponent<FieldOfView>();
    }
    void LateUpdate()
    {
        UpdateVision();
        UpdateSmell();
        //UpdatePerception();
    }
    private void UpdateVision()
    {
        if (fov == null) return;
        var seen = fov.GetSeenTargets();

        bool tempSeePlayer = false;

        preyTargets.Clear();
        predatorTargets.Clear();

        foreach (var target in seen)
        {
            if (target != null) {
                if (target.CompareTag("Player") && PlayerID.Instance.IsAlive) {
                    tempSeePlayer = true;
                    PlayerTarget = fov.PlayerTarget?.transform;
                    if (!CanSeePlayer)
                    {
                        OnPlayerDetected?.Invoke(PlayerTarget.transform);
                    }
                } 
                else if (target.TryGetComponent<SMPreyBrain>(out _)) {
                    preyTargets.Add(target);
                }
                else if (target.TryGetComponent<SMHyenaBrain>(out _))
                {
                    predatorTargets.Add(target);
                }
            }
        }
        CanSeePlayer = tempSeePlayer;
        if (!tempSeePlayer) PlayerTarget = null;
    }
    private void UpdatePerception()
    {
        /*
        if (PlayerTarget != null)
        {
            Debug.Log("player target not null");
            if (!CanSeePlayer)
            {
                Debug.Log("can see player now");
                OnPlayerDetected?.Invoke(PlayerTarget.transform);
                CanSeePlayer = true;
            }
        } else
        {
            Debug.Log("cannot see player now");
            CanSeePlayer = false;
        }
        /*
        inTerritory.checkIsInTerritory();
        if (inTerritory)
        {
            territory.getDistanceToCenter(transform.position);
            territory should maybe be static
        }
        */
    }
    private void UpdateSmell()
    {
    }
    public Vector3 GetSmellPosition()
    {
        return smell != null ? smell.GetToSmellPos() : Vector3.zero;
    }
}

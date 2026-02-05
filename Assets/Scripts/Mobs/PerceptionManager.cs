using System;
using UnityEngine;
using SIGGD.Mobs;
using System.Collections.Generic;

public class PerceptionManager : MonoBehaviour
{
    private Smell smell;
    private FieldOfView fov;
    public List<GameObject> preyTargets = new List<GameObject>();
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

        foreach (var target in seen)
        {
            if (target != null) {
                if (target.CompareTag("Player")) {
                    tempSeePlayer = true;
                    PlayerTarget = fov.PlayerTarget?.transform;
                    if (!CanSeePlayer)
                    {
                        OnPlayerDetected?.Invoke(PlayerTarget.transform);
                    }
                } else if (target.TryGetComponent<PreyBehaviour>(out _)) {
                    preyTargets.Add(target);
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
        return smell != null ? smell.GetSmellPos() : Vector3.zero;
    }
}

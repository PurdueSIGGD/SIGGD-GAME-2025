using NUnit.Framework;
using System;
using UnityEngine;
using SIGGD.Mobs;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Validation;

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
        UpdatePerception();
    }
    private void UpdateVision()
    {
        PlayerTarget = fov.PlayerTarget != null ? fov.PlayerTarget.transform : null;
        CanSeePlayer = PlayerTarget != null;
        preyTargets.Clear();
        var seen = fov.GetSeenTargets();
        foreach (var target in seen)
        {
            if (target.TryGetComponent<PreyBehaviour>(out _))
            {
                preyTargets.Add(target);
            }
        }
    }
    private void UpdatePerception()
    {
        if (PlayerTarget != null)
        {
            OnPlayerDetected?.Invoke(PlayerTarget.transform);
        }
    }
}

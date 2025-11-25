using System;
using UnityEngine;
using UnityEngine.UI;

public class InteractableUI : MonoBehaviour
{
    private Action onCompleteCallback;
    private Func<bool> exitCondition;
    private float interactionTimer;
    private float interactionDuration;
    private bool activeInteraction;
    
    [SerializeField] private Slider interactionSlider;

    private void Start()
    {
        DeactivateUI();
    }

    private void Update()
    {
        if (activeInteraction)
        {
            interactionSlider.value = 1 - interactionTimer / interactionDuration;
            
            interactionTimer -= Time.deltaTime;
            if (interactionTimer <= 0f)
            {
                activeInteraction = false;
                onCompleteCallback?.Invoke();
            }
            
            if (exitCondition != null && exitCondition.Invoke())
            {
                Debug.Log("UI Interaction Exited Early");
                ResetInteractUI();
            }
        }
        
    }

    public void ActivateUI(IInteractable<PlayerInteractor> item)
    {
        Debug.Log("UI Activated");
        gameObject.SetActive(true);
    }
    
    public void BeginInteractUI(IInteractable<PlayerInteractor> item, Action onComplete, Func<bool> exit, float duration = 3f)
    {
        Debug.Log("UI Interaction Started");
        interactionTimer = duration;
        interactionDuration = duration;
        activeInteraction = true;
        onCompleteCallback = onComplete;
        exitCondition = exit;
    }

    public void ResetInteractUI()
    {
        Debug.Log("UI Interaction Reset");
        activeInteraction = false;
        
        onCompleteCallback = null;
        exitCondition = null;
        interactionTimer = 0f;
        interactionDuration = 0f;
        interactionSlider.value = 0f;
    }
    
    public void DeactivateUI()
    {
        Debug.Log("UI Deactivated");
        gameObject.SetActive(false);
        
        ResetInteractUI();
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemUIInfo
{
    public Sprite icon;
    public string itemName;
    // add more fields as needed
}

public interface IScrollItem
{
    ItemUIInfo GetItemUIInfo();
}


public class ScrollMenu : MonoBehaviour
{
    [SerializeField] private List<RectTransform> scrollItems;
    [SerializeField] private int numVisibleAtOnce = 5;
    
    private Vector3[] originalPositions; // positions to 
    private Vector3[] originalScales;
    
    private void Awake()
    {
        // Store original positions and scales
        originalPositions = new Vector3[scrollItems.Count];
        originalScales = new Vector3[scrollItems.Count];
        for (int i = 0; i < scrollItems.Count; i++)
        {
            originalPositions[i] = scrollItems[i].localPosition;
            originalScales[i] = scrollItems[i].localScale;
        }
        
        UpdateItemPositions();
    }
    
    public void RefreshIcons<T>(T[] items) where T : IScrollItem
    {
        int count = Math.Min(items.Length, scrollItems.Count);
        for (int i = 0; i < count; i++)
        {
            ItemUIInfo info = items[i].GetItemUIInfo();
            var iconImage = scrollItems[i].GetComponentInChildren<UnityEngine.UI.Image>();
            if (iconImage != null)
            {
                iconImage.sprite = info.icon;
            }
        }
    }

    // function to bind to scroll up button
    public void BindToInput(Action<InputAction.CallbackContext> scrollUpAction, Action<InputAction.CallbackContext> scrollDownAction)
    {
        scrollUpAction += ctx => ScrollUp();
        scrollDownAction += ctx => ScrollDown();
    }
    
    public void ScrollUp()
    {
        // move all items up, wrap the top one to the bottom, and update positions/scales/items
    }
    
    public void ScrollDown()
    {
        
    }
    
    
    private void UpdateItemPositions()
    {
        
    }
}

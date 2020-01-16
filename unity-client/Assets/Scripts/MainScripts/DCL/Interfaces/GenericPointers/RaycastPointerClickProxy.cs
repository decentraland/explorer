using System;
using UnityEngine;

public class RaycastPointerClickProxy : MonoBehaviour, IRaycastPointerClickHandler
{
    public event Action OnClick;
    
    public void OnPointerClick()
    {
        OnClick?.Invoke();
    }
}
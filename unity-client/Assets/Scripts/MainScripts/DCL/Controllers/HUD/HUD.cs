using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "HUD", menuName = "UI/HUD")]
public class HUD : ScriptableObject
{
    public enum HideBehaviour
    {
        HideUI = 0,
        BuildModeActive = 1
    }
    public ViewController controller;
    public GameObject hudView;
    public HideBehaviour[] hideBehaviours;


    public void Hide(HideBehaviour hideBehaviour)
    {
        if (!hideBehaviours.Contains(hideBehaviour)) return;

    }
}

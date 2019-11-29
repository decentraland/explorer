using System;
using Microsoft.SqlServer.Server;
using UnityEngine;

[CreateAssetMenu(fileName = "InputAction_OneTime", menuName = "InputActions/OneTime")]
public class InputAction_OneTime : ScriptableObject
{
    public delegate void Triggered(DCLAction_OneTime action);
    public event Triggered OnTriggered;

    [SerializeField] private DCLAction_OneTime dclAction;
    public DCLAction_OneTime GetDCLAction() => dclAction;

    private int triggeredInFrame = -1;
    private InputActionResolver_OneTime[] resolvers;

    public bool WasTriggeredThisFrame()
    {
        return triggeredInFrame == Time.frameCount;
    }

    internal void RaiseOnTriggered()
    {
        triggeredInFrame = Time.frameCount;
        OnTriggered?.Invoke(dclAction);
    }

    #region Editor

#if UNITY_EDITOR

    private void OnEnable()
    {
        Application.quitting -= CleanUp;
        Application.quitting += CleanUp;
    }

    private void CleanUp()
    {
        Application.quitting -= CleanUp;
        if (UnityEditor.AssetDatabase.Contains(this)) //It could happen that the SO has been created in runtime
            Resources.UnloadAsset(this);
    }

    [UnityEditor.CustomEditor(typeof(InputAction_OneTime), true)]
    internal class InputAction_OneTimeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (Application.isPlaying && GUILayout.Button("Raise OnChange"))
            {
                ((InputAction_OneTime)target).RaiseOnTriggered();
            }
        }
    }
#endif

    #endregion

}

internal abstract class InputActionResolver_OneTime
{
    protected readonly InputAction_OneTime oneTimeAction;

    public InputActionResolver_OneTime(InputAction_OneTime oneTimeAction)
    {
        this.oneTimeAction = oneTimeAction;
    }

    public abstract void UpdateTime(float deltaTime);
}

internal class KeyboardInputActionResolver_OneTime : InputActionResolver_OneTime
{
    private readonly KeyCode keyCode;

    public KeyboardInputActionResolver_OneTime(InputAction_OneTime oneTimeAction, KeyCode keyCode) : base(oneTimeAction)
    {
        this.keyCode = keyCode;
    }

    public override void UpdateTime(float deltaTime)
    {
        if (Input.GetKeyDown(keyCode))
        {
            oneTimeAction.RaiseOnTriggered();
        }
    }
}

internal class MouseButtonInputActionResolver_OneTime : InputActionResolver_OneTime
{
    private readonly int mouseButtonIndex;

    public MouseButtonInputActionResolver_OneTime(InputAction_OneTime oneTimeAction, int mouseButtonIndex) : base(oneTimeAction)
    {
        this.mouseButtonIndex = mouseButtonIndex;
    }

    public override void UpdateTime(float deltaTime)
    {
        if (Input.GetMouseButtonDown(mouseButtonIndex))
        {
            oneTimeAction.RaiseOnTriggered();
        }
    }
}
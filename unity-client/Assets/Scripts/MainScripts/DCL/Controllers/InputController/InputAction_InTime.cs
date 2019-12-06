using System;
using UnityEngine;

[CreateAssetMenu(fileName = "InputAction_InTime", menuName = "InputActions/InTime")]
public class InputAction_InTime : ScriptableObject
{
    public delegate void Started(DCLAction_InTime action);
    public delegate void Finished(DCLAction_InTime action);
    public event Started OnStarted;
    public event Finished OnFinished;

    [SerializeField] internal DCLAction_InTime dclAction;
    public DCLAction_InTime GetDCLAction() => dclAction;

    public bool isOn { get; private set; }

    internal void RaiseOnStarted()
    {
        isOn = true;
        OnStarted?.Invoke(dclAction);
    }

    internal void RaiseOnFinished()
    {
        isOn = false;
        OnFinished?.Invoke(dclAction);
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

    [UnityEditor.CustomEditor(typeof(InputAction_InTime), true)]
    internal class InputAction_InTimeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (Application.isPlaying && GUILayout.Button("Raise OnStarted"))
            {
                ((InputAction_InTime)target).RaiseOnStarted();
            }
            if (Application.isPlaying && GUILayout.Button("Raise OnFinished"))
            {
                ((InputAction_InTime)target).RaiseOnFinished();
            }
        }
    }
#endif

    #endregion

}
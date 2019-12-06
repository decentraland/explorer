using UnityEngine;

[CreateAssetMenu(fileName = "InputAction_OneTime", menuName = "InputActions/OneTime")]
public class InputAction_OneTime : ScriptableObject
{
    public delegate void Triggered(DCLAction_OneTime action);
    public event Triggered OnTriggered;

    [SerializeField] private DCLAction_OneTime dclAction;
    public DCLAction_OneTime GetDCLAction() => dclAction;

    private int triggeredInFrame = -1;

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
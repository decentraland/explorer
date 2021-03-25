#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unity's default VerticalLayoutGroup aligns the child horizontally. We override this behaviour to avoid that 
/// </summary>
public class DCLVerticalLayoutGroup : VerticalLayoutGroup
{
    public bool ignoreHorizontalAlignment;

    public override void SetLayoutHorizontal()
    {
        ignoreHorizontalAlignment = true;
        if (ignoreHorizontalAlignment)
            return;

        //base.SetLayoutHorizontal();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(DCLVerticalLayoutGroup))]
public class DCLVerticalLayoutGroupEditor : UnityEditor.UI.HorizontalOrVerticalLayoutGroupEditor
{
    private SerializedProperty ignoreHorizontalAlignment;
    private new void OnEnable()
    {
        base.OnEnable();
        ignoreHorizontalAlignment = serializedObject.FindProperty("ignoreHorizontalAlignment");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();
        EditorGUILayout.PropertyField(ignoreHorizontalAlignment);
    }
}
#endif
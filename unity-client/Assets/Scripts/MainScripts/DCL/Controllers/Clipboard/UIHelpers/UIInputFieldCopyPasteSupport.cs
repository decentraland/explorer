using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class UIInputFieldCopyPasteSupport : MonoBehaviour
{
    [SerializeField] private bool supportCopy = true;
    [SerializeField] private bool supportPaste = true;

#if UNITY_WEBGL && !UNITY_EDITOR
    private TMP_InputField inputField;

    private bool isTextSelected;
    private int selectionStart;
    private int selectionEnd;

    private void Awake()
    {
        inputField = gameObject.GetComponent<TMP_InputField>();

        inputField.onTextSelection.AddListener(
            (text, startPos, caretPos) =>
            {
                isTextSelected = true;
                selectionStart = startPos;
                selectionEnd = caretPos;
            });

        inputField.onEndTextSelection.AddListener(
            (text, startPos, caretPos) => { isTextSelected = false; });
    }

    private void OnEnable()
    {
        if (supportPaste) Clipboard.OnPasteInput += OnPaste;
        if (supportCopy) Clipboard.OnCopyInput += OnCopy;
    }

    private void OnDisable()
    {
        if (supportPaste) Clipboard.OnPasteInput -= OnPaste;
        if (supportCopy) Clipboard.OnCopyInput -= OnCopy;
    }
    
    private void OnPaste(string pasteValue)
    {
        if (!inputField.isFocused)
            return;

        int selectionBlockBegin = inputField.caretPosition;
        int selectionBlockEnd = selectionBlockBegin;

        if (isTextSelected)
        {
            // NOTE: user can make selection from left to right or right to left
            selectionBlockBegin = Mathf.Min(selectionStart, selectionEnd);
            selectionBlockEnd = Mathf.Max(selectionStart, selectionEnd);
        }

        inputField.text = InsertString(inputField.text, pasteValue, selectionBlockBegin, selectionBlockEnd);
        inputField.caretPosition = selectionBlockBegin + pasteValue.Length;
    }

    private void OnCopy()
    {
        if (!inputField.isFocused || !isTextSelected)
            return;

        int selectionBlockBegin = Mathf.Min(selectionStart, selectionEnd);
        int selectionBlockEnd = Mathf.Max(selectionStart, selectionEnd);
        string copy = inputField.text.Substring(
            selectionBlockBegin,
            Mathf.Min(inputField.text.Length - selectionBlockBegin, selectionBlockEnd - selectionBlockBegin)
        );
        Clipboard.WriteText(copy);
    }

    private static string InsertString(string original, string insert, int startPosition, int endPosition)
    {
        return (startPosition <= original.Length ? original.Substring(0, startPosition) : "")
               + insert
               + (endPosition < original.Length ? original.Substring(endPosition) : "");
    }

#endif
}
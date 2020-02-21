using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DCL.Interface;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMP_OpenLinkInTab : MonoBehaviour, IPointerClickHandler
{
    TextMeshProUGUI textMeshPro;

    void Awake()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkId = TMP_TextUtilities.FindIntersectingLink(textMeshPro, Input.mousePosition, null);
        if (linkId != -1)
        {
            TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkId];
            WebInterface.OpenURL(linkInfo.GetLinkID());
        }
    }
}
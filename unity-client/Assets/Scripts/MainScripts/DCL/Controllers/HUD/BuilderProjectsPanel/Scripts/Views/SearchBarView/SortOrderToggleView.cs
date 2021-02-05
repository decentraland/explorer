using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal class SortOrderToggleView : MonoBehaviour, IPointerClickHandler
{
    public delegate void OnToggleDelegate(bool isDescending);
    public event OnToggleDelegate OnToggle;

    [SerializeField] private Color selectedColor;
    [SerializeField] private Color deselectedColor;
    [SerializeField] private Image ascendingImage;
    [SerializeField] private Image descendingImage;

    public bool isDescending { private set; get; } = true;

    public void Set(bool descending)
    {
        ascendingImage.color = descending ? deselectedColor : selectedColor;
        descendingImage.color = descending ? selectedColor : deselectedColor;
        isDescending = descending;
        OnToggle?.Invoke(descending);
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        Set(!isDescending);
    }
}

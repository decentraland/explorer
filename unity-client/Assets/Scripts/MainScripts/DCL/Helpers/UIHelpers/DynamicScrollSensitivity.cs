using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Let to set a dynamic sensitivity value depending on the height of the content container.
/// Call to RecalculateSensitivity() function to calculate and apply the new sensitivity value.
/// </summary>
public class DynamicScrollSensitivity : MonoBehaviour
{
    [Tooltip("Scroll Rect component that will be modified.")]
    public ScrollRect mainScrollRect;
    [Tooltip("Viewport associated to the Scroll Rect.")]
    public RectTransform viewport;
    [Tooltip("Transform that will contain all the items of the viewport.")]
    public RectTransform contentContainer;
    [Tooltip("Min value for the calculated scroll sensitivity.")]
    public float minSensitivity = 10f;
    [Tooltip("Max value for the calculated scroll sensitivity.")]
    public float maxSensitivity = 100f;

    private void Awake() { RecalculateSensitivity(); }

    /// <summary>
    /// Recalculate the scroll sensitivity value depending on the current height of the content container.
    /// </summary>
    [ContextMenu("Recalculate Sensitivity")]
    public void RecalculateSensitivity()
    {
        float newSensitivity = contentContainer.rect.height * minSensitivity / viewport.rect.height;
        mainScrollRect.scrollSensitivity = Mathf.Clamp(newSensitivity, minSensitivity, maxSensitivity);
    }
}
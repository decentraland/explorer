using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class LoadingBar : MonoBehaviour
{
    public float currentPercentage { get; set; }

    [SerializeField] internal TMP_Text percentageText;
    [SerializeField] internal RectTransform loadingBar;

    private float maxBarWeidth;

    private void Awake() { maxBarWeidth = ((RectTransform)transform).rect.size.x; }

    public void SetActive(bool isActive) { gameObject.SetActive(isActive); }

    public void SetPercentage(float newValue)
    {
        currentPercentage = newValue;
        percentageText.text = $"{newValue.ToString("0")}%";
        loadingBar.sizeDelta = new Vector2(newValue * maxBarWeidth / 100f, loadingBar.sizeDelta.y);
    }
}
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class SearchBarView : MonoBehaviour
{
    public delegate void OnFilterDelegate(bool isOwner, bool isOperator, bool isContributor);
    public delegate void OnSortOrderToggleDelegate(bool isDescending);

    public event OnFilterDelegate OnFilter;
    public event Action<string> OnSortType;
    public event Action<string> OnSearch;
    public event OnSortOrderToggleDelegate OnSortOrderChanged;

    [SerializeField] internal SearchInputField inputField;
    [SerializeField] internal Button sortButton;
    [SerializeField] internal TextMeshProUGUI sortTypeLabel;
    [SerializeField] internal SortOrderToggleView sortOrderToggle;
    [SerializeField] internal Toggle ownerToggle;
    [SerializeField] internal Toggle operatorToggle;
    [SerializeField] internal Toggle contributorToggle;
    [SerializeField] private TextMeshProUGUI resultLabel;
    [SerializeField] internal SortDropdownView sortDropdown;

    private string resultFormat;
    private bool filterOwner = false;
    private bool filterOperator = false;
    private bool filterContributor = false;

    private void Awake()
    {
        resultFormat = resultLabel.text;

        sortButton.onClick.AddListener(OnSortButtonPressed);

        ownerToggle.onValueChanged.AddListener(OnToggleOwner);
        operatorToggle.onValueChanged.AddListener(OnToggleOperator);
        contributorToggle.onValueChanged.AddListener(OnToggleContributor);

        filterOwner = ownerToggle.isOn;
        filterOperator = operatorToggle.isOn;
        filterContributor = contributorToggle.isOn;

        inputField.OnSearchText += s => OnSearch?.Invoke(s);
        sortOrderToggle.OnToggle += b => OnSortOrderChanged?.Invoke(b);
        sortDropdown.OnSortTypeSelected += OnSortTypeSelected;
    }

    public void SetResultCount(int count)
    {
        resultLabel.text = string.Format(resultFormat, count);
    }

    public void ShowFilters(bool filterOwner, bool filterOperator, bool filterContributor)
    {
        ownerToggle.gameObject.SetActive(filterOwner);
        operatorToggle.gameObject.SetActive(filterOperator);
        contributorToggle.gameObject.SetActive(filterContributor);
    }

    public void SetSortTypes(string[] types)
    {
        sortDropdown.AddSortType(types);
    }

    private void OnSortButtonPressed()
    {
        if (sortDropdown.GetSortTypesCount() > 1)
        {
            sortDropdown.Show();
        }
    }

    private void OnSortTypeSelected(string type)
    {
        sortTypeLabel.text = type;
        OnSortType?.Invoke(type);
    }

    private void OnToggleOwner(bool isOn)
    {
        filterOwner = isOn;
        ReportFilter();
    }

    private void OnToggleOperator(bool isOn)
    {
        filterOperator = isOn;
        ReportFilter();
    }

    private void OnToggleContributor(bool isOn)
    {
        filterContributor = isOn;
        ReportFilter();
    }

    private void ReportFilter()
    {
        OnFilter?.Invoke(filterOwner, filterOperator, filterContributor);
    }
}

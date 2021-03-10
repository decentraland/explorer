using System;
using UnityEngine;

internal class UsersSearchPromptView : MonoBehaviour, IDisposable
{
    public event Action<string> OnSearchText; 
    public event Action OnShouldHide; 
    
    [SerializeField] private SearchInputField searchInputField;
    [SerializeField] private GameObject emptyListGO;
    [SerializeField] private Transform friendListParent;
    [SerializeField] private UserElementView userElementBase;
    [SerializeField] private ShowHideAnimator showHideAnimator;

    private bool isDestroyed = false;
    private RectTransform rectTransform;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        searchInputField.OnSearchText += OnSearch;
        SetFriendListEmpty(true);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        searchInputField.OnSearchText -= OnSearch;
        isDestroyed = true;
    }
    
    private void Update()
    {
        HideIfClickedOutside();
    }

    public void Dispose()
    {
        if (!isDestroyed)
        {
            Destroy(gameObject);
        }
    }
    
    public void SetFriendListEmpty(bool isEmpty)
    {
        emptyListGO.SetActive(isEmpty);
        friendListParent.gameObject.SetActive(!isEmpty);
    }

    public void ClearSearch()
    {
        searchInputField.ClearSearch();
    }

    public UserElementView GetUsersBaseElement()
    {
        return userElementBase;
    }

    public Transform GetUserElementsParent()
    {
        return friendListParent;
    }

    public void Show()
    {
        this.enabled = true;
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        showHideAnimator.Show();
    }

    public void Hide()
    {
        showHideAnimator.Hide();
    }

    private void OnSearch(string value)
    {
        OnSearchText?.Invoke(value);
    }
    
    private void HideIfClickedOutside()
    {
        if (Input.GetMouseButtonDown(0) &&
            !RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
        {
            OnShouldHide?.Invoke();
            this.enabled = false;
        }
    }
}

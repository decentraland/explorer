using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

internal class SortDropdownView : MonoBehaviour, IDeselectHandler
{
    [SerializeField] private Transform buttonsContainer;
    [SerializeField] private SortDropdownButton dropdownButtonBase;
    [SerializeField] private ShowHideAnimator showHideAnimator;

    private readonly Queue<SortDropdownButton> buttonsPool = new Queue<SortDropdownButton>();
    private readonly List<SortDropdownButton> activeButtons = new List<SortDropdownButton>();

    private void Awake()
    {
        dropdownButtonBase.gameObject.SetActive(false);
        buttonsPool.Enqueue(dropdownButtonBase);
        gameObject.SetActive(false);
    }

    public int GetSortTypesCount()
    {
        return activeButtons.Count;
    }

    public void Show()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        EventSystem.current.SetSelectedGameObject(gameObject);
        showHideAnimator.Show();
    }

    public void Hide()
    {
        showHideAnimator.Hide();
    }

    public void AddSortType(string[] texts)
    {
        for (int i = 0; i < texts.Length; i++)
        {
            AddSortType(texts[i]);
        }
    }

    public void AddSortType(string text)
    {
        SortDropdownButton button;
        if (buttonsPool.Count > 0)
        {
            button = buttonsPool.Dequeue();
        }
        else
        {
            button = Object.Instantiate(dropdownButtonBase, buttonsContainer);
            button.transform.ResetLocalTRS();
        }
        button.SetText(text);
        button.gameObject.SetActive(true);
        activeButtons.Add(button);
    }

    public void Clear()
    {
        for (int i = 0; i < activeButtons.Count; i++)
        {
            activeButtons[i].gameObject.SetActive(false);
            buttonsPool.Enqueue(activeButtons[i]);
        }
        activeButtons.Clear();
    }

    void IDeselectHandler.OnDeselect(BaseEventData eventData)
    {
        Hide();
    }
}

using System;
using System.Collections.Generic;

internal class SectionSearchHandler : ISectionSearchHandler
{
    public const string NAME_SORT_TYPE = "NAME";
    public const string SIZE_SORT_TYPE = "SIZE";

    private readonly string[] scenesSortTypes = { NAME_SORT_TYPE, SIZE_SORT_TYPE };

    public event Action OnUpdated;
    public event Action<List<ISearchInfo>> OnResult;

    private readonly SearchHandler<ISearchInfo> scenesSearchHandler;

    private bool filterOwner = false;
    private bool filterOperator = false;
    private bool filterContributor = false;
    
    string[] ISectionSearchHandler.sortTypes => scenesSortTypes;
    string ISectionSearchHandler.searchString => scenesSearchHandler.currentSearchString;
    bool ISectionSearchHandler.filterOwner => filterOwner;
    bool ISectionSearchHandler.filterOperator => filterOperator;
    bool ISectionSearchHandler.filterContributor => filterContributor;
    bool ISectionSearchHandler.descendingSortOrder => scenesSearchHandler.isDescendingSortOrder;
    string ISectionSearchHandler.sortType => scenesSearchHandler.currentSortingType;
    int ISectionSearchHandler.resultCount => scenesSearchHandler.resultCount;

    public SectionSearchHandler()
    {
        scenesSearchHandler = new SearchHandler<ISearchInfo>(scenesSortTypes, (item) =>
        {
            bool result = true;
            if (filterContributor)
                result = item.isContributor;
            if (filterOperator && result)
                result = item.isOperator;
            if (filterOwner && result)
                result = item.isOwner;
            return result;
        });

        scenesSearchHandler.OnSearchChanged += list =>
        {
            OnUpdated?.Invoke();
            OnResult?.Invoke(list);
        };
    }

    void ISectionSearchHandler.SetSearchableList(List<ISearchInfo> list)
    {
        scenesSearchHandler.SetSearchableList(list);
    }

    void ISectionSearchHandler.AddItem(ISearchInfo item)
    {
        scenesSearchHandler.AddItem(item);
    }

    void ISectionSearchHandler.RemoveItem(ISearchInfo item)
    {
        scenesSearchHandler.RemoveItem(item);
    }

    void ISectionSearchHandler.SetFilter(bool isOwner, bool isOperator, bool isContributor)
    {
        filterOwner = isOwner;
        filterOperator = isOperator;
        filterContributor = isContributor;
        scenesSearchHandler.NotifyFilterChanged();
    }

    void ISectionSearchHandler.SetSortType(string sortType)
    {
        scenesSearchHandler.NotifySortTypeChanged(sortType);
    }

    void ISectionSearchHandler.SetSortOrder(bool isDescending)
    {
        scenesSearchHandler.NotifySortOrderChanged(isDescending);
    }

    void ISectionSearchHandler.SetSearchString(string searchText)
    {
        scenesSearchHandler.NotifySearchChanged(searchText);
    }
}
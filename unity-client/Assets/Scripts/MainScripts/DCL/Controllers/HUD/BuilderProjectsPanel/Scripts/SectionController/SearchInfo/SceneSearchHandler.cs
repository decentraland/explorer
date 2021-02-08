    using System;
    using System.Collections.Generic;

    internal class SceneSearchHandler : ISectionSearchHandler
    {
        public const string NAME_SORT_TYPE = "NAME";
        public const string SIZE_SORT_TYPE = "SIZE";

        private readonly string[] scenesSortTypes = { NAME_SORT_TYPE, SIZE_SORT_TYPE };

        public event Action OnUpdated;
        public event Action<List<SearchInfoScene>> OnResult;

        private SearchHandler<SearchInfoScene> scenesSearchHandler;

        private bool filterOwner = false;
        private bool filterOperator = false;
        private bool filterContributor = false;

        public SceneSearchHandler()
        {
            scenesSearchHandler = new SearchHandler<SearchInfoScene>(scenesSortTypes, (item) =>
            {
                if (filterContributor)
                    return item.isContributor;
                if (filterOperator)
                    return item.isOperator;
                if (filterOwner)
                    return item.isOwner;
                return true;
            });

            scenesSearchHandler.OnSearchChanged += list =>
            {
                OnUpdated?.Invoke();
                OnResult?.Invoke(list);
            };
        }

        public void SetSearchableList(List<SearchInfoScene> list)
        {
            scenesSearchHandler.SetSearchableList(list);
        }

        public void AddItem(SearchInfoScene item)
        {
            scenesSearchHandler.AddItem(item);
        }

        public void RemoveItem(SearchInfoScene item)
        {
            scenesSearchHandler.RemoveItem(item);
        }

        string[] ISectionSearchHandler.sortTypes => scenesSortTypes;
        string ISectionSearchHandler.searchString => scenesSearchHandler.currentSearchString;
        bool ISectionSearchHandler.filterOwner => filterOwner;
        bool ISectionSearchHandler.filterOperator => filterOperator;
        bool ISectionSearchHandler.filterContributor => filterContributor;
        bool ISectionSearchHandler.descendingSortOrder => scenesSearchHandler.isDescendingSortOrder;
        string ISectionSearchHandler.sortType => scenesSearchHandler.currentSortingType;
        int ISectionSearchHandler.resultCount => scenesSearchHandler.resultCount;

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

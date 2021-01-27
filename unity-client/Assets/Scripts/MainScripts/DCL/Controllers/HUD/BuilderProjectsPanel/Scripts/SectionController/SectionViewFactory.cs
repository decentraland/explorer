using UnityEngine;

internal interface ISectionViewFactory
{
    GameObject GetViewPrefab(SectionsController.SectionId id);
}

[System.Serializable]
internal class SectionViewFactory : ISectionViewFactory
{
    [System.Serializable]
    internal class Section
    {
        public SectionsController.SectionId id;
        public GameObject view;
    }

    [SerializeField] private Section[] sections;

    GameObject ISectionViewFactory.GetViewPrefab(SectionsController.SectionId id)
    {
        for (int i = 0; i < sections.Length; i++)
        {
            if (sections[i].id == id)
            {
                return sections[i].view;
            }
        }

        return null;
    }
}
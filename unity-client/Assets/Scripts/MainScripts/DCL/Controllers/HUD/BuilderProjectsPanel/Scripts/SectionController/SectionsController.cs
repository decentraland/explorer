using System;
using System.Collections.Generic;
using UnityEngine;

internal class SectionsController : IDisposable
{
    public event Action<SectionBase> OnSectionLoaded;
    public event Action<SectionBase> OnSectionShow;
    public event Action<SectionBase> OnSectionHide;

    private Dictionary<SectionId, SectionBase> loadedSections = new Dictionary<SectionId, SectionBase>();
    private Transform sectionsParent;
    private SectionBase currentOpenSection;

    public enum SectionId
    {
        SCENES_MAIN,
        SCENES_DEPLOYED,
        SCENES_PROJECT,
        LAND
    }

    public SectionsController(Transform sectionsParent)
    {
        this.sectionsParent = sectionsParent;
    }

    public SectionBase GetOrLoadSection(SectionId id)
    {
        if (loadedSections.TryGetValue(id, out SectionBase section))
        {
            return section;
        }

        section = InstantiateSection(id);

        loadedSections.Add(id,section);
        OnSectionLoaded?.Invoke(section);
        return section;
    }

    public void OpenSection(SectionId id)
    {
        var section = GetOrLoadSection(id);
        OpenSection(section);
    }

    private void OpenSection(SectionBase section)
    {
        if (currentOpenSection == section)
            return;

        if (currentOpenSection != null)
        {
            currentOpenSection.SetVisible(false);
            OnSectionHide?.Invoke(currentOpenSection);
        }

        currentOpenSection = section;

        if (currentOpenSection != null)
        {
            currentOpenSection.SetVisible(true);
            OnSectionShow?.Invoke(currentOpenSection);
        }
    }

    private SectionBase InstantiateSection(SectionId id)
    {
        SectionBase result = null;
        switch (id)
        {
            case SectionId.SCENES_MAIN:
                result = new SectionScenesController();
                break;
            case SectionId.SCENES_DEPLOYED:
                break;
            case SectionId.SCENES_PROJECT:
                break;
            case SectionId.LAND:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(id), id, null);
        }

        result?.SetViewContainer(sectionsParent);

        return result;
    }

    public void Dispose()
    {
        using (var iterator = loadedSections.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                iterator.Current.Value.Dispose();
            }
        }
        loadedSections.Clear();
    }
}

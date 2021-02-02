using System;
using UnityEngine;

internal abstract class SectionBase : IDisposable
{
    public event Action OnRequestContextMenuHide;
    public event Action<SectionsController.SectionId> OnRequestOpenSection;

    public bool isVisible { get; private set; } = false;

    public abstract void SetViewContainer(Transform viewContainer);
    public abstract void Dispose();
    protected abstract void OnShow();
    protected abstract void OnHide();

    public void SetVisible(bool visible)
    {
        if (isVisible == visible)
            return;

        isVisible = visible;
        if (visible) OnShow();
        else OnHide();
    }

    protected void RequestOpenSection(SectionsController.SectionId id)
    {
        OnRequestOpenSection?.Invoke(id);
    }

    protected void RequestHideContextMenu()
    {
        OnRequestContextMenuHide?.Invoke();
    }
}
using System;
using UnityEngine;

internal abstract class SectionBase : IDisposable
{
    protected GameObject viewGO;

    public abstract SectionsController.SectionId id { get; }
    public bool isVisible { get; private set; } = false;

    public abstract void Dispose();

    public void SetVisible(bool visible)
    {
        if (isVisible == visible)
            return;

        isVisible = visible;
        viewGO.SetActive(visible);

        if (visible) OnShow();
        else OnHide();
    }

    public virtual void OnShow() { }
    public virtual void OnHide() { }

    protected SectionBase(GameObject viewGO)
    {
        this.viewGO = viewGO;
    }
}
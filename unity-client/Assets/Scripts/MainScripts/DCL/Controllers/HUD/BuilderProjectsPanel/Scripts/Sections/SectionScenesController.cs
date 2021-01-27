using UnityEngine;

internal class SectionScenesController : SectionBase
{
    public override SectionsController.SectionId id => SectionsController.SectionId.SCENES_MAIN;

    private SectionScenesView view;

    public SectionScenesController(GameObject viewGO) : base(viewGO)
    {
        view = viewGO.GetComponent<SectionScenesView>();
    }

    public override void Dispose()
    {
        Object.Destroy(viewGO);
    }
}

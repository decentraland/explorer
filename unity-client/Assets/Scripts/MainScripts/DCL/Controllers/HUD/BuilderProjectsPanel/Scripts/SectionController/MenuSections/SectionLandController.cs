using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using UnityEngine;
using Object = UnityEngine.Object;

internal class SectionLandController : SectionBase, ILandsListener
{
    public const string VIEW_PREFAB_PATH = "BuilderProjectsPanelMenuSections/SectionLandsView";
    
    public override ISectionSearchHandler searchHandler => landSearchHandler;

    private readonly SectionLandView view;

    private readonly LandSearchHandler landSearchHandler = new LandSearchHandler();
    private readonly Dictionary<string, LandElementView> landElementViews = new Dictionary<string, LandElementView>();
    private readonly Queue<LandElementView> landElementViewsPool = new Queue<LandElementView>();

    public SectionLandController() : this(
        Object.Instantiate(Resources.Load<SectionLandView>(VIEW_PREFAB_PATH))
    ) { }

    public SectionLandController(SectionLandView view)
    {
        this.view = view;
        PoolView(view.GetLandElementeBaseView());

        landSearchHandler.OnResult += OnSearchResult;
    }

    public override void SetViewContainer(Transform viewContainer)
    {
        view.SetParent(viewContainer);
    }

    public override void Dispose()
    {
        view.Dispose();
    }

    protected override void OnShow()
    {
        view.SetActive(true);
    }

    protected override void OnHide()
    {
        view.SetActive(false);
    }

    void ILandsListener.OnSetLands(List<LandWithAccess> lands)
    {
        view.SetEmpty(lands == null || lands.Count == 0);
        
        if (lands == null)
            return;

        List<LandElementView> toRemove = landElementViews.Values
                                                         .Where(landElementView => lands.All(land => land.id != landElementView.GetId()))
                                                         .ToList();

        for (int i = 0; i < toRemove.Count; i++)
        {
            landElementViews.Remove(toRemove[i].GetId());
            PoolView(toRemove[i]);
        }
        
        for (int i = 0; i < lands.Count; i++)
        {
            if (!landElementViews.TryGetValue(lands[i].id, out LandElementView landElementView))
            {
                landElementView = GetPooledView();
                landElementViews.Add(lands[i].id, landElementView);
            }

            bool isEstate = lands[i].type == LandType.ESTATE;
            landElementView.SetId(lands[i].id);
            landElementView.SetName(lands[i].name);
            landElementView.SetCoords(lands[i].@base.x, lands[i].@base.y);
            landElementView.SetSize(lands[i].size);
            landElementView.SetRole(lands[i].role == LandRole.OWNER);
            landElementView.SetThumbnail(GetLandThumbnailUrl(lands[i], isEstate));
            landElementView.SetIsEstate(isEstate);
        }
        landSearchHandler.SetSearchableList(landElementViews.Values.Select(scene => scene.searchInfo).ToList());
    }

    private void OnSearchResult(List<LandSearchInfo> searchInfoLands)
    {
        if (landElementViews == null)
            return;

        using (var iterator = landElementViews.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                iterator.Current.Value.SetParent(view.GetLandElementsContainer());
                iterator.Current.Value.gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < searchInfoLands.Count; i++)
        {
            if (!landElementViews.TryGetValue(searchInfoLands[i].id, out LandElementView landView))
                continue;
            
            landView.gameObject.SetActive(true);
            landView.transform.SetSiblingIndex(i);
        }
        view.ResetScrollRect();
    }

    private void PoolView(LandElementView view)
    {
        view.SetActive(false);
        landElementViewsPool.Enqueue(view);
    }

    private LandElementView GetPooledView()
    {
        LandElementView landView;

        if (landElementViewsPool.Count > 0)
        {
            landView = landElementViewsPool.Dequeue();
        }
        else
        {
            landView = Object.Instantiate(view.GetLandElementeBaseView(), view.GetLandElementsContainer());
        }
        return landView;
    }

    private string GetLandThumbnailUrl(LandWithAccess land, bool isEstate)
    {
        if (land == null)
            return null;
        
        const int width = 100;
        const int height = 100;
        const int sizeFactorParcel = 15;
        const int sizeFactorEstate = 35;
        
        if (!isEstate)
        {
            return MapUtils.GetMarketPlaceThumbnailUrl(new[] { land.@base }, width, height, sizeFactorParcel);
        }

        return MapUtils.GetMarketPlaceThumbnailUrl(land.parcels, width, height, sizeFactorEstate);
    }
}

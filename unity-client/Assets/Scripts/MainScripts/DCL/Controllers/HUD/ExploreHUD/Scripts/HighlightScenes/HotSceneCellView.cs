﻿using System;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;
using TMPro;

internal class HotSceneCellView : MonoBehaviour
{
    const int THMBL_MARKETPLACE_WIDTH = 196;
    const int THMBL_MARKETPLACE_HEIGHT = 143;
    const int THMBL_MARKETPLACE_SIZEFACTOR = 50;

    [Header("Animators")]
    [SerializeField] Animator viewAnimator;
    [SerializeField] ShowHideAnimator jumpInButtonAnimator;

    [Header("Crowd")]
    [SerializeField] GameObject crowdCountContainer;
    [SerializeField] TextMeshProUGUI crowdCount;

    [Header("Events")]
    [SerializeField] GameObject eventsContainer;

    [Header("Friends")]
    [SerializeField] ExploreFriendsView friendsView;
    [SerializeField] GameObject friendsContainer;

    [Header("Scene")]
    [SerializeField] TextMeshProUGUI sceneName;
    [SerializeField] internal RawImageFillParent thumbnailImage;
    [SerializeField] UIHoverCallback sceneInfoButton;

    [Header("UI")]
    [SerializeField] UIHoverCallback jumpInHoverArea;
    [SerializeField] Button_OnPointerDown jumpIn;
    [SerializeField] Sprite errorThumbnail;

    public delegate void JumpInDelegate(Vector2Int coords, string serverName, string layerName);
    public static event JumpInDelegate OnJumpIn;

    public static event Action<HotSceneCellView> OnInfoButtonPointerDown;
    public static event Action OnInfoButtonPointerExit;

    public event Action<Texture2D> OnThumbnailSet;

    public CrowdHandler crowdHandler { private set; get; }
    public MapInfoHandler mapInfoHandler { private set; get; }
    public FriendsHandler friendsHandler { private set; get; }
    public ThumbnailHandler thumbnailHandler { private set; get; }
    public AnimationHandler animationHandler { private set; get; }

    private ViewPool<ExploreFriendsView> friendPool;
    private Dictionary<string, ExploreFriendsView> friendViewById;
    private bool isLoaded = false;

    protected void Awake()
    {
        friendPool = new ViewPool<ExploreFriendsView>(friendsView, 0);
        friendViewById = new Dictionary<string, ExploreFriendsView>();

        crowdHandler = new CrowdHandler();
        crowdHandler.onInfoUpdate += OnCrowdInfoUpdated;

        mapInfoHandler = new MapInfoHandler();
        mapInfoHandler.onInfoUpdate += OnMapInfoUpdated;

        friendsHandler = new FriendsHandler(mapInfoHandler);
        friendsHandler.onFriendAdded += OnFriendAdded;
        friendsHandler.onFriendRemoved += OnFriendRemoved;

        thumbnailHandler = new ThumbnailHandler();
        animationHandler = new AnimationHandler(viewAnimator);

        crowdCountContainer.SetActive(crowdHandler.info.usersTotalCount > 0);
        eventsContainer.SetActive(false);

        jumpInHoverArea.OnPointerEnter += () =>
        {
            jumpInButtonAnimator.gameObject.SetActive(true);
            jumpInButtonAnimator.Show();
        };
        jumpInHoverArea.OnPointerExit += () => jumpInButtonAnimator.Hide();
        sceneInfoButton.OnPointerDown += () => jumpInButtonAnimator.Hide(true);

        // NOTE: we don't use the pointer down callback to avoid being mistakenly pressed while dragging
        jumpIn.onClick.AddListener(JumpInPressed);

        sceneInfoButton.OnPointerDown += () => OnInfoButtonPointerDown?.Invoke(this);
        sceneInfoButton.OnPointerExit += () => OnInfoButtonPointerExit?.Invoke();
    }

    public void JumpInPressed()
    {
        HotScenesController.HotSceneInfo.Realm realm = new HotScenesController.HotSceneInfo.Realm() { layer = null, serverName = null };
        for (int i = 0; i < crowdHandler.info.realms.Length; i++)
        {
            if (crowdHandler.info.realms[i].usersCount < crowdHandler.info.realms[i].usersMax)
            {
                realm = crowdHandler.info.realms[i];
                break;
            }
        }

        OnJumpIn?.Invoke(crowdHandler.info.baseCoords, realm.serverName, realm.layer);
    }

    public void Clear()
    {
        mapInfoHandler.Clear();
        thumbnailHandler.Dispose();
        thumbnailImage.texture = null;
        isLoaded = false;
    }

    private void OnDestroy()
    {
        friendPool.Dispose();
        thumbnailHandler.Dispose();

        crowdHandler.onInfoUpdate -= OnCrowdInfoUpdated;
        mapInfoHandler.onInfoUpdate -= OnMapInfoUpdated;
        friendsHandler.onFriendAdded -= OnFriendAdded;
        friendsHandler.onFriendRemoved -= OnFriendRemoved;
    }

    private void OnEnable()
    {
        jumpInButtonAnimator.gameObject.SetActive(false);
        jumpInHoverArea.enabled = isLoaded;
        if (isLoaded)
        {
            animationHandler.SetLoaded();
        }
    }

    private void OnCrowdInfoUpdated(HotScenesController.HotSceneInfo info)
    {
        crowdCount.text = info.usersTotalCount.ToString();
        crowdCountContainer.SetActive(info.usersTotalCount > 0);
    }

    private void OnFriendAdded(UserProfile profile, Color backgroundColor)
    {
        var view = friendPool.GetView();
        view.SetUserProfile(profile, backgroundColor);
        friendViewById.Add(profile.userId, view);
    }

    private void OnFriendRemoved(UserProfile profile)
    {
        if (friendViewById.TryGetValue(profile.userId, out ExploreFriendsView view))
        {
            friendPool.PoolView(view);
            friendViewById.Remove(profile.userId);
        }
    }

    private void OnMapInfoUpdated(MinimapMetadata.MinimapSceneInfo info)
    {
        sceneName.text = info.name;

        FetchThumbnail(info.previewImageUrl,
            onFail: () => FetchThumbnail(MapUtils.GetMarketPlaceThumbnailUrl(info, THMBL_MARKETPLACE_WIDTH, THMBL_MARKETPLACE_HEIGHT, THMBL_MARKETPLACE_SIZEFACTOR),
            onFail: () => SetThumbnail(errorThumbnail.texture)));
    }

    private void FetchThumbnail(string url, Action onFail)
    {
        thumbnailHandler.FetchThumbnail(url, SetThumbnail, onFail);
    }

    private void SetThumbnail(Texture2D texture)
    {
        thumbnailImage.texture = texture;
        OnThumbnailSet?.Invoke(texture);

        SetLoaded();

        if (!(HUDAudioPlayer.i is null))
            HUDAudioPlayer.i.Play(HUDAudioPlayer.Sound.listItemAppear);
    }

    private void SetLoaded()
    {
        isLoaded = true;
        animationHandler.SetLoaded();
        jumpInHoverArea.enabled = true;
    }
}

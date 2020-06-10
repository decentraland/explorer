using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using DCL.Helpers;
using System.Linq;
using DCL.Interface;
using System.Collections.Generic;

public class TeleportPromptHUDView : MonoBehaviour
{
    const string MARKETPLACE_PARCEL_API = "https://api.decentraland.org/v1/parcels/{0}/{1}";
    const string MARKETPLACE_MAP_API = "https://api.decentraland.org/v1/map.png?width=480&height=237&size=20&center={0}&selected={1}";
    const string EVENTS_API = "https://events.decentraland.org/api/events";

    const string EVENT_STRING_LIVE = "Current event";
    const string EVENT_STRING_TODAY = "Today @ {0:HH:mm}";
    const string SCENE_STRING_NO_OWNER = "Unknown";

    const float SCENE_INFO_TIMEOUT = 3;

    const string TELEPORT_COMMAND_MAGIC = "magic";
    const string TELEPORT_COMMAND_CROWD = "crowd";
    const string TELEPORT_COMMAND_COORDS = "coords";

    [SerializeField] internal GameObject content;

    [Header("Images")]
    [SerializeField] RawImage imageSceneThumbnail;
    [SerializeField] Image imageGotoCrowd;
    [SerializeField] Image imageGotoMagic;

    [Header("Containers")]
    [SerializeField] GameObject containerCoords;
    [SerializeField] GameObject containerMagic;
    [SerializeField] GameObject containerCrowd;
    [SerializeField] GameObject containerScene;
    [SerializeField] GameObject containerEvent;

    [Header("Scene info")]
    [SerializeField] TextMeshProUGUI textCoords;
    [SerializeField] TextMeshProUGUI textSceneName;
    [SerializeField] TextMeshProUGUI textSceneOwner;

    [Header("Event info")]
    [SerializeField] TextMeshProUGUI textEventInfo;
    [SerializeField] TextMeshProUGUI textEventName;
    [SerializeField] TextMeshProUGUI textEventAttendees;

    [Header("Buttons")]
    [SerializeField] Button closeButton;
    [SerializeField] Button continueButton;
    [SerializeField] Button cancelButton;

    [Header("Spinners")]
    [SerializeField] GameObject spinnerGeneral;
    [SerializeField] GameObject spinnerImage;

    Coroutine fetchParcelDataRoutine;
    Coroutine fetchParcelImageRoutine;
    Coroutine fetchEventsRoutine;

    MinimapMetadata.MinimapSceneInfo currentSceneInfo;
    Vector2Int currentCoords;
    Texture2D downloadedBanner;

    string teleportTarget;

    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);
        cancelButton.onClick.AddListener(Hide);
        continueButton.onClick.AddListener(Teleport);
    }

    internal void Teleport(string teleportCommand)
    {
        Reset();
        content.SetActive(true);

        switch (teleportCommand)
        {
            case TELEPORT_COMMAND_MAGIC:
                teleportTarget = teleportCommand;
                TeleportToMagic();
                break;
            case TELEPORT_COMMAND_CROWD:
                teleportTarget = teleportCommand;
                TeleportToCrowd();
                break;
            default:
                var coords = teleportCommand.Split(',')
                        .Select(x => x.Trim())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToArray();
                if (coords.Length == 2)
                {
                    int x, y;
                    if (int.TryParse(coords[0], out x) && int.TryParse(coords[1], out y))
                    {
                        teleportTarget = TELEPORT_COMMAND_COORDS;
                        TeleportToCoords(new Vector2Int(x, y));
                        break;
                    }
                }
                Debug.LogError($"Teleport error: {teleportCommand} is not a valid destination");
                Hide();
                break;
        }
    }
    private void TeleportToMagic()
    {
        containerMagic.SetActive(true);
        imageGotoMagic.gameObject.SetActive(true);
        Utils.UnlockCursor();
    }

    private void TeleportToCrowd()
    {
        containerCrowd.SetActive(true);
        imageGotoCrowd.gameObject.SetActive(true);
        Utils.UnlockCursor();
    }

    private void TeleportToCoords(Vector2Int coords)
    {
        containerCoords.SetActive(true);
        Utils.UnlockCursor();

        currentCoords = coords;
        textCoords.text = $"{coords.x}, {coords.y}";

        spinnerGeneral.SetActive(true);
        fetchParcelDataRoutine = StartCoroutine(FetchSceneInfo(coords, (info) =>
        {
            spinnerGeneral.SetActive(false);
            spinnerImage.SetActive(true);

            SetSceneInfo(info);

            fetchParcelImageRoutine = StartCoroutine(FetchSceneThumbnail(coords, info, (texture) =>
            {
                downloadedBanner = texture;
                imageSceneThumbnail.texture = texture;

                RectTransform rt = (RectTransform)imageSceneThumbnail.transform.parent;
                float h = rt.rect.height;
                float w = h * (texture.width / (float)texture.height);
                imageSceneThumbnail.rectTransform.sizeDelta = new Vector2(w, h);

                spinnerImage.SetActive(false);
                imageSceneThumbnail.gameObject.SetActive(true);
            }));
        }, Hide));

        fetchEventsRoutine = StartCoroutine(FetchEventsData(coords, SetEventInfo));
    }

    internal void Hide()
    {
        content.SetActive(false);

        MinimapMetadata.GetMetadata().OnSceneInfoUpdated -= OnMapMetadataInfoUpdated;

        if (fetchParcelDataRoutine != null) StopCoroutine(fetchParcelDataRoutine);
        if (fetchParcelImageRoutine != null) StopCoroutine(fetchParcelImageRoutine);
        if (fetchEventsRoutine != null) StopCoroutine(fetchEventsRoutine);

        fetchParcelDataRoutine = null;
        fetchParcelImageRoutine = null;
        fetchEventsRoutine = null;

        if (downloadedBanner != null)
        {
            UnityEngine.Object.Destroy(downloadedBanner);
            downloadedBanner = null;
        }
    }

    private void Teleport()
    {
        switch (teleportTarget)
        {
            case TELEPORT_COMMAND_CROWD:
                WebInterface.GoToCrowd();
                break;
            case TELEPORT_COMMAND_MAGIC:
                WebInterface.GoToMagic();
                break;
            case TELEPORT_COMMAND_COORDS:
                WebInterface.GoTo(currentCoords.x, currentCoords.y);
                break;
        }
        Hide();
    }

    private void OnDestroy()
    {
        if (downloadedBanner != null)
        {
            UnityEngine.Object.Destroy(downloadedBanner);
            downloadedBanner = null;
        }
    }

    private void Reset()
    {
        containerCoords.SetActive(false);
        containerCrowd.SetActive(false);
        containerMagic.SetActive(false);
        containerScene.SetActive(false);
        containerEvent.SetActive(false);

        imageSceneThumbnail.gameObject.SetActive(false);
        imageGotoCrowd.gameObject.SetActive(false);
        imageGotoMagic.gameObject.SetActive(false);

        spinnerImage.SetActive(false);
        spinnerGeneral.SetActive(false);
    }

    private void SetSceneInfo(SceneInfo info)
    {
        containerScene.SetActive(true);
        textSceneName.text = !string.IsNullOrEmpty(info.sceneName) ? info.sceneName : ToString(currentCoords);
        textSceneOwner.text = !string.IsNullOrEmpty(info.sceneOwner) ? info.sceneOwner : SCENE_STRING_NO_OWNER;
    }

    private void SetEventInfo(EventInfo eventInfo)
    {
        if (eventInfo.isNow || eventInfo.startsToday)
        {
            containerEvent.SetActive(true);
            if (eventInfo.isNow) textEventInfo.text = EVENT_STRING_LIVE;
            else if (eventInfo.startsToday) textEventInfo.text = string.Format(EVENT_STRING_TODAY, eventInfo.startingDate);
            textEventName.text = eventInfo.name;
            textEventAttendees.text = string.Format("+{0}", eventInfo.attendeesCount);
        }
    }

    IEnumerator FetchSceneInfo(Vector2Int coords, Action<SceneInfo> onResponse, Action onError)
    {
        currentSceneInfo = MinimapMetadata.GetMetadata().GetSceneInfo(coords.x, coords.y);

        if (currentSceneInfo == null)
        {
            MinimapMetadata.GetMetadata().OnSceneInfoUpdated += OnMapMetadataInfoUpdated;
            WebInterface.RequestScenesInfoAroundParcel(new Vector2(coords.x, coords.y), 1);

            float timeOut = Time.unscaledTime + SCENE_INFO_TIMEOUT;

            while (currentSceneInfo == null && Time.unscaledTime < timeOut)
            {
                yield return null;
            }

            MinimapMetadata.GetMetadata().OnSceneInfoUpdated -= OnMapMetadataInfoUpdated;
        }

        if (currentSceneInfo == null)
        {
            yield return FetchSceneInfoFallbackMarketPlace(coords);
        }

        if (currentSceneInfo == null)
        {
            onError?.Invoke();
            yield break;
        }

        onResponse?.Invoke(new SceneInfo()
        {
            sceneName = currentSceneInfo.name,
            sceneOwner = currentSceneInfo.owner,
            sceneDescription = currentSceneInfo.description,
            thumbnailUrl = currentSceneInfo.previewImageUrl,
            parcels = currentSceneInfo.parcels
        });
    }

    IEnumerator FetchSceneInfoFallbackMarketPlace(Vector2Int coords)
    {
        string url = string.Format(MARKETPLACE_PARCEL_API, coords.x, coords.y);
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (!request.isNetworkError && !request.isHttpError)
            {
                ParcelResponse response = Utils.FromJsonWithNulls<ParcelResponse>(request.downloadHandler.text);

                currentSceneInfo = new MinimapMetadata.MinimapSceneInfo()
                {
                    name = response.data.data.name,
                    description = response.data.data.description,
                    owner = SCENE_STRING_NO_OWNER,
                    parcels = new List<Vector2Int>() { coords }
                };
            }
        }
    }

    IEnumerator FetchSceneThumbnail(Vector2Int center, SceneInfo sceneInfo, Action<Texture2D> onResponse)
    {
        string url = sceneInfo.thumbnailUrl;
        if (string.IsNullOrEmpty(url))
        {
            string parcels = string.Join(";", sceneInfo.parcels.Select(x => ToString(x)).ToArray());
            url = string.Format(MARKETPLACE_MAP_API, ToString(center), parcels);

        }
        yield return Utils.FetchTexture(url, onResponse);
    }

    IEnumerator FetchEventsData(Vector2Int coords, Action<EventInfo> onResponse)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(EVENTS_API))
        {
            yield return request.SendWebRequest();

            if (!request.isNetworkError && !request.isHttpError)
            {
                EventResponse response = Utils.FromJsonWithNulls<EventResponse>(request.downloadHandler.text);
                DateTime dateNow = DateTime.Now;

                for (int i = 0; i < response.data.Length; i++)
                {
                    EventData eventData = response.data[i];
                    if (eventData.coordinates[0] == coords.x && eventData.coordinates[1] == coords.y)
                    {
                        DateTime eventStart;
                        DateTime eventEnd;
                        if (DateTime.TryParse(eventData.start_at, out eventStart) && DateTime.TryParse(eventData.finish_at, out eventEnd))
                        {
                            onResponse?.Invoke(new EventInfo()
                            {
                                startsToday = eventStart.Date == dateNow.Date,
                                isNow = eventStart <= dateNow && dateNow <= eventEnd,
                                startingDate = eventStart,
                                name = eventData.name,
                                attendeesCount = eventData.total_attendees
                            });
                            break;
                        }
                    }
                }
            }
        }
    }

    private void OnMapMetadataInfoUpdated(MinimapMetadata.MinimapSceneInfo info)
    {
        if (info.parcels.Any(parcel => parcel == currentCoords))
        {
            currentSceneInfo = info;
        }
    }

    private string ToString(Vector2Int v)
    {
        return string.Format("{0},{1}", v.x, v.y);
    }

    class SceneInfo
    {
        public string sceneName;
        public string sceneOwner;
        public string sceneDescription;
        public string thumbnailUrl;
        public List<Vector2Int> parcels;
    }

    class EventInfo
    {
        public bool isNow = false;
        public bool startsToday = false;
        public DateTime startingDate;
        public string name;
        public int attendeesCount;
    }

    [Serializable]
    struct ParcelResponse
    {
        public bool ok;
        public ParcelData data;
    }

    [Serializable]
    struct ParcelData
    {
        public string id;
        public string owner;
        public string district_id;
        public Data data;
    }

    [Serializable]
    struct Data
    {
        public string name;
        public string description;
    }

    [Serializable]
    struct EventResponse
    {
        public EventData[] data;
    }

    [Serializable]
    struct EventData
    {
        public string name;
        public int total_attendees;
        public string start_at;
        public string finish_at;
        public int[] coordinates;
    }
}

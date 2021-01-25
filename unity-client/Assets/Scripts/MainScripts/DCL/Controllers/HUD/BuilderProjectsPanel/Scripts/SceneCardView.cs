using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal class SceneCardView : MonoBehaviour
{
    public delegate void OnSceneIdEvent(string sceneId);
    public delegate void OnContextMenuPressedEvent(string sceneId, bool isDeployedScene, bool isOwnerOrOperator, bool isContributor);

    public static event OnSceneIdEvent OnJumpInPressed;
    public static event OnSceneIdEvent OnEditorPressed;
    public static event OnContextMenuPressedEvent OnContextMenuPressed;

    public enum UserRole { OWNER, OPERATOR, CONTRIBUTOR, NONE }

    [SerializeField] private Texture2D defaultThumbnail;
    [Space]

    [SerializeField] private RawImageFillParent thumbnail;
    [SerializeField] private TextMeshProUGUI sceneName;
    [Space]

    [SerializeField] internal GameObject coordsContainer;
    [SerializeField] private TextMeshProUGUI coordsText;
    [Space]

    [SerializeField] internal GameObject sizeContainer;
    [SerializeField] private TextMeshProUGUI sizeText;
    [Space]

    [SerializeField] internal Button jumpInButton;
    [SerializeField] internal Button editorButton;
    [SerializeField] private Button contextMenuButton;
    [Space]

    [SerializeField] internal GameObject roleOwnerGO;
    [SerializeField] internal GameObject roleOperatorGO;
    [SerializeField] internal GameObject roleContributorGO;

    private string sceneId;
    private bool isSceneDeployed;
    private UserRole userRole;

    private void Awake()
    {
        jumpInButton.onClick.AddListener(()=> OnJumpInPressed?.Invoke(sceneId));
        editorButton.onClick.AddListener(()=> OnEditorPressed?.Invoke(sceneId));

        contextMenuButton.onClick.AddListener(
            ()=>
                OnContextMenuPressed?.Invoke(sceneId, isSceneDeployed,
                    userRole == UserRole.OWNER || userRole == UserRole.OPERATOR,
                    userRole == UserRole.CONTRIBUTOR)
                );
    }

    public void Setup(string sceneId, bool isDeployed, string sceneName,
        Vector2 sceneCoords, Vector2 sceneSize,
        Texture2D thumbnailTexture, UserRole userRole)
    {
        this.sceneId = sceneId;

        SetThumbnail(thumbnailTexture);
        SetName(sceneName);
        SetCoords(sceneCoords);
        SetSize(sceneSize);
        SetDeployed(isDeployed);
        SetUserRole(userRole);
    }

    public void SetName(string name)
    {
        sceneName.text = name;
    }

    public void SetCoords(Vector2 coords)
    {
        coordsText.text = $"{coords.x},{coords.y}m";
    }

    public void SetSize(Vector2 size)
    {
        sizeText.text = $"{size.x},{size.y}m";
    }

    public void SetThumbnail(Texture2D thumbnailTexture)
    {
        thumbnail.texture = thumbnailTexture ?? defaultThumbnail;
    }

    public void SetDeployed(bool deployed)
    {
        isSceneDeployed = deployed;
        coordsContainer.SetActive(deployed);
        sizeContainer.SetActive(!deployed);
        jumpInButton.gameObject.SetActive(deployed);
    }

    public void SetUserRole(UserRole role)
    {
        userRole = role;

        roleOwnerGO.SetActive(false);
        roleOperatorGO.SetActive(false);
        roleContributorGO.SetActive(false);

        switch (role)
        {
            case UserRole.OWNER:
                roleOwnerGO.SetActive(true);
                break;
            case UserRole.OPERATOR:
                roleOperatorGO.SetActive(true);
                break;
            case UserRole.CONTRIBUTOR:
                roleContributorGO.SetActive(true);
                break;
        }
    }
}

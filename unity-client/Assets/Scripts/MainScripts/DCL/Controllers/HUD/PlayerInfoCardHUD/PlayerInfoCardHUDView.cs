using System.Collections.Generic;
using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerInfoCardHUDView : MonoBehaviour
{
    private const string PREFAB_PATH = "PlayerInfoCardHUD";

    public enum Tabs
    {
        Passport,
        Trade,
        Block
    }

    [System.Serializable]
    private class TabsMapping
    {
        public GameObject container;
        public Toggle toggle;
        public Tabs tab;
    }

    [SerializeField] private GenericFactory collectiblesFactory;
    [SerializeField] private Canvas cardCanvas;
    [SerializeField] private TabsMapping[] tabsMapping;
    [SerializeField] private Button hideCardButton;

    [Space]
    [SerializeField] private Image avatarPicture;
    [SerializeField] private TextMeshProUGUI name;

    [Header("Passport")]
    [SerializeField] private TextMeshProUGUI description;

    [Header("Trade")]
    [SerializeField] private RectTransform wearablesContainer;

    private readonly List<PlayerInfoCollectibleItem> playerInfoCollectibles = new List<PlayerInfoCollectibleItem>(10);
    private UserProfile currentUserProfile;

    public static PlayerInfoCardHUDView CreateView()
    {
        return Instantiate(Resources.Load<GameObject>(PREFAB_PATH)).GetComponent<PlayerInfoCardHUDView>();
    }

    public void Initialize(UnityAction cardClosedCallback)
    {
        hideCardButton.onClick.RemoveAllListeners();
        hideCardButton.onClick.AddListener(cardClosedCallback);

        for (int index = 0; index < tabsMapping.Length; index++)
        {
            var tab = tabsMapping[index];
            tab.toggle.onValueChanged.RemoveAllListeners();
            tab.toggle.onValueChanged.AddListener((on) => { UpdateTabs(); });
        }

        for (int index = 0; index < tabsMapping.Length; index++)
        {
            if (tabsMapping[index].tab == Tabs.Passport)
            {
                tabsMapping[index].toggle.isOn = true;
                break;
            }
        }
    }

    public void SetCardActive(bool active)
    {
        cardCanvas.enabled = active;
    }

    private void UpdateTabs()
    {
        for (int index = 0; index < tabsMapping.Length; index++)
        {
            tabsMapping[index].container.SetActive(tabsMapping[index].toggle.isOn);
        }
    }
    
    public void SetUserProfile(UserProfile userProfile)
    {
        currentUserProfile = userProfile;
        name.text = currentUserProfile.userName;
        description.text = currentUserProfile.description;
        avatarPicture.sprite = currentUserProfile.faceSnapshot;

        ClearCollectibles();
        var collectiblesIds = currentUserProfile.GetInventoryItemsIds();
        for (int index = 0; index < collectiblesIds.Length; index++)
        {
            string collectibleId = collectiblesIds[index];
            WearableItem collectible = CatalogController.wearableCatalog.Get(collectibleId);
            if (collectible == null) continue;

            var playerInfoCollectible = collectiblesFactory.Instantiate<PlayerInfoCollectibleItem>(collectible.rarity, wearablesContainer.transform);
            if (playerInfoCollectible == null) continue;
            playerInfoCollectibles.Add(playerInfoCollectible);
            playerInfoCollectible.Initialize(collectible);
        }
    }

    public void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }

    private void ClearCollectibles()
    {
        for (var i = playerInfoCollectibles.Count - 1; i >= 0; i--)
        {
            var playerInfoCollectible = playerInfoCollectibles[i];
            playerInfoCollectibles.RemoveAt(i);
            Destroy(playerInfoCollectible.gameObject);
        }
    }
}
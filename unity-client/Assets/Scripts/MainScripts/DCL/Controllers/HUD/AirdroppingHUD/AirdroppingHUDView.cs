using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AirdroppingHUDView : MonoBehaviour
{
    private const string VIEW_PATH = "AirdroppingHUD";

    [SerializeField] private GenericFactory collectiblesFactory;
    [SerializeField] private GenericFactory erc20Factory;

    [Header("Initial Screen")]
    [SerializeField] private GameObject initialScreen;
    [SerializeField] private TextMeshProUGUI initialScreenTitle;
    [SerializeField] private TextMeshProUGUI initialScreenSubtitle;
    [SerializeField] private Button initialScreenDoneButton;

    [Header("Single Item Screen")]
    [SerializeField] private GameObject singleItemScreen;
    [SerializeField] private GameObject singleItemContainer;
    [SerializeField] private Button singleItemDoneButton;
    [SerializeField] private TextMeshProUGUI itemsLeft;

    [Header("Summary Screen")]
    [SerializeField] private GameObject summaryScreen;
    [SerializeField] private GameObject summaryItemsContainer;
    [SerializeField] private Button summaryDoneButton;

    internal static AirdroppingHUDView Create()
    {
        return Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<AirdroppingHUDView>();
    }

    public void Initialize(UnityAction nextStateCallback)
    {
        initialScreenDoneButton.onClick.RemoveAllListeners();
        initialScreenDoneButton.onClick.AddListener(nextStateCallback);

        singleItemDoneButton.onClick.RemoveAllListeners();
        singleItemDoneButton.onClick.AddListener(nextStateCallback);

        summaryDoneButton.onClick.RemoveAllListeners();
        summaryDoneButton.onClick.AddListener(nextStateCallback);

        CleanState();
    }

    public void ShowInitialScreen(string title, string subtitle)
    {
        CleanState();
        initialScreen.SetActive(true);
        initialScreenTitle.text = title;
        initialScreenSubtitle.text = subtitle;
    }

    public void ShowItemScreen(AirdroppingHUDController.ItemModel model, int itemsleft)
    {
        CleanState();
        singleItemScreen.SetActive(true);
        itemsLeft.text = itemsleft.ToString();
        CreateItemPanel(singleItemContainer.transform, model).SetData(model.name, model.subtitle, model.thumbnailURL);
    }

    public void ShowSummaryScreen(AirdroppingHUDController.ItemModel[] items)
    {
        CleanState();
        summaryScreen.SetActive(true);
        for (int index = 0; index < items.Length; index++)
        {
            var item = items[index];
            CreateItemPanel(summaryItemsContainer.transform, items[index]).SetData(item.name, item.subtitle, item.thumbnailURL);
        }
    }

    public void CleanState()
    {
        initialScreen.SetActive(false);

        singleItemScreen.SetActive(false);
        singleItemContainer.transform.DestroyAllChild();

        summaryScreen.SetActive(false);
        summaryItemsContainer.transform.DestroyAllChild();
    }

    public void SetVisibility(bool active)
    {
        gameObject.SetActive(active);
    }

    private AirdroppingItemPanel CreateItemPanel(Transform parent, AirdroppingHUDController.ItemModel model)
    {
        AirdroppingItemPanel item = null;
        if (model.type == "collectible")
        {
            item = collectiblesFactory.Instantiate<AirdroppingItemPanel>(model.rarity, parent);
        }

        if (model.type == "erc20")
        {
            item = erc20Factory.Instantiate<AirdroppingItemPanel>(model.rarity, parent);
        }

        return item;
    }
}
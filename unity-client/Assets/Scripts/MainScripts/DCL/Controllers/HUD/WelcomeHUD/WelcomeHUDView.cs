using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WelcomeHUDView : MonoBehaviour
{
    private const string PREFAB_PATH = "WelcomeHUD";

    [SerializeField] internal TextMeshProUGUI headerText1;
    [SerializeField] internal TextMeshProUGUI headerText2;
    [SerializeField] internal TextMeshProUGUI bodyText;
    [SerializeField] internal TextMeshProUGUI buttonText;

    [SerializeField] internal Button confirmButton;
    [SerializeField] internal Button closeButton;
    public static WelcomeHUDView CreateView()
    {
        return Instantiate(Resources.Load<GameObject>(PREFAB_PATH)).GetComponent<WelcomeHUDView>();
    }

    public void Initialize(WelcomeHUDController.Model model)
    {
        if (model == null)
            return;

        headerText1.text = model.title;
        headerText2.text = model.timeText;
        bodyText.text = model.bodyText;
        buttonText.text = model.buttonText;

        confirmButton.gameObject.SetActive(model.showButton);
        headerText2.gameObject.SetActive(model.showTime);
    }
}

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

    public void Populate()
    {
    }
}

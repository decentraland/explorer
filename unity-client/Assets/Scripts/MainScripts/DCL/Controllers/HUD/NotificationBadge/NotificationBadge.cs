using UnityEngine;

public class NotificationBadge : MonoBehaviour
{
    [SerializeField] private FloatVariable notificationVariable;
    [SerializeField] private TMPro.TextMeshProUGUI notificationText;
    [SerializeField] private GameObject notificationContainer;

    private void Start()
    {
        if (notificationVariable == null)
            return;

        notificationVariable.OnChange += NotificationVariable_OnChange;
        NotificationVariable_OnChange(notificationVariable.Get(), notificationVariable.Get());
    }

    private void OnDestroy()
    {
        if (notificationVariable == null)
            return;

        notificationVariable.OnChange -= NotificationVariable_OnChange;
    }

    private void NotificationVariable_OnChange(float current, float previous)
    {
        if (current > 0)
        {
            notificationContainer.SetActive(true);

            if (current < 99)
            {
                notificationText.text = ((int)current).ToString();
            }
            else
            {
                notificationText.text = "99+";
            }
        }
        else
        {
            notificationContainer.SetActive(false);
        }
    }
}

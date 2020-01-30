using UnityEngine;

public class GenesisPlazaStageController : TutorialStageController
{
    public override void OnStageStart()
    {
        base.OnStageStart();

        // NOTE: we should probably remove this toast when proper system for wellcome toast is created
        ShowWellcomeToast();

        TutorialController.i?.SetRunningStageFinished();
    }

    private void ShowWellcomeToast()
    {
        string notificationText = $"Wellcome, {UserProfile.GetOwnUserProfile().userName}!";
        Vector2Int currentCoords = CommonScriptableObjects.playerCoords.Get();
        string parcelName = MinimapMetadata.GetMetadata().GetTile(currentCoords.x, currentCoords.y)?.name;
        if (!string.IsNullOrEmpty(parcelName))
        {
            notificationText += $" You are in {parcelName} {currentCoords.x}, {currentCoords.y}";
        }

        NotificationModel model = new NotificationModel()
        {
            message = notificationText,
            scene = "",
            type = NotificationModel.NotificationType.GENERIC_WITHOUT_BUTTON,
            timer = 5
        };

        HUDController.i?.notificationHud.ShowNotification(model);
    }
}

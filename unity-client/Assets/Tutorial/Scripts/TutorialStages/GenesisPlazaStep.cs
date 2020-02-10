public class GenesisPlazaStep : TutorialStep
{

    public override void OnStepStart()
    {
        base.OnStepStart();
        HUDController.i?.ShowWelcomeNotification();
    }
}

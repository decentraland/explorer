namespace DCL.Tutorial
{
    /// <summary>
    /// Class that represents the onboarding tutorial step related to show the social features available in the taskbar.
    /// </summary>
    public class TutorialStep_Tooltip_SocialFeatures : TutorialStep_Tooltip
    {
        protected override void SetTooltipPosition()
        {
            base.SetTooltipPosition();

            if (tutorialController != null &&
                tutorialController.hudController != null &&
                tutorialController.hudController.taskbarHud != null)
            {
                if (tutorialController.hudController.taskbarHud.socialTooltipReference)
                {
                    tooltipTransform.position =
                        tutorialController.hudController.taskbarHud.socialTooltipReference.position;
                }
            }
        }
    }
}
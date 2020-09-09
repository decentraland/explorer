using DCL.Interface;

namespace DCL.GoToGenesisPlazaHUD
{
    public class GoToGenesisPlazaHUDController : IHUD
    {
        public GoToGenesisPlazaHUDView view { private set; get; }

        public GoToGenesisPlazaHUDController()
        {
            view = GoToGenesisPlazaHUDView.Create();

            view.continueButton.onClick.AddListener(OnGoToGenesisButtonClick);
        }

        public void SetVisibility(bool visible)
        {
            view.SetVisibility(visible);
        }

        public void Dispose()
        {
            if (view != null)
            {
                view.continueButton.onClick.RemoveListener(OnGoToGenesisButtonClick);
                UnityEngine.Object.Destroy(view.gameObject);
            }
        }

        private void OnGoToGenesisButtonClick()
        {
            //CommonScriptableObjects.rendererState.OnChange += RendererState_OnChange;

            WebInterface.GoTo(0, 0);

            //if (tutorialController != null)
            //    tutorialController.SetTutorialDisabled();
        }

        //private void RendererState_OnChange(bool current, bool previous)
        //{
        //    if (current)
        //    {
        //        CommonScriptableObjects.rendererState.OnChange -= RendererState_OnChange;
        //        if (SceneController.i != null)
        //            SceneController.i.OnSortScenes += SceneController_OnSortScenes;
        //    }
        //}

        //private void SceneController_OnSortScenes()
        //{
        //    SceneController.i.OnSortScenes -= SceneController_OnSortScenes;

        //    if (tutorialController != null)
        //        tutorialController.SetTutorialEnabled(false.ToString());
        //}
    }
}
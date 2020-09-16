using UnityEngine;
using UnityEngine.UI;

namespace DCL.GoToGenesisPlazaHUD
{
    public class GoToGenesisPlazaHUDView : MonoBehaviour
    {
        public bool isOpen { get; private set; } = false;

        public event System.Action OnClose;

        private const string PATH = "GoToGenesisPlazaHUD";
        private const string VIEW_OBJECT_NAME = "_GoToGenesisPlazaHUD";

        [SerializeField] private ShowHideAnimator goToGenesisPlazaAnimator;
        [SerializeField] private Button cancelButton;
        [SerializeField] internal Button continueButton;

        private void Initialize()
        {
            gameObject.name = VIEW_OBJECT_NAME;

            cancelButton.onClick.AddListener(() =>
            {
                SetVisibility(false);
            });
        }

        public static GoToGenesisPlazaHUDView Create()
        {
            GoToGenesisPlazaHUDView view = Instantiate(Resources.Load<GameObject>(PATH)).GetComponent<GoToGenesisPlazaHUDView>();
            view.Initialize();
            return view;
        }

        public void SetVisibility(bool visible)
        {
            if (visible)
                goToGenesisPlazaAnimator.Show();
            else
                goToGenesisPlazaAnimator.Hide();

            if (!visible && isOpen)
                OnClose?.Invoke();

            isOpen = visible;
        }
    }
}
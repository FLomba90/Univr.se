using Abstractions;
using Abstractions.Enums;
using AppExtensions;
using Settings;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Controllers
{
    public class Matchmaking_ErrorPanel : MenuView
    {
        #region resources
        public override ViewType ViewType => ViewType.Error;
        protected override string LOG_TAG => nameof(Matchmaking_ErrorPanel);

        #endregion resources

        #region data
        [Header("Ui bindings")]
        [SerializeField]
        TMPro.TextMeshProUGUI m_title;
        [SerializeField]
        TMPro.TextMeshProUGUI m_description;
        [SerializeField]
        Button m_backButton;
        [SerializeField]
        TMPro.TextMeshProUGUI m_backButtonText;
        [SerializeField]
        Image m_backButtonFrame;
        [SerializeField]
        Image m_panelFrameImage;
        [SerializeField]
        UIGradient m_backButtonImageGradient;
        [SerializeField]
        UIGradient m_backButtonFrameGradient;
        public class Factory : PlaceholderFactory<Matchmaking_ErrorPanel> { }

        #endregion data

        #region dependency injection
        [Inject]
        readonly AppResources m_appResources;

        #endregion dependency injection

        #region monobehaviour callbacks

        protected override void Start()
        {
            base.Start();
            InitObservables();
            InitUiContent();
        }
        #endregion monobehaviour callbacks

        #region callbacks
        void DidTapOnConnectButton(Unit obj)
        {
            Debug.Log($"{LOG_TAG}.{nameof(DidTapOnConnectButton)}");
            MenuController.BackToOrigin();
        }
        private void PointerExitOnConnectButton(BaseEventData obj)
        {
           m_backButtonFrame.gameObject.SetActive(false);
        }
        private void PointerEnterOnConnectButton(BaseEventData obj)
        {
           m_backButtonFrame.gameObject.SetActive(true);
        }

        #endregion callbacks

        #region logic

        void InitUiContent()
        {
            m_title.text = m_appResources.Matchmaking.ErrorTitle;
            m_description.text = string.IsNullOrEmpty(MenuController.LastError.ErrorMessage) ? m_appResources.Matchmaking.DefaultErrorDescription : MenuController.LastError.ErrorMessage;
            m_backButtonText.text = m_appResources.Matchmaking.ErrorButtonText;

           m_backButtonImageGradient.m_color1 = ColorsExtensions.CopyColor(m_appResources.Ui.Pink,m_backButtonImageGradient.m_color1.a);
           m_backButtonImageGradient.m_color2 = ColorsExtensions.CopyColor(m_appResources.Ui.Blue,m_backButtonImageGradient.m_color2.a);

           m_backButtonFrameGradient.m_color1 = ColorsExtensions.CopyColor(m_appResources.Ui.Pink,m_backButtonImageGradient.m_color1.a);
           m_backButtonFrameGradient.m_color2 = ColorsExtensions.CopyColor(m_appResources.Ui.Blue,m_backButtonImageGradient.m_color2.a);
           
           m_panelFrameImage.color = ColorsExtensions.CopyColor(m_appResources.Ui.Error, m_panelFrameImage.color.a);

           m_backButtonFrame.gameObject.SetActive(false);
        }
        void InitObservables()
        {
           m_backButton.onClick.AsObservable().Subscribe(DidTapOnConnectButton).AddTo(Disposer);

            var et =m_backButton.gameObject.AddComponent<EventTrigger>();
            et.triggers = new System.Collections.Generic.List<EventTrigger.Entry>()
            {
                new EventTrigger.Entry()
                {
                    eventID = EventTriggerType.PointerEnter,
                },
                new EventTrigger.Entry()
                {
                    eventID = EventTriggerType.PointerExit,
                },
            };
            et.triggers[0].callback.AsObservable().Subscribe(PointerEnterOnConnectButton).AddTo(Disposer);
            et.triggers[1].callback.AsObservable().Subscribe(PointerExitOnConnectButton).AddTo(Disposer);
        }
        #endregion logic
    }
}
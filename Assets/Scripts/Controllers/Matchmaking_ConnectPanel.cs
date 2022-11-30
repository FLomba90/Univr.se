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
    public class Matchmaking_ConnectPanel : MenuView
    {
        #region resources
        public override ViewType ViewType => ViewType.ConnectToServer;
        protected override string LOG_TAG => nameof(Matchmaking_ConnectPanel);

        #endregion resources

        #region data
        [Header("Ui bindings")]
        [SerializeField]
        TMPro.TextMeshProUGUI m_title;
        [SerializeField]
        TMPro.TextMeshProUGUI m_description;
        [SerializeField]
        Button m_connectButton;
        [SerializeField]
        TMPro.TextMeshProUGUI m_connectButtonText;
        [SerializeField]
        Image m_connectButtonFrame;
        [SerializeField]
        UIGradient m_connectButtonImageGradient;
        [SerializeField]
        UIGradient m_connectButtonFrameGradient;
        public class Factory : PlaceholderFactory<Matchmaking_ConnectPanel> { }

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
            MenuController.StartConnection();
        }
        private void PointerExitOnConnectButton(BaseEventData obj)
        {
            m_connectButtonFrame.gameObject.SetActive(false);
        }
        private void PointerEnterOnConnectButton(BaseEventData obj)
        {
            m_connectButtonFrame.gameObject.SetActive(true);
        }

        #endregion callbacks

        #region logic

        void InitUiContent()
        {
            m_title.text = m_appResources.Matchmaking.ConnectTitle;
            m_description.text = m_appResources.Matchmaking.ConnectDescription;
            m_connectButtonText.text = m_appResources.Matchmaking.ConnectButtonText;

            m_connectButtonImageGradient.m_color1 = ColorsExtensions.CopyColor(m_appResources.Ui.Pink, m_connectButtonImageGradient.m_color1.a);
            m_connectButtonImageGradient.m_color2 = ColorsExtensions.CopyColor(m_appResources.Ui.Blue, m_connectButtonImageGradient.m_color2.a);

            m_connectButtonFrameGradient.m_color1 = ColorsExtensions.CopyColor(m_appResources.Ui.Pink, m_connectButtonImageGradient.m_color1.a);
            m_connectButtonFrameGradient.m_color2 = ColorsExtensions.CopyColor(m_appResources.Ui.Blue, m_connectButtonImageGradient.m_color2.a);


            m_connectButtonFrame.gameObject.SetActive(false);
        }
        void InitObservables()
        {
            m_connectButton.onClick.AsObservable().Subscribe(DidTapOnConnectButton).AddTo(Disposer);

            var et = m_connectButton.gameObject.AddComponent<EventTrigger>();
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
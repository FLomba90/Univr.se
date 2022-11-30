using Abstractions;
using AppExtensions;
using Managers;
using Services;
using Settings;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using static Assets.Scripts.Utils.Utils;

namespace Controllers
{
    public interface IMatchUiController
    {
        void UpdateScore(int team1Score, int team2Score);
        void GameCompleted(int winnerTeam);
    }
    public class MatchUiController : ADisposableMonoBehaviour, IMatchUiController
    {
        #region resources
        protected override string LOG_TAG => nameof(MatchUiController);

        #endregion resources

        #region data
        [Header("Ui bindings")]
        [SerializeField]
         GameObject m_scoreContent;
        [SerializeField]
        TMPro.TextMeshProUGUI m_team1TextLeft;
        [SerializeField]
        TMPro.TextMeshProUGUI m_team1TextRight;
        [SerializeField]
        Image m_team1Icon;
        [SerializeField]
        UIGradient m_team1IconGradient;
        [SerializeField]
        TMPro.TextMeshProUGUI m_team2TextLeft;
        [SerializeField]
        TMPro.TextMeshProUGUI m_team2TextRight;
        [SerializeField]
        Image m_team2Icon;
        [SerializeField]
        UIGradient m_team2IconGradient;
        [SerializeField]
        TMPro.TextMeshProUGUI m_team1Score;
        [SerializeField]
        TMPro.TextMeshProUGUI m_team2Score;
        [Space(3)]
        [Header("Match Completed")]
        [SerializeField]
        GameObject m_gameCompletedContent;
        [SerializeField]
        TMPro.TextMeshProUGUI m_resultTitle;
        [SerializeField]
        TMPro.TextMeshProUGUI m_resultTeam1;
        [SerializeField]
        Image m_resultTeam1Icon;
        [SerializeField]
        UIGradient m_resultTeam1IconGradient;
        [SerializeField]
        TMPro.TextMeshProUGUI m_resultTeam2;
        [SerializeField]
        Image m_resultTeam2Icon;
        [SerializeField]
        TMPro.TextMeshProUGUI m_resultTeam1Score;
        [SerializeField]
        TMPro.TextMeshProUGUI m_resultTeam2Score;
        [SerializeField]
        UIGradient m_resultTeam2IconGradient;
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
        [SerializeField]
        UIGradient m_contentFrame;

        public AppNetworkInternalError LastError { get; private set; }

        #endregion data

        #region dependency injection
        [Inject]
        readonly AppNetworkSettings m_appNetworkSettings;
        [Inject]
        readonly INetworkMatchManager m_networkMatchManager;
        [Inject]
        readonly INetworkService m_networkService;
        [Inject]
        readonly AppResources m_appResources;

        MenuView m_currentActiveView;

        #endregion dependency injection

        #region monobehaviour callbacks
        protected override void Start()
        {
            base.Start();

            InitUiContent();
            InitObservables();
        }

        #endregion monobehaviour callbacks

        #region callbacks
        void DidTapOnBackButton(Unit obj)
        {
            Debug.Log($"{LOG_TAG}.{nameof(DidTapOnBackButton)}");
            m_networkMatchManager.Close();
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

        #region abstractions
        #endregion abstractions

        #region logic

        public void UpdateScore(int team1Score, int team2Score)
        {
            m_team1Score.text = team1Score.ToString();
            m_resultTeam1Score.text = team1Score.ToString();
            m_team2Score.text = team2Score.ToString();
            m_resultTeam2Score.text = team2Score.ToString();
        }
        public void GameCompleted(int winnerTeam)
        {
            m_scoreContent.SetActive(false);
            m_gameCompletedContent.SetActive(true);
            m_resultTeam1.text = winnerTeam == 0 ? m_appResources.Matchmaking.TeamIsWinner : m_appResources.Matchmaking.TeamIsLoser;
            m_resultTeam2.text = winnerTeam == 1 ? m_appResources.Matchmaking.TeamIsWinner : m_appResources.Matchmaking.TeamIsLoser;
        }
        void InitUiContent()
        {
            m_scoreContent.SetActive(true);
            m_gameCompletedContent.SetActive(false);

            m_team1TextLeft.text = m_appResources.GameResources.Team1Name.Split(' ')[0];
            m_team1TextRight.text = m_appResources.GameResources.Team1Name.Split(' ')[1];
            m_team1Icon.sprite = m_appResources.GameResources.Team1Icon;
            m_team1IconGradient.m_color1 = ColorsExtensions.CopyColor(m_appResources.Ui.Pink, m_team1IconGradient.m_color1.a);
            m_team1IconGradient.m_color2 = ColorsExtensions.CopyColor(m_appResources.Ui.Pink, m_team1IconGradient.m_color2.a);

            m_team2TextLeft.text = m_appResources.GameResources.Team2Name.Split(' ')[0];
            m_team2TextRight.text = m_appResources.GameResources.Team2Name.Split(' ')[1];
            m_team2Icon.sprite = m_appResources.GameResources.Team2Icon;
            m_team2IconGradient.m_color1 = ColorsExtensions.CopyColor(m_appResources.Ui.Blue, m_team2IconGradient.m_color1.a);
            m_team2IconGradient.m_color2 = ColorsExtensions.CopyColor(m_appResources.Ui.Blue, m_team2IconGradient.m_color2.a);

            m_resultTitle.text = m_appResources.Matchmaking.MatchCompleted;
            m_backButtonText.text = m_appResources.Matchmaking.CloseButtonText;

            m_backButtonImageGradient.m_color1 = ColorsExtensions.CopyColor(m_appResources.Ui.Pink, m_backButtonImageGradient.m_color1.a);
            m_backButtonImageGradient.m_color2 = ColorsExtensions.CopyColor(m_appResources.Ui.Blue, m_backButtonImageGradient.m_color2.a);

            m_backButtonFrameGradient.m_color1 = ColorsExtensions.CopyColor(m_appResources.Ui.Pink, m_backButtonImageGradient.m_color1.a);
            m_backButtonFrameGradient.m_color2 = ColorsExtensions.CopyColor(m_appResources.Ui.Blue, m_backButtonImageGradient.m_color2.a);

            m_contentFrame.m_color1 = ColorsExtensions.CopyColor(m_appResources.Ui.Pink, m_backButtonImageGradient.m_color1.a);
            m_contentFrame.m_color2 = ColorsExtensions.CopyColor(m_appResources.Ui.Blue, m_backButtonImageGradient.m_color2.a);

            m_resultTeam1Icon.sprite = m_appResources.GameResources.Team1Icon;
            m_resultTeam1Icon.preserveAspect = true;
            m_resultTeam1IconGradient.m_color1 = ColorsExtensions.CopyColor(m_appResources.Ui.Pink, m_resultTeam1IconGradient.m_color1.a);
            m_resultTeam1IconGradient.m_color2 = ColorsExtensions.CopyColor(m_appResources.Ui.Pink, m_resultTeam1IconGradient.m_color2.a);

            m_resultTeam2Icon.sprite = m_appResources.GameResources.Team2Icon;
            m_resultTeam2Icon.preserveAspect = true;
            m_resultTeam2IconGradient.m_color1 = ColorsExtensions.CopyColor(m_appResources.Ui.Blue, m_resultTeam2IconGradient.m_color1.a);
            m_resultTeam2IconGradient.m_color2 = ColorsExtensions.CopyColor(m_appResources.Ui.Blue, m_resultTeam2IconGradient.m_color2.a);
        }

        void InitObservables()
        {
            m_backButton.onClick.AsObservable().Subscribe(DidTapOnBackButton).AddTo(Disposer);

            var et = m_backButton.gameObject.AddComponent<EventTrigger>();
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
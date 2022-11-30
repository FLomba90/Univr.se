using Abstractions;
using Abstractions.Enums;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Services;
using Settings;
using System.Linq;
using UniRx;
using UnityEngine;
using Utils;
using Zenject;
using static Assets.Scripts.Utils.Utils;

namespace Controllers
{
    public interface IMenuController
    {
        AppNetworkInternalError LastError { get; }
        void StartConnection();
        void BackToOrigin();
        void TeamSelectionHasChanged(int PlayerId, int selectedTeam);
        void LeaveGameWithError(AppNetworkInternalError error);
    }
    public class MenuController : ADisposableMonoBehaviour, IMenuController
    {
        #region resources
        protected override string LOG_TAG => nameof(MenuController);

        #endregion resources

        #region data
        [SerializeField]
        public Transform m_menuContent;

        public AppNetworkInternalError LastError { get; private set; }

        #endregion data

        #region dependency injection
        [Inject]
        readonly AppNetworkSettings m_appNetworkSettings;
        [Inject]
        readonly IMenuViewFactory m_menuViewFactory;
        [Inject]
        readonly INetworkService m_networkService;

        MenuView m_currentActiveView;

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
        void OnConnectionResult((bool, AppNetworkInternalError) result)
        {
            Debug.Log($"{LOG_TAG}.{nameof(OnConnectionResult)}: {result.Item1}");
            if (result.Item1)
            {
                Debug.Log($"{LOG_TAG}.{nameof(OnConnectionResult)}: Done");

                m_networkService.SetLocalPlayerCustomProperty(m_appNetworkSettings.Game.PlayerCustomPropKey_SelectedTeam, -1);
                ChangeView(ViewType.GameSideSelection);
            }
            else
            {
                LastError = result.Item2;
                ChangeView(ViewType.Error);
            }
        }
        void OnNetworkEvent(EventData eventData)
        {
            if (eventData.Code.Equals(m_appNetworkSettings.Events.NetworkEvent_LaunchGame))
            {
                ChangeView(ViewType.Loading);
                if (PhotonNetwork.IsMasterClient)
                {
                    m_networkService.StartMatch();
                }
            }
        }
        void OnDisconnected(AppNetworkInternalError error)
        {
            Debug.Log($"{LOG_TAG}.{nameof(OnDisconnected)}");
            LastError = error;
            if(LastError == null || !error.HasDisconnectionError || error.DisconnectError == DisconnectCause.DisconnectByClientLogic)
            {
                ChangeView(ViewType.ConnectToServer);
                return;
            }
            ChangeView(ViewType.Error);
        }
        #endregion callbacks

        #region abstractions
        public void StartConnection()
        {
            ChangeView(ViewType.Loading);
            m_networkService.StartGame();
        }
        public void BackToOrigin()
        {
            m_networkService.LeaveGame();
        }

        public void TeamSelectionHasChanged(int PlayerId, int selectedTeam)
        {
            if (selectedTeam.Equals(-1) || !PhotonNetwork.IsMasterClient) { return; }

            var sumTeam1Players = 0;
            var sumTeam2Players = 0;
           foreach(var player in PhotonNetwork.PlayerList.ToList())
            {
                if(m_networkService.TryGetPlayerCustomProperty<int>(player.ActorNumber, m_appNetworkSettings.Game.PlayerCustomPropKey_SelectedTeam, out var team))
                {
                    if (team.Equals(-1)) { return; }
                    sumTeam1Players += team.Equals(0) ? 1 : 0;
                    sumTeam2Players += team.Equals(1) ? 1 : 0;
                }
                else
                {
                    return;
                }
            }

            if (PhotonNetwork.CurrentRoom.PlayerCount > 1 && sumTeam1Players > 0 && sumTeam2Players > 0)
            {
                m_networkService.SendEvent((byte)m_appNetworkSettings.Events.NetworkEvent_LaunchGame, null, ReceiverGroup.All, EventCaching.AddToRoomCache);
            }
        }
        public void LeaveGameWithError(AppNetworkInternalError error)
        {
            LastError = error;
            ChangeView(ViewType.Error);
        }
        #endregion abstractions

        #region logic

        void InitUiContent()
        {
            ChangeView(ViewType.ConnectToServer);
        }

        void InitObservables()
        {
            Observable.FromEvent<(bool, AppNetworkInternalError)>
                (h => m_networkService.ConnectionResult += h, h => m_networkService.ConnectionResult -= h).
                Subscribe(OnConnectionResult).AddTo(Disposer);

            Observable.FromEvent<AppNetworkInternalError>
                 (h => m_networkService.Disconnected += h, h => m_networkService.Disconnected -= h).
                Subscribe(OnDisconnected).AddTo(Disposer);

            Observable.FromEvent<EventData>
                 (h => m_networkService.NetworkEvent += h, h => m_networkService.NetworkEvent -= h).
                Subscribe(OnNetworkEvent).AddTo(Disposer);
        }

        async UniTask DisposeCurrentView()
        {
            if (m_currentActiveView == null) return;
            await m_currentActiveView.Dispose();
            if (m_currentActiveView != null)
            {
                Destroy(m_currentActiveView.gameObject);
                await UniTask.WaitForEndOfFrame();
            }
            m_currentActiveView = null;
        }

        async void ChangeView(ViewType type)
        {
            await DisposeCurrentView();
            m_currentActiveView = m_menuViewFactory.GetInstance(type);
            if (m_currentActiveView == null)
            {
                Debug.LogError($"{LOG_TAG}.{nameof(ChangeView)}: missing view type");
                return;
            }
            m_currentActiveView.transform.SetParent(m_menuContent);
        }
        #endregion logic
    }
}
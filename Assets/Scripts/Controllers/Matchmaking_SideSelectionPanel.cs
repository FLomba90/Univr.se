using Abstractions;
using Abstractions.Enums;
using AppExtensions;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Services;
using Settings;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using static Assets.Scripts.Utils.Utils;

namespace Controllers
{
    public class Matchmaking_SideSelectionPanel : MenuView
    {
        #region resources
        public override ViewType ViewType => ViewType.GameSideSelection;
        protected override string LOG_TAG => nameof(Matchmaking_SideSelectionPanel);

        #endregion resources

        #region data
        [Header("Ui bindings")]
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
        Transform m_leftContentList;
        [SerializeField]
        Transform m_centerContentList;
        [SerializeField]
        Transform m_rightContentList;
        [SerializeField]
        GameObject m_confirmButtonContent;
        [SerializeField]
        Button m_confirmButton;
        [SerializeField]
        TMPro.TextMeshProUGUI m_confirmButtonText;
        [SerializeField]
        Image m_confirmButtonFrame;
        [SerializeField]
        UIGradient m_confirmButtonImageGradient;
        [SerializeField]
        UIGradient m_confirmButtonFrameGradient;
        public class Factory : PlaceholderFactory<Matchmaking_SideSelectionPanel> { }

        Dictionary<int, Dictionary<SideSelectionCellPosition, TeamSelectionControllerUiCellController>> m_playerSelections;
        bool m_teamSelectionConfirmed;

        #endregion data

        #region dependency injection
        [Inject]
        readonly AppNetworkSettings m_appNetworkSettings;
        [Inject]
        readonly AppResources m_appResources;
        [Inject]
        readonly IUserInputService m_userInputService;
        [Inject]
        readonly INetworkService m_networkService;
        [Inject]
        readonly TeamSelectionControllerUiCellController.Factory m_TeamSelectionUiCellFactory;
        #endregion dependency injection

        #region monobehaviour callbacks
        protected override void Start()
        {
            base.Start();

            m_playerSelections = new Dictionary<int, Dictionary<SideSelectionCellPosition, TeamSelectionControllerUiCellController>>();
            InitObservables();
            InitUiContent();
        }
        #endregion monobehaviour callbacks

        #region callbacks
        void DidTapOnConfirmButton(Unit obj)
        {
            Debug.Log($"{LOG_TAG}.{nameof(DidTapOnConfirmButton)}");

            m_teamSelectionConfirmed = !m_teamSelectionConfirmed;

            var selectedSide = GetCurrentPlayerSideSelection(PhotonNetwork.LocalPlayer.ActorNumber);

            var confirmedTeam = m_teamSelectionConfirmed ? GetTeamFromSelectedSide(selectedSide) : -1;
            //m_teamSelectionConfirmed = !m_teamSelectionConfirmed && !confirmedTeam.Equals(-1);
           
            m_confirmButtonText.text = m_teamSelectionConfirmed ? m_appResources.Matchmaking.UndoTeamSelectionButtonText : m_appResources.Matchmaking.ConfirmTeamSelectionButtonText;
            m_playerSelections[PhotonNetwork.LocalPlayer.ActorNumber][selectedSide].SetComfirmation(m_teamSelectionConfirmed);

            m_networkService.SetLocalPlayerCustomProperty(m_appNetworkSettings.Game.PlayerCustomPropKey_SelectedTeam, confirmedTeam);

            MenuController.TeamSelectionHasChanged(PhotonNetwork.LocalPlayer.ActorNumber, confirmedTeam);
        }
        void PointerExitOnConnectButton(BaseEventData obj)
        {
            m_confirmButtonFrame.gameObject.SetActive(false);
        }
        void PointerEnterOnConnectButton(BaseEventData obj)
        {
            m_confirmButtonFrame.gameObject.SetActive(true);
        }
        void OnPlayerPropertiesUpdated((int playerId, Hashtable properties) obj)
        {
            if (!m_networkService.TryGetPlayerFromId(obj.playerId, out var player)
                || obj.playerId.Equals(PhotonNetwork.LocalPlayer.ActorNumber))
            {
                return;
            }

            if(m_networkService.TryGetCustomProperty<SideSelectionCellPosition>(obj.properties, m_appNetworkSettings.Game.PlayerCustomPropKey_SelectionSide, out var selectedSide)){
                ChangePlayerSelection(player, selectedSide);
            }
            if (m_networkService.TryGetCustomProperty<int>(obj.properties, m_appNetworkSettings.Game.PlayerCustomPropKey_SelectedTeam, out var selectedTeam)){
                MenuController.TeamSelectionHasChanged(PhotonNetwork.LocalPlayer.ActorNumber, selectedTeam);
            }
        }
        void OnRoomPropertiesUpdated(Hashtable obj)
        {
            //insert here your code
        }
        void OnPlayerLeftRoom(int playerId)
        {
            DisposePlayerSelection(playerId);
            if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                MenuController.LeaveGameWithError(
                    new AppNetworkInternalError(AppNetworkError.AllPlayersLeft, m_appNetworkSettings.GetAppNetworkErrorMessage(AppNetworkError.AllPlayersLeft)));
            }
        }
        void OnPlayerEnteredRoom(int playerId)
        {
            if (!m_networkService.TryGetPlayerFromId(playerId, out var player))
            {
                return;
            }
            if (!m_networkService.TryGetPlayerCustomProperty<SideSelectionCellPosition>(playerId, m_appNetworkSettings.Game.PlayerCustomPropKey_SelectionSide, out var selectedSide))
            {
                ChangePlayerSelection(player, selectedSide);
                return;
            }
            ChangePlayerSelection(player, SideSelectionCellPosition.None);
        }
        void OnKeyboardArrowKeyDown(KeyCode code)
        {
            if(m_teamSelectionConfirmed) { return; }
            var newSide = GetCurrentPlayerSideSelection(PhotonNetwork.LocalPlayer.ActorNumber).GetSideSelectionCellPositionFromUserInput(code);
            ChangePlayerSelection(PhotonNetwork.LocalPlayer, GetCurrentPlayerSideSelection(PhotonNetwork.LocalPlayer.ActorNumber).GetSideSelectionCellPositionFromUserInput(code));
            m_networkService.SetLocalPlayerCustomProperty(m_appNetworkSettings.Game.PlayerCustomPropKey_SelectionSide, newSide);
        }
        #endregion callbacks

        #region logic

        void InitUiContent()
        {
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

            m_confirmButtonText.text = m_teamSelectionConfirmed ? m_appResources.Matchmaking.UndoTeamSelectionButtonText : m_appResources.Matchmaking.ConfirmTeamSelectionButtonText;
            m_confirmButtonImageGradient.m_color1 = ColorsExtensions.CopyColor(m_appResources.Ui.Pink, m_confirmButtonImageGradient.m_color1.a);
            m_confirmButtonImageGradient.m_color2 = ColorsExtensions.CopyColor(m_appResources.Ui.Blue, m_confirmButtonImageGradient.m_color2.a);
            m_confirmButtonFrameGradient.m_color1 = ColorsExtensions.CopyColor(m_appResources.Ui.Pink, m_confirmButtonImageGradient.m_color1.a);
            m_confirmButtonFrameGradient.m_color2 = ColorsExtensions.CopyColor(m_appResources.Ui.Blue, m_confirmButtonImageGradient.m_color2.a);

            m_confirmButtonFrame.gameObject.SetActive(false);

            PopulateSelections();
        }
        void InitObservables()
        {
            m_confirmButton.onClick.AsObservable().Subscribe(DidTapOnConfirmButton).AddTo(Disposer);

            Observable.FromEvent<KeyCode>(h => m_userInputService.OnKeyboardArrowKeyDown += h, h => m_userInputService.OnKeyboardArrowKeyDown -= h).
                Subscribe(OnKeyboardArrowKeyDown).AddTo(Disposer);
            Observable.FromEvent<int>(h => m_networkService.PlayerEnteredRoom += h, h => m_networkService.PlayerEnteredRoom -= h).
                Subscribe(OnPlayerEnteredRoom).AddTo(Disposer);
            Observable.FromEvent<int>(h => m_networkService.PlayerLeftRoom += h, h => m_networkService.PlayerLeftRoom -= h).
                Subscribe(OnPlayerLeftRoom).AddTo(Disposer);
            Observable.FromEvent<Hashtable>(h => m_networkService.RoomPropertiesUpdated += h, h => m_networkService.RoomPropertiesUpdated -= h).
                Subscribe(OnRoomPropertiesUpdated).AddTo(Disposer);
            Observable.FromEvent<(int, Hashtable)>(h => m_networkService.PlayerPropertiesUpdated += h, h => m_networkService.PlayerPropertiesUpdated -= h).
                Subscribe(OnPlayerPropertiesUpdated).AddTo(Disposer);

            var et = m_confirmButton.gameObject.AddComponent<EventTrigger>();
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
        async void PopulateSelections()
        {
            await InitPlayerSelection(PhotonNetwork.LocalPlayer);
            var players = PhotonNetwork.CurrentRoom.Players.Values.ToList();
            players.Remove(PhotonNetwork.LocalPlayer);
            players.ForEach(async player =>
            {
                await InitPlayerSelection(player);
            });
        }

        async UniTask InitPlayerSelection(Player player)
        {
            await InstantiateSelectionsForPlayer(player);
            if (!player.ActorNumber.Equals(PhotonNetwork.LocalPlayer.ActorNumber))
            {
                if (m_networkService.TryGetPlayerCustomProperty<SideSelectionCellPosition>(player.ActorNumber, m_appNetworkSettings.Game.PlayerCustomPropKey_SelectionSide, out var selectionSide))
                {
                    ChangePlayerSelection(player, selectionSide);
                    return;
                }
            }
            ChangePlayerSelection(player, SideSelectionCellPosition.None);
        }
        async UniTask InstantiateSelectionsForPlayer(Player player)
        {
            m_playerSelections[player.ActorNumber] = new Dictionary<SideSelectionCellPosition, TeamSelectionControllerUiCellController>();
            var cell = m_TeamSelectionUiCellFactory.Create(player.NickName, SideSelectionCellPosition.Left);
            cell.transform.SetParent(m_leftContentList);
            m_playerSelections[player.ActorNumber][SideSelectionCellPosition.Left] = cell;
            cell = m_TeamSelectionUiCellFactory.Create(player.NickName, SideSelectionCellPosition.None);
            cell.transform.SetParent(m_centerContentList);
            m_playerSelections[player.ActorNumber][SideSelectionCellPosition.None] = cell;
            cell = m_TeamSelectionUiCellFactory.Create(player.NickName, SideSelectionCellPosition.Right);
            cell.transform.SetParent(m_rightContentList);
            m_playerSelections[player.ActorNumber][SideSelectionCellPosition.Right] = cell;

            await UniTask.WaitForEndOfFrame();
        }
        async void ChangePlayerSelection(Player player, SideSelectionCellPosition selection)
        {
            if (!m_playerSelections.ContainsKey(player.ActorNumber)){
                await InstantiateSelectionsForPlayer(player);
            }
            m_playerSelections[player.ActorNumber][SideSelectionCellPosition.Right].ChangeSelection(selection);
            m_playerSelections[player.ActorNumber][SideSelectionCellPosition.None].ChangeSelection(selection);
            m_playerSelections[player.ActorNumber][SideSelectionCellPosition.Left].ChangeSelection(selection);

            if (player.Equals(PhotonNetwork.LocalPlayer))
            {
                m_confirmButtonContent.SetActive(selection != SideSelectionCellPosition.None);
            }
        }
        async void DisposePlayerSelection(int playerId)
        {
            if (!m_playerSelections.ContainsKey(playerId)){ return; }
            Destroy(m_playerSelections[playerId][SideSelectionCellPosition.Right]);
            Destroy(m_playerSelections[playerId][SideSelectionCellPosition.None]);
            Destroy(m_playerSelections[playerId][SideSelectionCellPosition.Left]);
            m_playerSelections.Remove(playerId);
            await UniTask.WaitForEndOfFrame();
        }
        SideSelectionCellPosition GetCurrentPlayerSideSelection(int playerId)
        {
            SideSelectionCellPosition result = SideSelectionCellPosition.None;
            if (m_playerSelections.ContainsKey(playerId)) {
                m_playerSelections[playerId].ToList().ForEach(tuple =>
                {
                    if (tuple.Value.IsSelected)
                    {
                        result = tuple.Key;
                    }
                });
            }
            return result;
        }
        int GetTeamFromSelectedSide(SideSelectionCellPosition cellPosition)
        {
            switch (cellPosition)
            {
                case SideSelectionCellPosition.Left:
                    return 0;
                case SideSelectionCellPosition.Right:
                    return 1;
            }
            return -1;
        }

        #endregion logic
    }
}
using Abstractions;
using Controllers;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon.Pun;
using Services;
using Settings;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;
using static Assets.Scripts.Utils.Utils;

namespace Managers
{
    public interface INetworkMatchManager
    {
        NetworkPlayerManager NetworkPlayerManager { get; }
        Transform SoccerBallSpawnPoint { get; }
        Transform Team1SpawnPoint { get; }
        Transform Team2SpawnPoint { get; }
        int GetTeam(int playerId);
        void PlayerIsReadyToPlay();
        void NotifyGoal(int winnerTeam);
        void Close();
    }
    public class NetworkMatchManager : ADisposablePunMonoBehaviour, INetworkMatchManager
    {
        #region resources

        protected override string LOG_TAG => nameof(NetworkMatchManager);

        #endregion resources

        #region data
        [SerializeField]
        Transform m_soccerBallSpawnPoint;
        [SerializeField]
        Transform m_team1SpawnPoint;
        [SerializeField]
        Transform m_team2SpawnPoint;
        [SerializeField]
        Transform m_floor;

        public NetworkPlayerManager NetworkPlayerManager { get; private set; }
        public Transform SoccerBallSpawnPoint => m_soccerBallSpawnPoint;
        public Transform Team1SpawnPoint => m_team1SpawnPoint;
        public Transform Team2SpawnPoint => m_team2SpawnPoint;

        MatchBallController m_spawnedBall;
        Dictionary<int, MatchTargetController> m_teamsTargets;
        Dictionary<int, int> m_teamsScore;
        List<Vector3> m_floorVertices;
        List<Vector3> m_floorCorners;
        List<Vector3> m_edgeVectors;

        #endregion data

        #region dependency injection
        [Inject]
        readonly INetworkService m_networkService;
        [Inject]
        readonly AppResources m_appResources;
        [Inject]
        readonly IMatchUiController m_matchUiController;
        [Inject]
        readonly AppNetworkSettings m_appNetworkSettings;
        [Inject]
        readonly NetworkPlayerManager.Factory m_networkPlayerFactory;
        [Inject]
        readonly MatchBallController.Factory m_matchBallFactory;
        [Inject]
        readonly MatchTargetController.Factory MatchTargetFactory;
        [Inject]
        readonly DiContainer m_diContainer;
        #endregion dependency injection

        #region monobehaviour callbacks
        protected override void Start()
        {
            base.Start();

            InitObservables();

            CalculateFloorCornerPoints();
            m_teamsScore = new Dictionary<int, int>()
            {
                { 0, 0 },
                { 1, 0 },
            };
            m_teamsTargets = new Dictionary<int, MatchTargetController>();
            m_matchUiController.UpdateScore(0, 0);

            SpawnLocalPlayerManager();
        }

        private void SpawnLocalPlayerManager()
        {
            var networkPlayerManager = m_networkPlayerFactory.Create();
            networkPlayerManager.transform.position = Vector3.zero;
            networkPlayerManager.PhotonView.ViewID = PhotonNetwork.AllocateViewID(PhotonNetwork.LocalPlayer.ActorNumber);
            object[] data = new object[]
                    {
                   networkPlayerManager.PhotonView.ViewID,
                };
            NetworkPlayerManager = networkPlayerManager;
            m_networkService.SendEvent(m_appNetworkSettings.Events.NetworkEvent_InstantiatePlayerManagerOnOtherClients, data, Photon.Realtime.ReceiverGroup.Others, Photon.Realtime.EventCaching.AddToRoomCache);
        }
        #endregion monobehaviour callbacks

        #region callbacks
        async void OnNetworkEvent(EventData eventData)
        {
            if (eventData.Code.Equals(m_appNetworkSettings.Events.NetworkEvent_InstantiatePlayerManagerOnOtherClients))
            {
                var photonViewId = (int)(eventData.CustomData as object[])[0];
                var networkPlayerManager = m_networkPlayerFactory.Create();
                networkPlayerManager.transform.position = Vector3.zero;
                networkPlayerManager.PhotonView.ViewID = photonViewId;
            }
            if (eventData.Code.Equals(m_appNetworkSettings.Events.NetworkEvent_InstantiateGameItems))
            {
                m_spawnedBall = m_matchBallFactory.Create();
                m_spawnedBall.transform.position = m_soccerBallSpawnPoint.position + (0.5f) * m_soccerBallSpawnPoint.up;
                m_spawnedBall.PhotonView.ViewID = (int)(eventData.CustomData as object[])[0];
                m_spawnedBall.PhotonView.OwnershipTransfer = OwnershipOption.Takeover;

                var teamsTarget = MatchTargetFactory.Create(0);
                teamsTarget.transform.position = (Vector3)(eventData.CustomData as object[])[2];
                teamsTarget.PhotonView.ViewID = (int)(eventData.CustomData as object[])[1];
                m_teamsTargets[0] = teamsTarget;

                teamsTarget = MatchTargetFactory.Create(1);
                teamsTarget.transform.position = (Vector3)(eventData.CustomData as object[])[4];
                teamsTarget.PhotonView.ViewID = (int)(eventData.CustomData as object[])[3];
                m_teamsTargets[1] = teamsTarget;
            }
            if (eventData.Code.Equals(m_appNetworkSettings.Events.NetworkEvent_UpdateScore))
            {
                var winnerTeam = (int)(eventData.CustomData as object[])[0];
                m_teamsScore[winnerTeam] += 1;
                m_matchUiController.UpdateScore(m_teamsScore[0], m_teamsScore[1]);

                Destroy(m_spawnedBall.gameObject);
                m_spawnedBall = null;
                await UniTask.WaitForEndOfFrame();

                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.Destroy(m_teamsTargets[0].PhotonView);
                    PhotonNetwork.Destroy(m_teamsTargets[1].PhotonView);
                    if (m_teamsScore[winnerTeam].Equals(m_appNetworkSettings.Game.MaxScore))
                    {
                        int GetWinner()
                        {
                           return m_teamsScore[0] > m_teamsScore[1] ? 0 : 1;
                        }
                        m_networkService.SendEvent(m_appNetworkSettings.Events.NetworkEvent_MatchCompleted, new object[] { GetWinner() }, Photon.Realtime.ReceiverGroup.All, Photon.Realtime.EventCaching.AddToRoomCache);
                        return;
                    }
                    m_networkService.SendEvent(m_appNetworkSettings.Events.NetworkEvent_ResetGame, null, Photon.Realtime.ReceiverGroup.All, Photon.Realtime.EventCaching.AddToRoomCache);
                }
            }
            if (eventData.Code.Equals(m_appNetworkSettings.Events.NetworkEvent_ResetGame))
            {
                m_spawnedBall = null;
                m_networkService.SetLocalPlayerCustomProperty(m_appNetworkSettings.Game.PlayerCustomPropKey_CanPlay, false);
                NetworkPlayerManager.Reset();
            }
            if (eventData.Code.Equals(m_appNetworkSettings.Events.NetworkEvent_MatchCompleted))
            {
                m_matchUiController.GameCompleted((int)(eventData.CustomData as object[])[0]);
            }
        }
        void OnPlayerPropertiesUpdated((int, ExitGames.Client.Photon.Hashtable) obj)
        {
            CheckForAllPlayersReady();
        }
        void OnDisconnected(AppNetworkInternalError obj)
        {
            //todo;
        }
        #endregion callbacks

        #region logic

        public void Close()
        {
            Application.Quit();
        }
        public void PlayerIsReadyToPlay()
        {
            m_networkService.SetLocalPlayerCustomProperty(m_appNetworkSettings.Game.PlayerCustomPropKey_CanPlay, true);
            CheckForAllPlayersReady();
        }
        public int GetTeam(int playerId)
        {
            if (m_networkService.TryGetPlayerCustomProperty<int>(playerId, m_appNetworkSettings.Game.PlayerCustomPropKey_SelectedTeam, out var team))
            {
                return team;
            }
            return -1;
        }

        public void NotifyGoal(int winnerTeam)
        {
            if (!PhotonNetwork.IsMasterClient) { return; }

            m_networkService.SendEvent(m_appNetworkSettings.Events.NetworkEvent_UpdateScore, new object[] { winnerTeam }, Photon.Realtime.ReceiverGroup.All, Photon.Realtime.EventCaching.AddToRoomCache);
        }
        void InitObservables()
        {
            Observable.FromEvent<AppNetworkInternalError>
                 (h => m_networkService.Disconnected += h, h => m_networkService.Disconnected -= h).
                Subscribe(OnDisconnected).AddTo(Disposer);

            Observable.FromEvent<(int, ExitGames.Client.Photon.Hashtable)>
                 (h => m_networkService.PlayerPropertiesUpdated += h, h => m_networkService.PlayerPropertiesUpdated -= h).
                Subscribe(OnPlayerPropertiesUpdated).AddTo(Disposer);

            Observable.FromEvent<EventData>
           (h => m_networkService.NetworkEvent += h, h => m_networkService.NetworkEvent -= h).
          Subscribe(OnNetworkEvent).AddTo(Disposer);
        }

        private void CheckForAllPlayersReady()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (m_spawnedBall != null) { return; }

                foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
                {
                    if (m_networkService.TryGetPlayerCustomProperty<bool>(player.ActorNumber, m_appNetworkSettings.Game.PlayerCustomPropKey_CanPlay, out var canPlay))
                    {
                        if (!canPlay) { return; }
                    }
                    else
                    {
                        return;
                    }
                }

                m_spawnedBall = m_matchBallFactory.Create();
                m_spawnedBall.transform.position = m_soccerBallSpawnPoint.position + (0.5f) * m_soccerBallSpawnPoint.up;
                PhotonNetwork.AllocateRoomViewID(m_spawnedBall.PhotonView);
                m_spawnedBall.PhotonView.OwnershipTransfer = OwnershipOption.Takeover;

                var teamsTarget = MatchTargetFactory.Create(0);
                teamsTarget.transform.position = CalculateFloorRandomPoint();
                PhotonNetwork.AllocateRoomViewID(teamsTarget.PhotonView);
                teamsTarget.PhotonView.OwnershipTransfer = OwnershipOption.Fixed;
                m_teamsTargets[0] = teamsTarget;

                teamsTarget = MatchTargetFactory.Create(1);
                teamsTarget.transform.position = CalculateFloorRandomPoint();
                PhotonNetwork.AllocateRoomViewID(teamsTarget.PhotonView);
                teamsTarget.PhotonView.OwnershipTransfer = OwnershipOption.Fixed;
                m_teamsTargets[1] = teamsTarget;

                object[] data = new object[]
                     {
                    m_spawnedBall.PhotonView.ViewID,
                    m_teamsTargets[0].PhotonView.ViewID,
                    m_teamsTargets[0].transform.position,
                   m_teamsTargets[1].PhotonView.ViewID,
                    m_teamsTargets[1].transform.position,
                 };
                m_networkService.SendEvent(m_appNetworkSettings.Events.NetworkEvent_InstantiateGameItems, data, Photon.Realtime.ReceiverGroup.Others, Photon.Realtime.EventCaching.AddToRoomCache);
            }
        }

        private void OnApplicationQuit()
        {
            m_networkService.LeaveGame();
        }

        void CalculateFloorEdgeVectors(int VectorCorner)
        {
            m_edgeVectors.Clear();

            m_edgeVectors.Add(m_floorCorners[3] - m_floorCorners[VectorCorner]);
            m_edgeVectors.Add(m_floorCorners[1] - m_floorCorners[VectorCorner]);
        }
        Vector3 CalculateFloorRandomPoint()
        {
            int randomCornerIdx = UnityEngine.Random.Range(0, 2) == 0 ? 0 : 2;
            CalculateFloorEdgeVectors(randomCornerIdx);

            float u = UnityEngine.Random.Range(0.0f, 1.0f);
            float v = UnityEngine.Random.Range(0.0f, 1.0f);

            if (v + u > 1)
            {
                v = 1 - v;
                u = 1 - u;
            }

            return m_floorCorners[randomCornerIdx] + u * m_edgeVectors[0] + v * m_edgeVectors[1];
        }
        void CalculateFloorCornerPoints()
        {
            m_floorCorners = new List<Vector3>();
            m_edgeVectors = new List<Vector3>();

            m_floorVertices = new List<Vector3>(m_floor.GetComponent<MeshFilter>().sharedMesh.vertices);
            m_floorCorners.Clear(); 
            m_floorCorners.Add(m_floor.TransformPoint(m_floorVertices[0]));
            m_floorCorners.Add(m_floor.TransformPoint(m_floorVertices[10]));
            m_floorCorners.Add(m_floor.TransformPoint(m_floorVertices[110]));
            m_floorCorners.Add(m_floor.TransformPoint(m_floorVertices[120]));
        }
    }

    #endregion logic
}
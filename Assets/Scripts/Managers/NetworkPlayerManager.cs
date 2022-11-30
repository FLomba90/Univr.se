using Abstractions;
using Controllers;
using Photon.Pun;
using Services;
using Settings;
using UnityEngine;
using Zenject;

namespace Managers
{
    public class NetworkPlayerManager : ADisposablePunMonoBehaviour
    {
        #region resources

        protected override string LOG_TAG => nameof(NetworkPlayerManager);

        #endregion resources

        #region data
        [SerializeField]
        PhotonView m_photonView;

        public PhotonView PhotonView => m_photonView;
        public AvatarController CurrentAvatar { get; private set; }
        public class Factory : PlaceholderFactory<NetworkPlayerManager> { }
        #endregion data

        #region dependency injection
        [Inject]
        readonly INetworkMatchManager m_networkMatchManager;
        [Inject]
        readonly INetworkService m_networkService;
        [Inject]
        readonly AppResources m_appResources;
        [Inject]
        readonly AppNetworkSettings m_appNetworkSettings;
        #endregion dependency injection

        #region monobehaviour callbacks
        protected override void Start()
        {
            base.Start();
            Init();
        }
        #endregion monobehaviour callbacks

        #region callbacks
        #endregion callbacks

        #region logic

        public void Reset()
        {
            if (m_photonView.IsMine)
            {
                if (m_networkService.TryGetPlayerCustomProperty<int>(PhotonNetwork.LocalPlayer.ActorNumber, m_appNetworkSettings.Game.PlayerCustomPropKey_SelectedTeam, out var team))
                {
                    CurrentAvatar.MoveTo(team.Equals(0) ? m_networkMatchManager.Team1SpawnPoint.position : m_networkMatchManager.Team2SpawnPoint.position);
                    m_networkMatchManager.PlayerIsReadyToPlay();
                }
            }
        }
        void Init()
        {
            if (m_photonView.IsMine)
            {
                if (m_networkService.TryGetPlayerCustomProperty<int>(PhotonNetwork.LocalPlayer.ActorNumber, m_appNetworkSettings.Game.PlayerCustomPropKey_SelectedTeam, out var team))
                {
                    if (team.Equals(-1)) { return; }
                    CurrentAvatar = PhotonNetwork.Instantiate(
                    team.Equals(0) ? m_appNetworkSettings.Game.NetworkAvatar1Path : m_appNetworkSettings.Game.NetworkAvatar2Path,
                   team.Equals(0) ? m_networkMatchManager.Team1SpawnPoint.position : m_networkMatchManager.Team2SpawnPoint.position,
                   Quaternion.identity).GetComponent<AvatarController>();
                    m_networkMatchManager.PlayerIsReadyToPlay();
                }
            }
        }
        #endregion logic
    }
}
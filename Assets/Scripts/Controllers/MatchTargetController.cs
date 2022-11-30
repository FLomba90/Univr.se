using Managers;
using Photon.Pun;
using UnityEngine;
using Zenject;

namespace Controllers
{
    public class MatchTargetController : MonoBehaviour
    {
        #region resources

        const string LOG_TAG = nameof(MatchTargetController);

        #endregion resources

        #region data
        [SerializeField]
        GameObject m_team1Skin;
        [SerializeField]
        GameObject m_team2Skin;
        [SerializeField]
        PhotonView m_photonView;

        public PhotonView PhotonView => m_photonView;
        int m_ownerTeam;
        public class Factory : PlaceholderFactory<int, MatchTargetController> { }
        #endregion data

        #region dependency injection
        [Inject]
        INetworkMatchManager m_networkManager;
        [Inject]
        public void Construct(int ownerTeam)
        {
            m_ownerTeam = ownerTeam;
        }
        #endregion dependency injection

        #region monobehaviour callbacks
        void Start()
        {
            InitializeContent();
        }

        #endregion monobehaviour callbacks

        #region callbacks

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<MatchBallController>() != null)
            {
                m_networkManager.NotifyGoal(m_ownerTeam == 0 ? 1 : 0);
            }
        }
        #endregion callbacks

        #region logic
        void InitializeContent()
        {
            m_team1Skin.SetActive(m_ownerTeam == 0);
            m_team2Skin.SetActive(m_ownerTeam == 1);
        }
        #endregion logic
    }
}
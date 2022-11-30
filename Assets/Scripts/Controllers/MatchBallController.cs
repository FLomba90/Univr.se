using Photon.Pun;
using Services;
using Settings;
using UnityEngine;
using Zenject;

namespace Controllers
{
    public class MatchBallController : MonoBehaviour
    {
        #region resources

        const string LOG_TAG = nameof(MatchBallController);

        #endregion resources

        #region data
        public class Factory : PlaceholderFactory<MatchBallController> { }

        [SerializeField]
        PhotonView m_photonView;
        [SerializeField]
        Rigidbody m_rigidBody;

        public PhotonView PhotonView => m_photonView;

        #endregion data

        #region dependency injection
        [Inject]
        readonly INetworkService m_networkService;
        [Inject]
        readonly AppResources m_appResources;
        [Inject]
        readonly AppNetworkSettings m_appNetworkSettings;
        #endregion dependency injection

        #region monobehaviour callbacks
        #endregion monobehaviour callbacks

        #region callbacks
        #endregion callbacks

        #region logic

        private void OnTriggerEnter(Collider other)
        {

        }
        private void OnCollisionEnter(Collision collision)
        {
            var go = collision.gameObject;
            var avatar = go.GetComponent<AvatarController>();

            if (avatar != null)
            {
                m_photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
            }
        }

        #endregion logic
    }
}
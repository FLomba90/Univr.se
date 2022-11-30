using Abstractions;
using Photon.Pun;
using UnityEngine;

namespace Controllers
{
    public class AvatarController : ADisposablePunMonoBehaviour
    {
        #region resources

        protected override string LOG_TAG => nameof(AvatarController);

        #endregion resources

        #region data
        [SerializeField]
        CapsuleCollider m_capsuleCollider;
        [SerializeField]
        CapsuleCollider m_ballCollider;
        [SerializeField]
        Rigidbody m_rigidbody;
        [SerializeField]
        float m_speed = 0.1f;

        public Rigidbody RB => m_rigidbody;
        PhotonView m_photonView;
        #endregion data

        #region dependency injection
        #endregion dependency injection

        #region monobehaviour callbacks

        protected override void Start()
        {
            base.Start();
            Setup();
        }

        private void Update()
        {
            CheckForMovement();
        }

        #endregion monobehaviour callbacks

        #region callbacks
        #endregion callbacks

        #region logic

        public void MoveTo(Vector3 pos)
        {
            m_rigidbody.isKinematic = true;
            transform.position = pos;
            transform.rotation = Quaternion.identity;
            m_rigidbody.isKinematic = !m_photonView.IsMine;
        }
        void Setup()
        {
            m_photonView = GetComponent<PhotonView>();
            m_capsuleCollider.enabled = m_photonView.IsMine;
            m_rigidbody.isKinematic = !m_photonView.IsMine;
            m_ballCollider.enabled = false;
        }

        void CheckForMovement()
        {
            if (!m_photonView.IsMine) { return; }

            if (Input.GetKey(KeyCode.A))
                m_rigidbody.AddForce(Vector3.left, ForceMode.Impulse);
            if (Input.GetKey(KeyCode.D))
                m_rigidbody.AddForce(Vector3.right, ForceMode.Impulse);
            if (Input.GetKey(KeyCode.W))
                m_rigidbody.AddForce(Vector3.forward, ForceMode.Impulse);
            if (Input.GetKey(KeyCode.S))
                m_rigidbody.AddForce(Vector3.back, ForceMode.Impulse);

            //if (Input.GetKey(KeyCode.D))
            //{
            //    transform.position += Vector3.right * m_speed * Time.deltaTime;
            //}
            //if (Input.GetKey(KeyCode.A))
            //{
            //    transform.position += Vector3.left * m_speed * Time.deltaTime;
            //}
            //if (Input.GetKey(KeyCode.W))
            //{
            //    transform.position += Vector3.forward * m_speed * Time.deltaTime;
            //}
            //if (Input.GetKey(KeyCode.S))
            //{
            //    transform.position += Vector3.back * m_speed * Time.deltaTime;
            //}
        }

        #endregion logic
    }
}
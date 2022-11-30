using UniRx;
using UnityEngine;

namespace Abstractions
{
    public abstract class ADisposableMonoBehaviour : MonoBehaviour
    {
        #region resources

        protected abstract string LOG_TAG { get; }

        #endregion resources

        #region data

        protected CompositeDisposable Disposer { get;private set; }

        #endregion data

        #region dependency injection
        #endregion data

        #region monobehaviour callbacks

        protected virtual void Start()
        {
            if(Disposer == null)
            {
                Disposer = new CompositeDisposable();
            }
        }

        protected virtual void OnDestroy()
        {
            if (Disposer != null)
            {
                Disposer.Dispose();
            }
        }
        #endregion monobehaviour callbacks

        #region callbacks
        #endregion callbacks

        #region logic
        #endregion logic
    }
}
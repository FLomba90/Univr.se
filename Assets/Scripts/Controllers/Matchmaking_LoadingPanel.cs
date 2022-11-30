using Abstractions;
using Abstractions.Enums;
using AppExtensions;
using Settings;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;

namespace Controllers
{
    public class Matchmaking_LoadingPanel : MenuView
    {
        #region resources
        public override ViewType ViewType => ViewType.Loading;
        protected override string LOG_TAG => nameof(Matchmaking_LoadingPanel);

        #endregion resources

        #region data
        [Header("Ui bindings")]
        [SerializeField]
        Image m_spinningImage;
        [SerializeField]
        UIGradient m_spinningGradient;

        IndeterminateSpinnerRotator m_spinningRotator;

        public class Factory : PlaceholderFactory<Matchmaking_LoadingPanel> { }

        #endregion data

        #region dependency injection
        [Inject]
        readonly AppResources m_appResources;

        #endregion dependency injection

        #region monobehaviour callbacks

        protected override void Start()
        {
            base.Start();
            InitUiContent();
        }

        private void Update()
        {
            m_spinningRotator?.UpdateIndeterminateSpinner();
        }
        #endregion monobehaviour callbacks

        #region callbacks

        #endregion callbacks

        #region logic

        void InitUiContent()
        {
            m_spinningRotator = new IndeterminateSpinnerRotator(m_spinningImage.transform, 8);

            m_spinningGradient.m_color1 = ColorsExtensions.CopyColor(m_appResources.Ui.Pink, m_spinningGradient.m_color1.a);
            m_spinningGradient.m_color2 = ColorsExtensions.CopyColor(m_appResources.Ui.Pink, m_spinningGradient.m_color2.a);
        }
        #endregion logic
    }
}
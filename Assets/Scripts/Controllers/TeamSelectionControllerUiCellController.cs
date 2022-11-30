using Abstractions.Enums;
using AppExtensions;
using Settings;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Controllers
{
    public class TeamSelectionControllerUiCellController : MonoBehaviour
    {
        #region resources

        protected readonly string LOG_TAG = nameof(TeamSelectionControllerUiCellController);

        #endregion resources

        #region data
        [Header("Ui bindings")]
        [SerializeField]
        TMPro.TextMeshProUGUI m_playerName;
        [SerializeField]
        Image m_controllerIconPlaceholder;
        [SerializeField]
        Image m_controllerIcon;
        [SerializeField]
        Outline m_controllerIconOutline;
        [SerializeField]
        Image m_leftArrowIcon;
        [SerializeField]
        Image m_rightArrowIcon;
        [SerializeField]
        UIGradient m_leftArrowImageGradient;
        [SerializeField]
        UIGradient m_rightArrowImageGradient;
        public class Factory : PlaceholderFactory<string, SideSelectionCellPosition, TeamSelectionControllerUiCellController> { }

        string m_playerN;
        SideSelectionCellPosition m_cellPosition;
        public bool IsSelected => m_isSelected;
        bool m_isSelected;
        bool m_isComfirmed;

        #endregion data

        #region dependency injection
        [Inject]
        readonly AppResources m_appResources;

        [Inject]
        public void Construct(string playerName, SideSelectionCellPosition cellPosition)
        {
            m_playerN = playerName;
            m_cellPosition = cellPosition;
        }

        #endregion dependency injection

        #region monobehaviour callbacks

        void Start()
        {
            InitUiContent();
        }
        #endregion monobehaviour callbacks

        #region callbacks
        #endregion callbacks

        #region logic

        public void SetComfirmation(bool isComfirmed)
        {
            m_isComfirmed = isComfirmed;
            SetComfirmed();
        }
        public void ChangeSelection(SideSelectionCellPosition newActivePosition)
        {
            m_isSelected = newActivePosition.Equals(m_cellPosition);
            SetSelection();
        }
        void InitUiContent()
        {
            m_playerName.text = m_playerN;
            m_controllerIconPlaceholder.sprite = m_appResources.Ui.ControllerPlaceholderIcon;
            m_controllerIcon.sprite = m_appResources.Ui.ControllerIcon;
            m_controllerIconOutline.enabled = false;
            m_leftArrowIcon.sprite = m_appResources.Ui.LeftArrowIcon;
            m_rightArrowIcon.sprite = m_appResources.Ui.RightArrowIcon;

            m_leftArrowImageGradient.m_color1 = ColorsExtensions.CopyColor(m_appResources.Ui.Pink, m_leftArrowImageGradient.m_color1.a);
            m_leftArrowImageGradient.m_color2 = ColorsExtensions.CopyColor(m_appResources.Ui.Blue, m_leftArrowImageGradient.m_color2.a);

            m_rightArrowImageGradient.m_color1 = ColorsExtensions.CopyColor(m_appResources.Ui.Pink, m_rightArrowImageGradient.m_color1.a);
            m_rightArrowImageGradient.m_color2 = ColorsExtensions.CopyColor(m_appResources.Ui.Blue, m_rightArrowImageGradient.m_color2.a);
            GetComponent<RectTransform>().localScale = Vector3.one;
        }

        void SetSelection()
        {
            m_playerName.gameObject.SetActive(m_isSelected);
            if (!m_isSelected)
            {
                m_leftArrowIcon.gameObject.SetActive(false);
                m_rightArrowIcon.gameObject.SetActive(false);
                m_controllerIcon.gameObject.SetActive(false);
                m_controllerIconPlaceholder.gameObject.SetActive(true);
                return;
            }
            m_leftArrowIcon.gameObject.SetActive(m_cellPosition == SideSelectionCellPosition.Right);
            m_rightArrowIcon.gameObject.SetActive(m_cellPosition == SideSelectionCellPosition.Left);
            m_controllerIcon.gameObject.SetActive(true);
            m_controllerIconPlaceholder.gameObject.SetActive(false);
        }

        void SetComfirmed()
        {
            m_controllerIconOutline.enabled = m_isComfirmed;
        }

        #endregion logic
    }
}
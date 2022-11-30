using Abstractions;
using Abstractions.Enums;
using Controllers;
using Zenject;

namespace Utils
{
    public interface IMenuViewFactory
    {
        MenuView GetInstance(ViewType type);
    }
    public class MenuViewFactory: IMenuViewFactory
    {
        #region dependency injection
        [Inject]
        Matchmaking_ConnectPanel.Factory m_connectPanelFactory;
        [Inject]
        Matchmaking_ErrorPanel.Factory m_errorPanelFactory;
        [Inject]
        Matchmaking_LoadingPanel.Factory m_loadingPanelFactory;
        [Inject]
        Matchmaking_SideSelectionPanel.Factory m_sideSelectionPanelFactory;
        #endregion dependency injection

        #region logic

        public MenuView GetInstance(ViewType type)
        {
            switch (type)
            {
                case ViewType.ConnectToServer:
                    return m_connectPanelFactory.Create();
                case ViewType.Loading:
                    return m_loadingPanelFactory.Create();
                case ViewType.Error:
                    return m_errorPanelFactory.Create();
                case ViewType.GameSideSelection:
                    return m_sideSelectionPanelFactory.Create();
            }
            return null;
        }
        #endregion logic
    }
}
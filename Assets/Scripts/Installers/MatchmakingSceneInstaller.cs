using Abstractions.Enums;
using Controllers;
using Services;
using UnityEngine;
using Utils;
using Zenject;

namespace Installers
{
    public class MatchmakingSceneInstaller : MonoInstaller<ProjectInstaller>
    {
        #region data

        [SerializeField]
        GameObject MenuController;
        [SerializeField]
        GameObject LoadingPanel;
        [SerializeField]
        GameObject ConnectPanel;   
        [SerializeField]
        GameObject ErrorPanel;  
        [SerializeField]
        GameObject SelectionSidePanel;
        [SerializeField]
        GameObject SelectionSideCell;

        #endregion data
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<MenuViewFactory>().AsSingle().NonLazy();
            Container.BindInterfacesTo<MenuController>().FromComponentOn(MenuController).AsSingle().NonLazy();
            Container.BindInterfacesTo<NetworkService>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesTo<UserInputService>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindFactory<Matchmaking_LoadingPanel, Matchmaking_LoadingPanel.Factory>().FromComponentInNewPrefab(LoadingPanel);
            Container.BindFactory<Matchmaking_ConnectPanel, Matchmaking_ConnectPanel.Factory>().FromComponentInNewPrefab(ConnectPanel);
            Container.BindFactory<Matchmaking_ErrorPanel, Matchmaking_ErrorPanel.Factory>().FromComponentInNewPrefab(ErrorPanel);
            Container.BindFactory<Matchmaking_SideSelectionPanel, Matchmaking_SideSelectionPanel.Factory>().FromComponentInNewPrefab(SelectionSidePanel);
            Container.BindFactory<string, SideSelectionCellPosition, TeamSelectionControllerUiCellController, TeamSelectionControllerUiCellController.Factory>().FromComponentInNewPrefab(SelectionSideCell);
        }
    }
}

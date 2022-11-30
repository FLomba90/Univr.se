using Controllers;
using Managers;
using Services;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class MatchSceneInstaller : MonoInstaller<MatchSceneInstaller>
    {
        #region data

        [SerializeField]
        GameObject NetworkPlayerPrefab;
        [SerializeField]
        GameObject SoccerBallPrefab;
        [SerializeField]
        GameObject MatchTargetPrefab;

        #endregion data
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<UserInputService>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesTo<NetworkService>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindFactory<NetworkPlayerManager, NetworkPlayerManager.Factory>().FromComponentInNewPrefab(NetworkPlayerPrefab);
            Container.BindFactory<MatchBallController, MatchBallController.Factory>().FromComponentInNewPrefab(SoccerBallPrefab);
            Container.BindFactory<int, MatchTargetController, MatchTargetController.Factory>().FromComponentInNewPrefab(MatchTargetPrefab);
        }
    }
}

using Settings;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class ProjectInstaller : MonoInstaller<ProjectInstaller>
    {
        #region data
        [SerializeField] AppResources m_appResources;
        [SerializeField] AppNetworkSettings m_appNetworkSettings;
        #endregion data
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<AppResources>().FromInstance(m_appResources).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<AppNetworkSettings>().FromInstance(m_appNetworkSettings).AsSingle().NonLazy();
        }
    }
}

using Abstractions.Enums;
using Controllers;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Abstractions
{
    public abstract class MenuView : ADisposableMonoBehaviour
    {
        public abstract ViewType ViewType { get;}

        [Inject]
        protected readonly IMenuController MenuController;

        protected override void Start()
        {
            base.Start();
            GetComponent<RectTransform>().localScale = Vector3.one;
        }

        public virtual async UniTask Dispose()
        {
            await UniTask.Yield();
        }
    }
}

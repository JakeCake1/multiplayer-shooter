using System;
using Shooter.Application;
using Shooter.Presentation;
using UnityEngine;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Shooter.Bootstrap.Client
{
    public sealed class ClientFlowPresentation : IStartable, IDisposable
    {
        private readonly IClientFlowController _controller;
        private ClientFlowView _view;

        public ClientFlowPresentation(IClientFlowController controller)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        public void Start()
        {
            var viewObject = new GameObject("Client Flow UI");
            Object.DontDestroyOnLoad(viewObject);
            _view = viewObject.AddComponent<ClientFlowView>();
            _view.Initialize(_controller);
        }

        public void Dispose()
        {
            if (_view != null)
            {
                Object.Destroy(_view.gameObject);
                _view = null;
            }
        }
    }
}

using System;
using Shooter.Application;
using Shooter.Presentation.Ui;
using UnityEngine;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class ClientFlowUiPresenter : IDisposable
    {
        private readonly IClientFlowController _flowController;
        private readonly ClientFlowScreenFactory _screenFactory;
        private readonly UiController _uiController;
        private bool _started;
        private bool _disposed;

        public ClientFlowUiPresenter(IClientFlowController flowController, ClientFlowScreenFactory screenFactory, UiController uiController)
        {
            _flowController = flowController ?? throw new ArgumentNullException(nameof(flowController));
            _screenFactory = screenFactory ?? throw new ArgumentNullException(nameof(screenFactory));
            _uiController = uiController ?? throw new ArgumentNullException(nameof(uiController));
        }

        public void Start()
        {
            if (_started)
            {
                return;
            }

            _started = true;
            _flowController.Changed += HandleFlowChanged;
            HandleFlowChanged(_flowController.Current);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _flowController.Changed -= HandleFlowChanged;
            _uiController.Dispose();
        }

        private async void HandleFlowChanged(ClientFlowSnapshot snapshot)
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                var request = _screenFactory.Create(snapshot);
                if (request == null)
                {
                    _uiController.Hide();
                    return;
                }

                await _uiController.ShowAsync(request);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}

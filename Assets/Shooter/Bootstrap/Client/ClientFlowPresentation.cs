using System;
using Shooter.Presentation.ClientFlow;
using VContainer.Unity;

namespace Shooter.Bootstrap.Client
{
    public sealed class ClientFlowPresentation : IStartable, IDisposable
    {
        private readonly ClientFlowUiPresenter _presenter;

        public ClientFlowPresentation(ClientFlowUiPresenter presenter)
        {
            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
        }

        public void Start()
        {
            _presenter.Start();
        }

        public void Dispose()
        {
            _presenter.Dispose();
        }
    }
}

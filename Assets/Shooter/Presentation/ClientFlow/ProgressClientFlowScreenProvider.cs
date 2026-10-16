using System;
using Shooter.Application;
using Shooter.Presentation.Ui;

namespace Shooter.Presentation.ClientFlow
{
    public abstract class ProgressClientFlowScreenProvider : IClientFlowScreenProvider
    {
        public abstract ClientFlowState State { get; }

        protected abstract string Status { get; }

        protected virtual Action Cancel => null;

        public UiScreenRequest Create(ClientFlowSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            return new UiScreenRequest(ClientUiScreenAddresses.Connecting, new ConnectingViewModel(Status, Cancel));
        }
    }
}

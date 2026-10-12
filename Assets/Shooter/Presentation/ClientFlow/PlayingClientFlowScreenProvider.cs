using System;
using Shooter.Application;
using Shooter.Presentation.Ui;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class PlayingClientFlowScreenProvider : IClientFlowScreenProvider
    {
        public ClientFlowState State => ClientFlowState.Playing;

        public UiScreenRequest Create(ClientFlowSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            return null;
        }
    }
}

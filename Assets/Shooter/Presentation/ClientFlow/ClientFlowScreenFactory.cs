using System;
using System.Collections.Generic;
using Shooter.Application;
using Shooter.Presentation.Ui;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class ClientFlowScreenFactory
    {
        private readonly Dictionary<ClientFlowState, IClientFlowScreenProvider> _providers = new Dictionary<ClientFlowState, IClientFlowScreenProvider>();

        public ClientFlowScreenFactory(IEnumerable<IClientFlowScreenProvider> providers)
        {
            RegisterProviders(providers ?? throw new ArgumentNullException(nameof(providers)));
        }

        public UiScreenRequest Create(ClientFlowSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            return GetProvider(snapshot.State).Create(snapshot);
        }

        private void RegisterProviders(IEnumerable<IClientFlowScreenProvider> providers)
        {
            foreach (var provider in providers)
            {
                RegisterProvider(provider);
            }
        }

        private void RegisterProvider(IClientFlowScreenProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentException("The client flow screen provider collection contains null.", nameof(provider));
            }

            if (!_providers.TryAdd(provider.State, provider))
            {
                throw new InvalidOperationException($"More than one client flow screen provider is registered for {provider.State}.");
            }
        }

        private IClientFlowScreenProvider GetProvider(ClientFlowState state)
        {
            return _providers.TryGetValue(state, out var provider) ? provider : throw new InvalidOperationException($"No client flow screen provider is registered for {state}.");
        }
    }
}

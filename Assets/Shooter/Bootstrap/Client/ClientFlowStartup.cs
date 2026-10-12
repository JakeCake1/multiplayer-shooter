using System;
using Shooter.Application;
using UnityEngine;
using VContainer.Unity;

namespace Shooter.Bootstrap.Client
{
    public sealed class ClientFlowStartup : IStartable
    {
        private readonly IClientFlowController _flowController;

        public ClientFlowStartup(IClientFlowController flowController)
        {
            _flowController = flowController ?? throw new ArgumentNullException(nameof(flowController));
        }

        public async void Start()
        {
            try
            {
                await _flowController.InitializeAsync();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}

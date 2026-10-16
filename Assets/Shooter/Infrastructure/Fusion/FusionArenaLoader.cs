using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionArenaLoader : IDisposable
    {
        private AsyncOperationHandle<GameObject> _instanceHandle;
        private Task<GameObject> _loadTask;

        public Task<GameObject> LoadAsync()
        {
            _loadTask ??= InstantiateAsync();
            return _loadTask;
        }

        public void Dispose()
        {
            if (_instanceHandle.IsValid())
            {
                Addressables.ReleaseInstance(_instanceHandle);
            }

            _instanceHandle = default;
            _loadTask = null;
        }

        private async Task<GameObject> InstantiateAsync()
        {
            try
            {
                _instanceHandle = Addressables.InstantiateAsync(FusionContentAssetAddresses.ArenaPrefab);
                var arena = await _instanceHandle.Task;
                UnityEngine.Object.DontDestroyOnLoad(arena);
                return arena;
            }
            catch
            {
                Dispose();
                throw;
            }
        }
    }
}

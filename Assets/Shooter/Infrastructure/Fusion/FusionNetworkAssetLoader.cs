using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Shooter.Infrastructure.Fusion
{
    public sealed class FusionNetworkAssetLoader : IDisposable
    {
        private AsyncOperationHandle<GameObject> _playerHandle;
        private AsyncOperationHandle<GameObject> _matchStateHandle;
        private Task<FusionNetworkAssets> _loadTask;

        public FusionNetworkAssets Assets { get; private set; }

        public Task<FusionNetworkAssets> LoadAsync()
        {
            _loadTask ??= LoadAssetsAsync();
            return _loadTask;
        }

        public void Dispose()
        {
            ReleaseHandle(_playerHandle);
            ReleaseHandle(_matchStateHandle);
            Assets = null;
            _loadTask = null;
        }

        private async Task<FusionNetworkAssets> LoadAssetsAsync()
        {
            try
            {
                _playerHandle = Addressables.LoadAssetAsync<GameObject>(FusionNetworkAssetAddresses.PlayerPrefab);
                _matchStateHandle = Addressables.LoadAssetAsync<GameObject>(FusionNetworkAssetAddresses.MatchStatePrefab);
                await Task.WhenAll(_playerHandle.Task, _matchStateHandle.Task);
                Assets = new FusionNetworkAssets(_playerHandle.Result, _matchStateHandle.Result);
                return Assets;
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        private static void ReleaseHandle(AsyncOperationHandle<GameObject> handle)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
    }
}

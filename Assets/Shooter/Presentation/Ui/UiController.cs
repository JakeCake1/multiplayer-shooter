using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Shooter.Presentation.Ui
{
    public sealed class UiController : IDisposable
    {
        private AsyncOperationHandle<GameObject> _currentHandle;
        private AsyncOperationHandle<GameObject> _loadingHandle;
        private IUiView _currentView;
        private IUiViewModel _loadingViewModel;
        private string _currentAddress;
        private string _loadingAddress;
        private Task _loadingTask;
        private int _revision;
        private bool _disposed;

        public async Task ShowAsync(UiScreenRequest request)
        {
            ThrowIfDisposed();
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (TryRebindCurrent(request))
            {
                return;
            }

            if (TryJoinPendingLoad(request, out var pendingTask))
            {
                await pendingTask;
                return;
            }

            await StartLoadAsync(request);
        }

        public void Hide()
        {
            ThrowIfDisposed();
            _revision++;
            ReleaseCurrentScreen();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _revision++;
            ReleaseCurrentScreen();
        }

        private bool TryRebindCurrent(UiScreenRequest request)
        {
            if (_currentView == null || !string.Equals(_currentAddress, request.Address, StringComparison.Ordinal))
            {
                return false;
            }

            _currentView.Bind(request.ViewModel);
            return true;
        }

        private bool TryJoinPendingLoad(UiScreenRequest request, out Task pendingTask)
        {
            if (_loadingTask == null || !string.Equals(_loadingAddress, request.Address, StringComparison.Ordinal))
            {
                pendingTask = null;
                return false;
            }

            _loadingViewModel = request.ViewModel;
            pendingTask = _loadingTask;
            return true;
        }

        private Task StartLoadAsync(UiScreenRequest request)
        {
            var revision = ++_revision;
            var handle = Addressables.InstantiateAsync(request.Address);
            _loadingAddress = request.Address;
            _loadingViewModel = request.ViewModel;
            _loadingHandle = handle;
            _loadingTask = CompleteLoadAsync(handle, request.Address, revision);
            return _loadingTask;
        }

        private async Task CompleteLoadAsync(AsyncOperationHandle<GameObject> handle, string address, int revision)
        {
            try
            {
                var instance = await handle.Task;
                if (_disposed || revision != _revision)
                {
                    Addressables.ReleaseInstance(handle);
                    return;
                }

                var view = FindView(instance);
                var viewModel = _loadingViewModel;
                view.Bind(viewModel);
                UnityEngine.Object.DontDestroyOnLoad(instance);
                ReplaceCurrentScreen(handle, view, address);
            }
            catch
            {
                ReleaseFailedHandle(handle);
                throw;
            }
            finally
            {
                ClearPendingLoad(handle);
            }
        }

        private static IUiView FindView(GameObject instance)
        {
            var behaviours = instance.GetComponentsInChildren<MonoBehaviour>(true);
            for (var index = 0; index < behaviours.Length; index++)
            {
                if (behaviours[index] is IUiView view)
                {
                    return view;
                }
            }

            throw new InvalidOperationException($"Addressable UI prefab '{instance.name}' does not contain an IUiView component.");
        }

        private void ReplaceCurrentScreen(AsyncOperationHandle<GameObject> handle, IUiView view, string address)
        {
            ReleaseCurrentScreen();
            _currentHandle = handle;
            _currentView = view;
            _currentAddress = address;
        }

        private void ReleaseCurrentScreen()
        {
            _currentView?.Unbind();
            _currentView = null;
            _currentAddress = null;
            if (_currentHandle.IsValid())
            {
                Addressables.ReleaseInstance(_currentHandle);
                _currentHandle = default;
            }
        }

        private void ClearPendingLoad(AsyncOperationHandle<GameObject> handle)
        {
            if (!_loadingHandle.Equals(handle))
            {
                return;
            }

            _loadingHandle = default;
            _loadingAddress = null;
            _loadingViewModel = null;
            _loadingTask = null;
        }

        private static void ReleaseFailedHandle(AsyncOperationHandle<GameObject> handle)
        {
            if (!handle.IsValid())
            {
                return;
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Addressables.ReleaseInstance(handle);
                return;
            }

            Addressables.Release(handle);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(UiController));
            }
        }
    }
}

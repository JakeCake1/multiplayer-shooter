using Fusion;
using UnityEngine;

namespace Shooter.Infrastructure.Fusion
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(FusionPlayerHealthState))]
    public sealed class FusionClientPlayerVisibility : NetworkBehaviour
    {
        private FusionPlayerHealthState _healthState;
        private Renderer[] _renderers;
        private bool? _isVisible;

        public override void Spawned()
        {
            if (Runner.IsServer)
            {
                enabled = false;
                return;
            }

            if (Object.HasInputAuthority)
            {
                return;
            }

            _healthState = GetComponent<FusionPlayerHealthState>();
            _renderers = GetComponentsInChildren<Renderer>(true);
            _isVisible = null;
            RefreshVisibility();
        }

        public override void Render()
        {
            if (_healthState == null)
            {
                return;
            }

            RefreshVisibility();
        }

        private void RefreshVisibility()
        {
            SetVisibility(!_healthState.IsDead);
        }

        private void SetVisibility(bool isVisible)
        {
            if (_isVisible == isVisible)
            {
                return;
            }

            for (var index = 0; index < _renderers.Length; index++)
            {
                if (_renderers[index] != null)
                {
                    _renderers[index].enabled = isVisible;
                }
            }

            _isVisible = isVisible;
        }
    }
}

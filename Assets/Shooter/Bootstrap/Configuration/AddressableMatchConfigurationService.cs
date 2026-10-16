using System;
using Newtonsoft.Json.Linq;
using Shooter.Application;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Shooter.Bootstrap.Configuration
{
    public sealed class AddressableMatchConfigurationService : IMatchConfigurationService, IDisposable
    {
        private AsyncOperationHandle<TextAsset> _handle;
        private MatchConfiguration _configuration;
        private bool _loaded;

        public MatchConfiguration Load()
        {
            if (_loaded)
            {
                return _configuration;
            }

            _handle = Addressables.LoadAssetAsync<TextAsset>(MatchConfigurationAssetAddress.Value);
            var asset = _handle.WaitForCompletion();
            if (_handle.Status != AsyncOperationStatus.Succeeded || asset == null)
            {
                throw new InvalidOperationException($"Failed to load match configuration Addressable '{MatchConfigurationAssetAddress.Value}'.", _handle.OperationException);
            }

            _configuration = Parse(asset.text);
            _loaded = true;
            Debug.Log($"[Configuration] Loaded match configuration v{_configuration.Version} from '{MatchConfigurationAssetAddress.Value}'.");
            return _configuration;
        }

        public void Dispose()
        {
            if (!_handle.IsValid())
            {
                return;
            }

            Addressables.Release(_handle);
            _handle = default;
        }

        private static MatchConfiguration Parse(string json)
        {
            var root = JObject.Parse(json);
            return new MatchConfiguration(RequireInt(root, "version"), ParseMatch(root), ParsePlayer(root), ParseWeapon(root));
        }

        private static MatchTimingConfiguration ParseMatch(JObject root)
        {
            var source = RequireObject(root, "match");
            return new MatchTimingConfiguration(RequireInt(source, "requiredPlayerCount"), RequireFloat(source, "countdownSeconds"), RequireFloat(source, "durationSeconds"), RequireFloat(source, "finishingSeconds"), RequireFloat(source, "serverShutdownGraceSeconds"));
        }

        private static PlayerGameplayConfiguration ParsePlayer(JObject root)
        {
            var source = RequireObject(root, "player");
            return new PlayerGameplayConfiguration(RequireFloat(source, "movementSpeed"), RequireFloat(source, "movementAcceleration"), RequireFloat(source, "movementBraking"), RequireFloat(source, "jumpSpeed"), RequireFloat(source, "gravity"), RequireInt(source, "maximumHealth"), RequireFloat(source, "respawnDelaySeconds"), RequireFloat(source, "minimumLookPitch"), RequireFloat(source, "maximumLookPitch"));
        }

        private static WeaponGameplayConfiguration ParseWeapon(JObject root)
        {
            var source = RequireObject(root, "weapon");
            return new WeaponGameplayConfiguration(RequireFloat(source, "roundsPerMinute"), RequireFloat(source, "range"), RequireInt(source, "damagePerHit"), RequireFloat(source, "shotOriginHeight"), RequireFloat(source, "shotOriginForwardOffset"));
        }

        private static JObject RequireObject(JObject source, string name)
        {
            return source[name] as JObject ?? throw new InvalidOperationException($"Match configuration section '{name}' is required.");
        }

        private static int RequireInt(JObject source, string name)
        {
            return source[name]?.Value<int>() ?? throw new InvalidOperationException($"Match configuration value '{name}' is required.");
        }

        private static float RequireFloat(JObject source, string name)
        {
            return source[name]?.Value<float>() ?? throw new InvalidOperationException($"Match configuration value '{name}' is required.");
        }
    }
}

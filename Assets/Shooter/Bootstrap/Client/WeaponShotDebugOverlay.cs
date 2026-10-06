using System.Collections.Generic;
using Shooter.Infrastructure.Fusion;
using UnityEngine;

namespace Shooter.Bootstrap
{
    public sealed class WeaponShotDebugOverlay : DebugOverlay
    {
        private const float ActivityVisibilitySeconds = 5f;
        private const float RayVisibilitySeconds = 1f;
        private const float Width = 500f;
        private const float Top = 204f;
        private const float RowHeight = 22f;
        private const float VerticalPadding = 16f;
        private const float RayWidth = 0.04f;
        private readonly Dictionary<int, int> _confirmedShotCounts = new Dictionary<int, int>();
        private readonly Dictionary<int, float> _lastShotTimes = new Dictionary<int, float>();
        private readonly Dictionary<int, LineRenderer> _rays = new Dictionary<int, LineRenderer>();
        private readonly Dictionary<int, Material> _rayMaterials = new Dictionary<int, Material>();
        private readonly List<int> _visiblePlayerIds = new List<int>();

        private void Update()
        {
            ObserveConfirmedShots();
            CollectVisiblePlayerIds();
            RefreshRayVisibility();
        }

        private void OnGUI()
        {
            if (_visiblePlayerIds.Count == 0)
            {
                return;
            }

            var height = VerticalPadding + RowHeight * _visiblePlayerIds.Count;
            GUI.Box(new Rect(12f, Top, Width, height), GUIContent.none);
            for (var index = 0; index < _visiblePlayerIds.Count; index++)
            {
                DrawPlayerRow(_visiblePlayerIds[index], index);
            }
        }

        private void OnDestroy()
        {
            foreach (var material in _rayMaterials.Values)
            {
                Destroy(material);
            }
        }

        private void ObserveConfirmedShots()
        {
            var weapons = FindObjectsByType<FusionServerWeapon>(FindObjectsSortMode.None);
            foreach (var weapon in weapons)
            {
                ObserveConfirmedShots(weapon);
            }
        }

        private void ObserveConfirmedShots(FusionServerWeapon weapon)
        {
            if (!IsSpawned(weapon))
            {
                return;
            }

            var playerId = weapon.Object.InputAuthority.PlayerId;
            var confirmedShotCount = weapon.ConfirmedShotCount;
            if (!_confirmedShotCounts.TryGetValue(playerId, out var previousShotCount))
            {
                RegisterPlayer(playerId, confirmedShotCount, weapon);
                return;
            }

            _confirmedShotCounts[playerId] = confirmedShotCount;
            if (confirmedShotCount > previousShotCount)
            {
                RegisterShotActivity(playerId, weapon);
            }
        }

        private void RegisterPlayer(int playerId, int confirmedShotCount, FusionServerWeapon weapon)
        {
            _confirmedShotCounts[playerId] = confirmedShotCount;
            if (confirmedShotCount > 0)
            {
                RegisterShotActivity(playerId, weapon);
            }
        }

        private void RegisterShotActivity(int playerId, FusionServerWeapon weapon)
        {
            _lastShotTimes[playerId] = Time.unscaledTime;
            var ray = GetOrCreateRay(playerId);
            ray.SetPosition(0, weapon.LastShotOrigin);
            ray.SetPosition(1, weapon.LastShotEnd);
            ray.enabled = true;
        }

        private void CollectVisiblePlayerIds()
        {
            _visiblePlayerIds.Clear();
            foreach (var pair in _lastShotTimes)
            {
                if (Time.unscaledTime - pair.Value < ActivityVisibilitySeconds)
                {
                    _visiblePlayerIds.Add(pair.Key);
                }
            }

            _visiblePlayerIds.Sort();
        }

        private void RefreshRayVisibility()
        {
            foreach (var pair in _rays)
            {
                pair.Value.enabled = _lastShotTimes.TryGetValue(pair.Key, out var lastShotTime) && Time.unscaledTime - lastShotTime < RayVisibilitySeconds;
            }
        }

        private LineRenderer GetOrCreateRay(int playerId)
        {
            if (_rays.TryGetValue(playerId, out var existingRay))
            {
                return existingRay;
            }

            var rayObject = new GameObject($"Player {playerId} Shot Debug Ray");
            rayObject.transform.SetParent(transform, false);
            var ray = rayObject.AddComponent<LineRenderer>();
            ConfigureRay(playerId, ray);
            _rays.Add(playerId, ray);
            return ray;
        }

        private void ConfigureRay(int playerId, LineRenderer ray)
        {
            var color = ClientDebugPlayerColors.Get(playerId);
            var shader = Shader.Find("Sprites/Default");
            var material = new Material(shader);
            material.color = Color.white;
            ray.sharedMaterial = material;
            ray.positionCount = 2;
            ray.useWorldSpace = true;
            ray.startWidth = RayWidth;
            ray.endWidth = RayWidth;
            ray.startColor = color;
            ray.endColor = new Color(color.r, color.g, color.b, 0.35f);
            ray.enabled = false;
            _rayMaterials.Add(playerId, material);
        }

        private void DrawPlayerRow(int playerId, int index)
        {
            var previousColor = GUI.color;
            GUI.color = ClientDebugPlayerColors.Get(playerId);
            GUI.Label(new Rect(24f, Top + 8f + RowHeight * index, Width - 24f, RowHeight), $"Player:{playerId}: {_confirmedShotCounts[playerId]} shots");
            GUI.color = previousColor;
        }
    }
}

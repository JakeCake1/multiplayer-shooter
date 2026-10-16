using System;
using Shooter.Application;
using Shooter.Features.MatchRules;
using Shooter.Infrastructure.Fusion;
using UnityEngine;

namespace Shooter.Bootstrap.Client
{
    public sealed class ServerShutdownDebugOverlay : DebugOverlay
    {
        private const float Width = 500f;
        private const float Height = 48f;
        private const float Top = 574f;
        private FusionMatchState _matchState;
        private bool _shutdownScheduled;
        private float _shutdownAt;

        private void Awake()
        {
            var queueName = LocalNetworkLaunchArguments.GetPlayFabMatchmakingQueue(Environment.GetCommandLineArgs());
            enabled = !string.IsNullOrWhiteSpace(queueName);
        }

        private void Update()
        {
            FindMatchStateIfNeeded();
            ScheduleShutdownCountdownIfNeeded();
        }

        private void OnGUI()
        {
            if (!_shutdownScheduled)
            {
                return;
            }

            var secondsRemaining = Mathf.Max(0f, _shutdownAt - Time.realtimeSinceStartup);
            GUI.Box(new Rect(12f, Top, Width, Height), GUIContent.none);
            GUI.Label(new Rect(24f, Top + 8f, Width - 24f, Height - 16f), $"PlayFab Server Shutdown: {secondsRemaining:0.0}s");
        }

        private void FindMatchStateIfNeeded()
        {
            if (FusionNetworkBehaviourLifecycle.IsSpawned(_matchState))
            {
                return;
            }

            _matchState = FindFirstObjectByType<FusionMatchState>();
        }

        private void ScheduleShutdownCountdownIfNeeded()
        {
            if (_shutdownScheduled || !FusionNetworkBehaviourLifecycle.IsSpawned(_matchState) || _matchState.Phase != MatchPhase.Finished)
            {
                return;
            }

            _shutdownScheduled = true;
            _shutdownAt = Time.realtimeSinceStartup + ServerProcessShutdownPolicy.DefaultMatchFinishedGracePeriodSeconds;
        }
    }
}

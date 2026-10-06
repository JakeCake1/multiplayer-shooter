using System;
using Shooter.Application;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shooter.Presentation
{
    public sealed class ClientFlowView : MonoBehaviour
    {
        private IClientFlowController _controller;
        private UIDocument _document;
        private PanelSettings _panelSettings;
        private VisualElement _root;
        private VisualElement _panel;
        private Label _title;
        private Label _status;
        private Button _primaryButton;
        private Button _exitButton;

        public void Initialize(IClientFlowController controller)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            CreateDocument();
            CreateVisualTree();
            _controller.Changed += Render;
            Render(_controller.Current);
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.Changed -= Render;
            }

            if (_panelSettings != null)
            {
                Destroy(_panelSettings);
            }
        }

        private void CreateDocument()
        {
            _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            _panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            _panelSettings.referenceResolution = new Vector2Int(1920, 1080);
            _panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            _panelSettings.match = 0.5f;
            _document = gameObject.AddComponent<UIDocument>();
            _document.panelSettings = _panelSettings;
        }

        private void CreateVisualTree()
        {
            _root = _document.rootVisualElement;
            ConfigureRoot(_root);
            _panel = new VisualElement();
            ConfigurePanel(_panel);
            _title = CreateLabel(32, FontStyle.Bold, Color.white);
            _status = CreateLabel(20, FontStyle.Normal, new Color(0.85f, 0.9f, 1f));
            _primaryButton = new Button(HandlePrimaryClicked);
            _exitButton = new Button(HandleExitClicked) { text = "Exit" };
            ConfigureButton(_primaryButton, new Color(0.15f, 0.55f, 0.95f));
            ConfigureButton(_exitButton, new Color(0.25f, 0.28f, 0.34f));
            _panel.Add(_title);
            _panel.Add(_status);
            _panel.Add(_primaryButton);
            _panel.Add(_exitButton);
            _root.Add(_panel);
        }

        private static void ConfigureRoot(VisualElement root)
        {
            root.style.position = Position.Absolute;
            root.style.left = 0;
            root.style.right = 0;
            root.style.top = 0;
            root.style.bottom = 0;
            root.style.alignItems = Align.Center;
            root.style.justifyContent = Justify.Center;
            root.style.backgroundColor = new Color(0.025f, 0.035f, 0.06f, 0.82f);
            root.pickingMode = PickingMode.Ignore;
        }

        private static void ConfigurePanel(VisualElement panel)
        {
            panel.style.width = 520;
            panel.style.paddingLeft = 36;
            panel.style.paddingRight = 36;
            panel.style.paddingTop = 30;
            panel.style.paddingBottom = 30;
            panel.style.backgroundColor = new Color(0.07f, 0.09f, 0.14f, 0.96f);
            panel.style.borderTopLeftRadius = 12;
            panel.style.borderTopRightRadius = 12;
            panel.style.borderBottomLeftRadius = 12;
            panel.style.borderBottomRightRadius = 12;
        }

        private static Label CreateLabel(int fontSize, FontStyle fontStyle, Color color)
        {
            var label = new Label();
            label.style.fontSize = fontSize;
            label.style.unityFontStyleAndWeight = fontStyle;
            label.style.color = color;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.marginBottom = 20;
            return label;
        }

        private static void ConfigureButton(Button button, Color color)
        {
            button.style.height = 48;
            button.style.marginTop = 8;
            button.style.fontSize = 18;
            button.style.color = Color.white;
            button.style.backgroundColor = color;
        }

        private async void HandlePrimaryClicked()
        {
            SetButtonsEnabled(false);
            try
            {
                await _controller.FindGameAsync();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                SetButtonsEnabled(true);
            }
        }

        private void HandleExitClicked()
        {
            _controller.Exit();
        }

        private void Render(ClientFlowSnapshot snapshot)
        {
            var isPlaying = snapshot.State == ClientFlowState.Playing;
            _root.style.backgroundColor = isPlaying ? Color.clear : new Color(0.025f, 0.035f, 0.06f, 0.82f);
            _panel.style.display = isPlaying ? DisplayStyle.None : DisplayStyle.Flex;
            _title.text = GetTitle(snapshot.State);
            _status.text = GetStatus(snapshot);
            _primaryButton.text = snapshot.State == ClientFlowState.Results ? "Find Game Again" : "Find Game";
            _primaryButton.style.display = snapshot.State == ClientFlowState.Menu || snapshot.State == ClientFlowState.Results ? DisplayStyle.Flex : DisplayStyle.None;
            _exitButton.style.display = snapshot.State == ClientFlowState.Menu || snapshot.State == ClientFlowState.Results ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SetButtonsEnabled(bool enabled)
        {
            _primaryButton?.SetEnabled(enabled);
            _exitButton?.SetEnabled(enabled);
        }

        private static string GetTitle(ClientFlowState state)
        {
            return state == ClientFlowState.Results ? "MATCH RESULTS" : state == ClientFlowState.Menu ? "MULTIPLAYER SHOOTER" : "MATCHMAKING";
        }

        private static string GetStatus(ClientFlowSnapshot snapshot)
        {
            if (!string.IsNullOrWhiteSpace(snapshot.Problem))
            {
                return snapshot.Problem;
            }

            if (snapshot.State == ClientFlowState.Connecting)
            {
                return "Connecting to the local match server...";
            }

            if (snapshot.State == ClientFlowState.WaitingForPlayers)
            {
                return BuildWaitingStatus(snapshot.Match);
            }

            if (snapshot.State == ClientFlowState.Results)
            {
                return BuildResultsStatus(snapshot.Match);
            }

            return "Ready for a local 1v1 match.";
        }

        private static string BuildWaitingStatus(ClientMatchSnapshot match)
        {
            return match == null ? "Connected. Waiting for replicated match state..." : $"Players: {match.ConnectedPlayerCount}/2\nStarting in: {match.SecondsRemaining:0.0}s";
        }

        private static string BuildResultsStatus(ClientMatchSnapshot match)
        {
            if (match == null)
            {
                return "Waiting for authoritative results...";
            }

            var outcome = match.IsDraw ? "Draw" : $"Winner: Player {match.WinnerPlayerId}";
            return $"{outcome}\nPlayer {match.FirstPlayerId}: {match.FirstPlayerKills}\nPlayer {match.SecondPlayerId}: {match.SecondPlayerKills}";
        }
    }
}

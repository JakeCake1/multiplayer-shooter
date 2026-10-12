using System;
using Shooter.Presentation.Ui;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class MainMenuView : UiToolkitView
    {
        private Button _findGameButton;
        private Button _exitButton;
        private Label _problemLabel;
        private MainMenuViewModel _viewModel;

        public override void Bind(IUiViewModel viewModel)
        {
            Unbind();
            _viewModel = viewModel as MainMenuViewModel ?? throw new ArgumentException($"{nameof(MainMenuView)} requires {nameof(MainMenuViewModel)}.", nameof(viewModel));
            ResolveElements();
            Render();
            Subscribe();
        }

        public override void Unbind()
        {
            Unsubscribe();
            _viewModel = null;
        }

        private void ResolveElements()
        {
            _findGameButton = QueryRequired<Button>("find-game-button");
            _exitButton = QueryRequired<Button>("exit-button");
            _problemLabel = QueryRequired<Label>("problem-label");
        }

        private void Render()
        {
            _problemLabel.text = _viewModel.Problem;
            _problemLabel.style.display = string.IsNullOrWhiteSpace(_viewModel.Problem) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void Subscribe()
        {
            _findGameButton.clicked += HandleFindGameClicked;
            _exitButton.clicked += HandleExitClicked;
        }

        private void Unsubscribe()
        {
            if (_findGameButton != null)
            {
                _findGameButton.clicked -= HandleFindGameClicked;
            }

            if (_exitButton != null)
            {
                _exitButton.clicked -= HandleExitClicked;
            }
        }

        private async void HandleFindGameClicked()
        {
            SetButtonsEnabled(false);
            try
            {
                await _viewModel.FindGameAsync();
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
            _viewModel.Exit();
        }

        private void SetButtonsEnabled(bool enabled)
        {
            _findGameButton?.SetEnabled(enabled);
            _exitButton?.SetEnabled(enabled);
        }
    }
}

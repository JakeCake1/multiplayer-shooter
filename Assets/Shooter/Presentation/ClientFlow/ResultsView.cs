using System;
using Shooter.Presentation.Ui;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shooter.Presentation.ClientFlow
{
    public sealed class ResultsView : UiToolkitView
    {
        private Label _outcomeLabel;
        private Label _firstPlayerScoreLabel;
        private Label _secondPlayerScoreLabel;
        private Button _findGameAgainButton;
        private Button _exitButton;
        private ResultsViewModel _viewModel;

        public override void Bind(IUiViewModel viewModel)
        {
            Unbind();
            _viewModel = viewModel as ResultsViewModel ?? throw new ArgumentException($"{nameof(ResultsView)} requires {nameof(ResultsViewModel)}.", nameof(viewModel));
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
            _outcomeLabel = QueryRequired<Label>("outcome-label");
            _firstPlayerScoreLabel = QueryRequired<Label>("first-player-score-label");
            _secondPlayerScoreLabel = QueryRequired<Label>("second-player-score-label");
            _findGameAgainButton = QueryRequired<Button>("find-game-again-button");
            _exitButton = QueryRequired<Button>("exit-button");
        }

        private void Render()
        {
            _outcomeLabel.text = _viewModel.Outcome;
            _firstPlayerScoreLabel.text = _viewModel.FirstPlayerScore;
            _secondPlayerScoreLabel.text = _viewModel.SecondPlayerScore;
        }

        private void Subscribe()
        {
            _findGameAgainButton.clicked += HandleFindGameAgainClicked;
            _exitButton.clicked += HandleExitClicked;
        }

        private void Unsubscribe()
        {
            if (_findGameAgainButton != null)
            {
                _findGameAgainButton.clicked -= HandleFindGameAgainClicked;
            }

            if (_exitButton != null)
            {
                _exitButton.clicked -= HandleExitClicked;
            }
        }

        private async void HandleFindGameAgainClicked()
        {
            SetButtonsEnabled(false);
            try
            {
                await _viewModel.FindGameAgainAsync();
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
            _findGameAgainButton?.SetEnabled(enabled);
            _exitButton?.SetEnabled(enabled);
        }
    }
}

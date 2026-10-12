namespace Shooter.Presentation.Ui
{
    public interface IUiView
    {
        void Bind(IUiViewModel viewModel);

        void Unbind();
    }
}

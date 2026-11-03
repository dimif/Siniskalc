namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Windows.Input;

    /// <summary>
    /// A page that shows what the artifact the pointer rests on does, in an info card. The slot pickers tell it which artifact the pointer
    /// is over (the command's parameter is the name of the artifact, or nothing for an empty slot).
    /// </summary>
    public interface IArtifactInfoSource
    {
        ICommand ShowArtifactInfoCommand { get; }

        ICommand ClearArtifactInfoCommand { get; }
    }
}

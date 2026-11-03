namespace Siniskalc.Heroes.ViewModels
{
    using System.Windows.Input;

    /// <summary>
    /// A hero, a town or the like on a page, shown as a name that opens it and a detail beside it, such as the class and level of a hero.
    /// </summary>
    public interface ILink
    {
        string Name { get; }

        string Detail { get; }

        ICommand OpenCommand { get; }
    }
}

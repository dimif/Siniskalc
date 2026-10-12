namespace Siniskalc.ViewModels
{
    /// <summary>
    /// One line of what artifacts do, such as "Attack +3". See <see cref="EffectLineProvider"/>.
    /// </summary>
    public sealed record EffectLine(string Label, string Value);
}

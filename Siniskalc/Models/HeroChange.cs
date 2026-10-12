namespace Siniskalc.Models
{
    using System;
    using Siniskalc.Core.Models;

    /// <summary>
    /// A <see cref="Change"/> to a hero, with how to undo it.
    /// </summary>
    /// <param name="Revert">Puts the loaded value back on a hero, leaving every other edit alone.</param>
    public sealed record HeroChange(ChangeArea Area, string Description, string Before, string After, Func<HeroModel, HeroModel> Revert)
        : Change(Area, Description, Before, After);
}

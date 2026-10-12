namespace Siniskalc.Wpf.ViewModels
{
    /// <summary>
    /// How a column of a finder shows a property of its rows.
    /// </summary>
    public enum FinderColumnKind
    {
        /// <summary>Plain text.</summary>
        Text,

        /// <summary>Grey text, for details next to the name.</summary>
        MutedText,

        /// <summary>A check box the user can tick; the property is written back.</summary>
        CheckBox,

        /// <summary>A check box that only shows the property.</summary>
        ReadOnlyCheckBox,
    }
}

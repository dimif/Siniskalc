namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Globalization;

    /// <summary>
    /// The words for an amount that a button adds.
    /// </summary>
    public static class AmountText
    {
        /// <summary>
        /// Returns an amount with a plus, in thousands when it is a whole number of them: 10 is "+10", 10000 is "+10k", 1500 is "+1,500".
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <returns>The text.</returns>
        public static string Plus(int amount) =>
            amount >= 1000 && amount % 1000 == 0
                ? $"+{(amount / 1000).ToString("N0", CultureInfo.InvariantCulture)}k"
                : $"+{amount.ToString("N0", CultureInfo.InvariantCulture)}";
    }
}

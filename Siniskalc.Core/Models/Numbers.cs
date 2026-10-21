namespace Siniskalc.Core.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A fixed list of numbers that compares by what it holds, for the lists of a model (the heroes of a player, the creatures in the
    /// dwellings of a castle) so that models can be compared and kept in a record.
    /// </summary>
    public sealed class Numbers : IReadOnlyList<int>, IEquatable<Numbers>
    {
        private readonly int[] numbers;

        public Numbers(IEnumerable<int> numbers)
        {
            this.numbers = numbers.ToArray();
        }

        public int Count => this.numbers.Length;

        public int this[int index] => this.numbers[index];

        public static Numbers Of(params int[] numbers) => new(numbers);

        /// <summary>
        /// Returns the list with the number at <paramref name="index"/> replaced.
        /// </summary>
        public Numbers With(int index, int number)
        {
            var copy = (int[])this.numbers.Clone();
            copy[index] = number;
            return new Numbers(copy);
        }

        public bool Equals(Numbers? other) => other is not null && this.numbers.AsSpan().SequenceEqual(other.numbers);

        public override bool Equals(object? obj) => this.Equals(obj as Numbers);

        public override int GetHashCode()
        {
            var hash = default(HashCode);
            foreach (var number in this.numbers)
            {
                hash.Add(number);
            }

            return hash.ToHashCode();
        }

        public IEnumerator<int> GetEnumerator() => ((IEnumerable<int>)this.numbers).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public override string ToString() => string.Join(", ", this.numbers);
    }
}

namespace Siniskalc.Core.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// An immutable list of integers with value equality: two lists are equal when they hold the same numbers in the same order.
    /// </summary>
    /// <remarks>
    /// A plain <c>int[]</c> or <c>List&lt;int&gt;</c> is equal only to itself, so a record that holds one is never equal to another record with
    /// the same contents. Models use this type for their lists of numbers (the hero ids of a player, the creature counts in the dwellings of
    /// a castle) so that two models with the same values compare equal. Once made, the list does not change: <see cref="With"/> returns a new one.
    /// </remarks>
    public sealed class Numbers : IReadOnlyList<int>, IEquatable<Numbers>
    {
        private readonly int[] numbers;

        /// <summary>
        /// Initializes a new instance of the <see cref="Numbers"/> class.
        /// </summary>
        /// <param name="numbers">The numbers, in order. They are copied, so a later change to the source does not reach the list.</param>
        /// <exception cref="ArgumentNullException"><paramref name="numbers"/> is <see langword="null"/>.</exception>
        public Numbers(IEnumerable<int> numbers)
        {
            this.numbers = numbers.ToArray();
        }

        /// <summary>
        /// Gets the number of numbers in the list.
        /// </summary>
        public int Count => this.numbers.Length;

        /// <summary>
        /// Gets the number at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The place of the number in the list.</param>
        /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is negative or not less than <see cref="Count"/>.</exception>
        public int this[int index] => this.numbers[index];

        /// <summary>
        /// Makes a list of the given numbers.
        /// </summary>
        /// <param name="numbers">The numbers, in order.</param>
        /// <returns>A new list holding <paramref name="numbers"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="numbers"/> is <see langword="null"/>.</exception>
        public static Numbers Of(params int[] numbers) => new(numbers);

        /// <summary>
        /// Returns the list with the number at <paramref name="index"/> replaced.
        /// </summary>
        /// <param name="index">The place of the number to replace.</param>
        /// <param name="number">The number to put there.</param>
        /// <returns>A new list; this one is not changed.</returns>
        /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is negative or not less than <see cref="Count"/>.</exception>
        public Numbers With(int index, int number)
        {
            var copy = (int[])this.numbers.Clone();
            copy[index] = number;
            return new Numbers(copy);
        }

        /// <summary>
        /// Returns whether <paramref name="other"/> holds the same numbers in the same order.
        /// </summary>
        /// <param name="other">The list to compare with.</param>
        /// <returns><see langword="true"/> if both lists hold the same numbers in the same order; <see langword="false"/> otherwise, also when <paramref name="other"/> is <see langword="null"/>.</returns>
        public bool Equals(Numbers? other) => other is not null && this.numbers.AsSpan().SequenceEqual(other.numbers);

        /// <summary>
        /// Returns whether <paramref name="obj"/> is a list of the same numbers.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="Numbers"/> that is equal to this one.</returns>
        public override bool Equals(object? obj) => this.Equals(obj as Numbers);

        /// <summary>
        /// Returns a hash code made from every number, in order, so equal lists have equal hash codes.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            var hash = default(HashCode);
            foreach (var number in this.numbers)
            {
                hash.Add(number);
            }

            return hash.ToHashCode();
        }

        /// <summary>
        /// Returns an enumerator over the numbers, in order.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<int> GetEnumerator() => ((IEnumerable<int>)this.numbers).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Returns the numbers separated by commas.
        /// </summary>
        /// <returns>The text, such as "1, 2, 3"; empty for an empty list.</returns>
        public override string ToString() => string.Join(", ", this.numbers);
    }
}

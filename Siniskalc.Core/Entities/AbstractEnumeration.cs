namespace Siniskalc.Core.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Represents a value of an enumeration that is not known at compile time, but is defined by static fields of a derived class.
    /// </summary>
    /// <remarks>
    /// See https://learn.microsoft.com/en-gb/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/enumeration-classes-over-enum-types.
    /// </remarks>
    public abstract class AbstractEnumeration : IGameEntity, IComparable, IEquatable<AbstractEnumeration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractEnumeration"/> class.
        /// </summary>
        /// <param name="name">
        /// The name of the enumeration.
        /// </param>
        /// <param name="code">
        /// The byte code associated with the enumeration.
        /// </param>
        /// <param name="availableFrom">
        /// The a number that represents what version of the game was this entity introduced in.
        /// </param>
        protected AbstractEnumeration(string name, byte code, int availableFrom)
        {
            this.Name = name;
            this.Code = code;
            this.AvailableFrom = availableFrom;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractEnumeration"/> class.
        /// </summary>
        /// <param name="name">
        /// The name of the enumeration.
        /// </param>
        /// <param name="code">
        /// The byte code associated with the enumeration.
        /// </param>
        protected AbstractEnumeration(string name, byte code)
            : this(name, code, 0)
        {
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public byte Code { get; }

        /// <summary>
        /// Gets the first tier (a version of the game, as a number that grows with every expansion) this value exists in.
        /// </summary>
        public int AvailableFrom { get; }

        public static bool operator ==(AbstractEnumeration? left, AbstractEnumeration? right)
            => left is null ? right is null : left.Equals(right);

        public static bool operator !=(AbstractEnumeration? left, AbstractEnumeration? right) => !(left == right);

        /// <summary>
        /// Returns all of the enumerations.
        /// </summary>
        /// <typeparam name="T">
        /// The type of enumeration to return.
        /// </typeparam>
        /// <returns>
        /// A read-only collection of the enumerated entities.
        /// </returns>
        public static IReadOnlyList<T> GetAll<T>()
            where T : AbstractEnumeration => Values<T>.All;

        /// <summary>
        /// Returns the enumerable entity matching the code, or <c>null</c>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of enumeration to return.
        /// </typeparam>
        /// <param name="code">
        /// The code associated with the entity.
        /// </param>
        /// <returns>
        /// The entity, or <c>null</c>.
        /// </returns>
        public static T? FromCode<T>(byte code)
            where T : AbstractEnumeration
            => Values<T>.ByCode.TryGetValue(code, out var value) ? value : null;

        /// <summary>
        /// Returns the enumerable entity matching the name, or <c>null</c>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of enumeration to return.
        /// </typeparam>
        /// <param name="name">
        /// The name associated with the entity.
        /// </param>
        /// <returns>
        /// The entity, or <c>null</c>.
        /// </returns>
        public static T? FromName<T>(string name)
            where T : AbstractEnumeration
            => GetAll<T>().FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Gets whether this value exists in savefiles written by the given tier.
        /// Expansions only add values, so anything introduced in or before <paramref name="tier"/> is available.
        /// </summary>
        public bool IsAvailableIn(int tier) => this.AvailableFrom <= tier;

        /// <inheritdoc />
        public override string ToString() => this.Name;

        /// <inheritdoc />
        public virtual bool Equals(AbstractEnumeration? other)
            => other is not null && this.GetType() == other.GetType() && this.Code == other.Code;

        /// <inheritdoc />
        public override bool Equals(object? obj) => this.Equals(obj as AbstractEnumeration);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(this.GetType(), this.Code);

        /// <inheritdoc />
        public int CompareTo(object? obj) => this.Code.CompareTo(((AbstractEnumeration)obj!).Code);

        /// <summary>
        /// The values of one enumeration, read once from its static fields.
        /// </summary>
        /// <remarks>
        /// Read on first use, from the static fields of <typeparamref name="T"/>. A static initializer of an enumeration must
        /// not call <see cref="GetAll{T}"/>, <see cref="FromCode{T}"/> or <see cref="FromName{T}"/> on its own type, as the values
        /// are still being created then and the call would fail.
        /// </remarks>
        private static class Values<T>
            where T : AbstractEnumeration
        {
            public static readonly IReadOnlyList<T> All = typeof(T)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(f => typeof(T).IsAssignableFrom(f.FieldType))
                .Select(f => (T)f.GetValue(null)!)
                .ToList();

            // Some enumerations share codes between values (the buildings of different factions); the first one declared wins.
            public static readonly IReadOnlyDictionary<byte, T> ByCode = BuildByCode();

            private static Dictionary<byte, T> BuildByCode()
            {
                var result = new Dictionary<byte, T>();
                foreach (var value in All)
                {
                    result.TryAdd(value.Code, value);
                }

                return result;
            }
        }
    }
}

using System;

namespace ReportsGenerator.Model
{
    /// <summary>
    /// An abstraction representing numbers of cases linked by their origin - it could be a 'Total' per day, or 'Change' for the long period.
    /// </summary>
    public readonly struct Metrics
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Metrics"/> structure.
        /// </summary>
        /// <param name="confirmed">Number of confirmed cases.</param>
        /// <param name="active">Number of active cases.</param>
        /// <param name="recovered">Number of recovered cases.</param>
        /// <param name="deaths">Number of deaths cases.</param>
        public Metrics(in long confirmed, in long active, in long recovered, in long deaths)
        {
            Confirmed = confirmed;
            Deaths = deaths;
            Active = active;
            Recovered = recovered;
        }

        // ReSharper disable once UnassignedGetOnlyAutoProperty
        /// <summary>
        /// Gets an empty <see cref="Metrics"/>.
        /// </summary>
        public static Metrics Empty { get; }

        /// <summary>
        /// Gets a number of active cases.
        /// </summary>
        public long Active { get; }

        /// <summary>
        /// Gets a number of confirmed cases.
        /// </summary>
        public long Confirmed { get; }

        /// <summary>
        /// Gets a number of deaths cases.
        /// </summary>
        public long Deaths { get; }

        /// <summary>
        /// Gets a number of recovered cases.
        /// </summary>
        public long Recovered { get; }

        /// <summary>
        /// A helper function that converts <see cref="Entry"/> to <see cref="Metrics"/>.
        /// </summary>
        /// <param name="entry">An object to convert.</param>
        /// <returns>A new <see cref="Metrics"/> instance, that contains values from <see cref="Entry"/> object.</returns>
        public static Metrics FromEntry(Entry entry)
        {
            return entry != Entry.Empty ? new Metrics(entry.Confirmed,
                entry.Active,
                entry.Recovered,
                entry.Deaths) : Metrics.Empty;
        }

        /// <summary>
        /// Subtracts 'right' <see cref="Metrics"/> instance from 'left' <see cref="Metrics"/> instance.
        /// </summary>
        /// <param name="left">Minuend <see cref="Metrics"/> instance.</param>
        /// <param name="right">Subtrahend <see cref="Metrics"/> instance.</param>
        /// <returns>A new <see cref="Metrics"/> instance each property of which is a difference value of parameters values.</returns>
        public static Metrics operator -(Metrics left, Metrics right)
        {
            return new Metrics(left.Confirmed - right.Confirmed,
                left.Active - right.Active,
                left.Recovered - right.Recovered,
                left.Deaths - right.Deaths
            );
        }

        /// <summary>
        /// Compares two <see cref="Metrics"/> instances for inequality.
        /// </summary>
        /// <param name="left">First <see cref="Metrics"/> instance to compare.</param>
        /// <param name="right">Second <see cref="Metrics"/> instance to compare.</param>
        /// <returns><langword>True</langword> if instances has the different property values, otherwise returns <langword>False</langword>.</returns>
        public static bool operator !=(Metrics left, Metrics right) => !left.Equals(right);

        /// <summary>
        /// Sums up two <see cref="Metrics"/> instances.
        /// </summary>
        /// <param name="left">First <see cref="Metrics"/> instance to sum.</param>
        /// <param name="right">Second <see cref="Metrics"/> instance to sum.</param>
        /// <returns>A new <see cref="Metrics"/> instance each property of which is a sum value of parameters values.</returns>
        public static Metrics operator +(Metrics left, Metrics right)
        {
            return new Metrics(left.Confirmed + right.Confirmed,
                left.Active + right.Active,
                left.Recovered + right.Recovered,
                left.Deaths + right.Deaths
            );
        }

        /// <summary>
        /// Compares two <see cref="Metrics"/> instances for equality.
        /// </summary>
        /// <param name="left">First <see cref="Metrics"/> instance to compare.</param>
        /// <param name="right">Second <see cref="Metrics"/> instance to compare.</param>
        /// <returns><langword>True</langword> if instances has the same property values, otherwise returns <langword>False</langword>.</returns>
        public static bool operator ==(Metrics left, Metrics right) => left.Equals(right);

        /// <summary>
        /// Deconstructs single <see cref="Metrics"/> objects to its properties.
        /// </summary>
        /// <param name="confirmed">Number of confirmed cases.</param>
        /// <param name="active">Number of active cases.</param>
        /// <param name="recovered">Number of recovered cases.</param>
        /// <param name="deaths">Number of deaths cases.</param>
        public void Deconstruct(out long confirmed, out long active, out long recovered, out long deaths)
        {
            confirmed = Confirmed;
            active = Active;
            recovered = Recovered;
            deaths = Deaths;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Metrics other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Confirmed, Deaths, Active, Recovered);
        }

        /// <inheritdoc />
        public override string ToString() => $"{Confirmed} {Active} {Recovered} {Deaths}";

        private bool Equals(Metrics other)
        {
            var (confirmed, active, recovered, deaths) = other;
            return Confirmed == confirmed && Deaths == deaths && Active == active && Recovered == recovered;
        }
    }
}
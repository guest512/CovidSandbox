namespace ReportsGenerator.Model
{
    /// <summary>
    /// An abstraction representing numbers of cases linked by their origin - it could be a 'Total' per day, or 'Change' for the long period.
    /// </summary>
    public record Metrics(long Confirmed, long Active, long Recovered, long Deaths)
    {
        /// <summary>
        /// Gets an empty <see cref="Metrics"/>.
        /// </summary>
        public static Metrics Empty { get; } = new(0, 0, 0, 0);
        
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

        /// <inheritdoc />
        public override string ToString() => $"{Confirmed} {Active} {Recovered} {Deaths}";
    }
}
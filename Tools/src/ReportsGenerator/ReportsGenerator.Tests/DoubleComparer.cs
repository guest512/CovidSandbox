using System;
using System.Collections;

namespace ReportsGenerator.Tests
{
    public class DoubleComparer : IComparer
    {
        private readonly double _epsilon;

        public DoubleComparer(double epsilon)
        {
            _epsilon = epsilon;
        }

        public int Compare(object? x, object? y)
        {
            var xNum = (double)(x ?? throw new ArgumentNullException(nameof(x)));
            var yNum = (double)(y ?? throw new ArgumentNullException(nameof(y)));

            if (Math.Abs(xNum - yNum) < _epsilon)
                return 0;

            if (xNum > yNum)
                return 1;

            return -1;
        }
    }
}
using System;
using System.Collections.Generic;

namespace Utils
{
    public class LambdaComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _comparer;

        public LambdaComparer(Func<T, T, bool> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            _comparer = comparer;
        }
        public bool Equals(T x, T y) => _comparer(x, y);
        public int GetHashCode(T obj) => obj.GetHashCode();
    }
}

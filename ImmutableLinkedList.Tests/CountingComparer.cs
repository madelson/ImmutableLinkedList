using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medallion.Collections.Tests
{
    internal class CountingEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly IEqualityComparer<T> _comparer;

        public int EqualsCount { get; private set; }

        public CountingEqualityComparer(IEqualityComparer<T> comparer = null)
        {
            this._comparer = comparer ?? EqualityComparer<T>.Default;
        }

        public bool Equals(T a, T b)
        {
            ++this.EqualsCount;
            return this._comparer.Equals(a, b);
        }

        public int GetHashCode(T value) => this._comparer.GetHashCode(value);
    }
}

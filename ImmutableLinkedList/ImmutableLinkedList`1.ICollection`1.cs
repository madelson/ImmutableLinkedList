using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections;

// Even though ImmutableLinkedList is read-only, implementing the ICollection<T> interface can be useful.
// The CopyTo() method provides a convenient mechanism for fast array copies, and many framework APIs that
// take IEnumerable (e. g. new List<T>(collection) will check for implementation of ICollection<T> to leverage
// the Count property / CopyTo for optimization

public partial struct ImmutableLinkedList<T> : ICollection<T>
{
    bool ICollection<T>.IsReadOnly => true;

    void ICollection<T>.Add(T item) => throw ReadOnly();

    void ICollection<T>.Clear() => throw ReadOnly();
    
    /// <summary>
    /// Copies the elements of the list to <paramref name="array"/>, starting at <paramref name="arrayIndex"/>
    /// </summary>
    public void CopyTo(T[] array, int arrayIndex)
    {
        if (array == null) { throw new ArgumentNullException(nameof(array)); }
        if ((uint)arrayIndex > (uint)array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "must be non-negative and less than or equal to the array length");
        }
        if (arrayIndex + this.Count > array.Length)
        {
            throw new ArgumentException("destination array was not long enough", nameof(array));
        }

        var currentIndex = arrayIndex;
        for (var current = this._head; current != null; current = current.Next)
        {
            array[currentIndex++] = current.Value;
        }
    }

    bool ICollection<T>.Remove(T item) => throw ReadOnly();

    private static NotSupportedException ReadOnly() => new("the collection is read-only");
}

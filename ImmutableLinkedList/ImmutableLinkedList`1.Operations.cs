using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Medallion.Collections
{
    public partial struct ImmutableLinkedList<T>
    {
        /// <summary>
        /// Returns true if <paramref name="value"/> is an element of the list.
        /// 
        /// This method is O(N)
        /// </summary>
        public bool Contains(T value)
        {
            var comparer = EqualityComparer<T>.Default;
            for (var current = this._head; current != null; current = current.Next)
            {
                if (comparer.Equals(current.Value, value)) { return true; }
            }

            return false;
        }

		/// <summary>
        /// Same as <see cref="Enumerable.SequenceEqual{TSource}(IEnumerable{TSource}, IEnumerable{TSource}, IEqualityComparer{TSource})"/>,
        /// but optimized for comparing two instances of <see cref="ImmutableLinkedList{T}"/>.
        /// 
        /// This method is O(n).
        /// </summary>
        public bool SequenceEqual(ImmutableLinkedList<T> that, IEqualityComparer<T>? comparer = null)
        {
            if (this._count != that._count) { return false; }

            var comparerToUse = comparer ?? EqualityComparer<T>.Default;

            var thisCurrent = this._head;
            var thatCurrent = that._head;
            while (true)
            {
                if (thisCurrent == thatCurrent) { return true; }
                if (thisCurrent == null || !comparerToUse.Equals(thisCurrent.Value, thatCurrent!.Value)) { return false; }

                thisCurrent = thisCurrent.Next;
                thatCurrent = thatCurrent.Next;
            }
        }

        #region ---- Prepend ----
        /// <summary>
        /// Returns a new list with <paramref name="value"/> prepended.
        /// 
        /// This method is O(1) and requires no copying.
        /// </summary>
        public ImmutableLinkedList<T> Prepend(T value) => new ImmutableLinkedList<T>(new Node(value) { Next = this._head }, this._count + 1);

		/// <summary>
        /// Returns a new list with <paramref name="values"/> prepended such that the first element of <paramref name="values"/> values
        /// becomes the first element of the returned list.
        /// 
        /// This method is O(k) where k is the number of elements in <paramref name="values"/> and requires no copying.
        /// </summary>
        public ImmutableLinkedList<T> PrependRange(IEnumerable<T> values)
        {
            if (this._count == 0) { return CreateRange(values); }

            if (values is ImmutableLinkedList<T> list)
            {
                return this.PrependRange(list);
            }

            if (values == null) { throw new ArgumentNullException(nameof(values)); }

            using (var enumerator = values.GetEnumerator())
            {
                if (!enumerator.MoveNext()) { return this; }

                var head = new Node(enumerator.Current);
                var last = head;
                var count = 1;
                while (enumerator.MoveNext())
                {
                    last = last.Next = new Node(enumerator.Current);
                    ++count;
                }

                last.Next = this._head;
                return new ImmutableLinkedList<T>(head, count + this._count);
            }
        }

		/// <summary>
        /// Same as <see cref="PrependRange(IEnumerable{T})"/>, but optimized for prepending another
        /// instance of <see cref="ImmutableLinkedList{T}"/>
        /// </summary>
        public ImmutableLinkedList<T> PrependRange(ImmutableLinkedList<T> list)
        {
            if (list._count == 0) { return this; }
            if (this._count == 0) { return list; }

            CopyNonEmptyRange(list._head!, null, out var newHead, out var newLast);
            newLast.Next = this._head;
            return new ImmutableLinkedList<T>(newHead, this._count + list._count);
        }
        #endregion

        #region ---- Append ----
		/// <summary>
        /// Returns a new list with <paramref name="value"/> appended.
        /// 
        /// This method is O(n) and requires copying the entire list.
        /// </summary>
        public ImmutableLinkedList<T> Append(T value)
        {
            if (this._count == 0) { return Create(value); }

            CopyNonEmptyRange(this._head!, null, out var newHead, out var newLast);
            newLast.Next = new Node(value);
            return new ImmutableLinkedList<T>(newHead, this._count + 1);
        }

		/// <summary>
        /// Returns a new list with <paramref name="values"/> appended.
        /// 
        /// This method is O(n + k) where k is the number of elements in <paramref name="values"/>. It requires
        /// copying the entire list.
        /// </summary>
        public ImmutableLinkedList<T> AppendRange(IEnumerable<T> values) => this.AppendRange(CreateRange(values));

		/// <summary>
        /// Same as <see cref="AppendRange(ImmutableLinkedList{T})"/>, but optimized for appending 
        /// another instance of <see cref="ImmutableLinkedList{T}"/>
        /// </summary>
        public ImmutableLinkedList<T> AppendRange(ImmutableLinkedList<T> list) => list.PrependRange(this);
        #endregion

        #region ---- Remove ----
        /// <summary>
        /// Returns a new list with the first instance of <paramref name="value"/> removed (if present).
        /// 
        /// This method is O(n). If <paramref name="value"/> exists in the list and is removed, all elements 
        /// prior to <paramref name="value"/> must be copied.
        /// </summary>
        public ImmutableLinkedList<T> Remove(T value)
        {
            var comparer = EqualityComparer<T>.Default;
            for (var current = this._head; current != null; current = current.Next)
            {
                if (comparer.Equals(current.Value, value))
                {
                    if (current == this._head)
                    {
                        return new ImmutableLinkedList<T>(this._head.Next, this._count - 1);
                    }

                    CopyNonEmptyRange(this._head!, current, out var newHead, out var newLast);
                    newLast.Next = current.Next;
                    return new ImmutableLinkedList<T>(newHead, this._count - 1);
                }
            }

            return this;
        }

        /// <summary>
        /// Returns a new list with the <paramref name="index"/>th element removed. Throws <see cref="ArgumentOutOfRangeException"/>
        /// if <paramref name="index"/> is not a valid list index.
        /// 
        /// This method is O(<paramref name="index"/>) and will result in the copying of the first <paramref name="index"/> - 1 elements.
        /// </summary>
        public ImmutableLinkedList<T> RemoveAt(int index) => this.RemoveAt(index, out _);

		/// <summary>
        /// Same as <see cref="RemoveAt(int)"/>, but also returns the <paramref name="removed"/> value
        /// </summary>
        public ImmutableLinkedList<T> RemoveAt(int index, out T removed)
        {
            if (index < 0 || index >= this._count) { throw new ArgumentOutOfRangeException(nameof(index), index, "must be non-negative and less than the length of the list"); }

            // remove at 0 requires no copying
            if (index == 0)
            {
                removed = this._head!.Value;
                return new ImmutableLinkedList<T>(this._head.Next, this._count - 1);
            }

            var current = this._head!.Next;
            for (var i = 1; i < index; ++i) { current = current!.Next; }

            CopyNonEmptyRange(this._head, current, out var newHead, out var newLast);
            newLast.Next = current!.Next;
            removed = current.Value;
            return new ImmutableLinkedList<T>(newHead, this._count - 1);
        }

		/// <summary>
        /// Returns a new list with all elements matching <paramref name="predicate"/> removed.
        /// 
        /// This method is O(n). All retained elements prior to the last removed element are copied.
        /// </summary>
        public ImmutableLinkedList<T> RemoveAll(Func<T, bool> predicate)
        {
            // first, remove prefix since we can do this without any copying
            var withoutPrefix = this.SkipWhile(predicate);
            if (withoutPrefix._count == 0) { return Empty; }

            // now, remove any elements from the middle/end, copying only if necessary
            var newHead = withoutPrefix._head!;
            var lastNonRemoved = newHead;
            var countRemoved = 0;
            var current = newHead.Next;
            while (current != null)
            {
                if (predicate(current.Value))
                {
                    if (countRemoved == 0)
                    {
                        // force a copy
                        CopyNonEmptyRange(newHead, current, out var copiedNewHead, out var copiedLastNonRemoved);
                        newHead = copiedNewHead;
                        lastNonRemoved = copiedLastNonRemoved;
                    }
                    ++countRemoved;
                }
                else if (countRemoved != 0)
                {
                    // if we copied, copy the current node
                    lastNonRemoved = lastNonRemoved.Next = new Node(current.Value);
                }
                else
                {
                    // if we didn't copy, we can just advance our last pointer
                    lastNonRemoved = current;
                }

                current = current.Next;
            }

            return new ImmutableLinkedList<T>(newHead, withoutPrefix._count - countRemoved);
        }
        #endregion

        #region ---- Skip ----
		/// <summary>
        /// Same as <see cref="Enumerable.Skip{TSource}(IEnumerable{TSource}, int)"/>, except the
        /// return type is <see cref="ImmutableLinkedList{T}"/> and allocations are avoided.
        /// 
        /// This method is O(<paramref name="count"/>) and does not result in any copying.
        /// </summary>
        public ImmutableLinkedList<T> Skip(int count)
        {
            if (count >= this._count) { return Empty; }

            var skipped = 0;
            var current = this._head;
            while (current != null && skipped < count)
            {
                current = current.Next;
                ++skipped;
            }

            return new ImmutableLinkedList<T>(current, this._count - skipped);
        }

		/// <summary>
        /// Same as <see cref="Enumerable.SkipWhile{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>, except
        /// the return type is <see cref="ImmutableLinkedList{T}"/> and allocations are avoided.
        /// 
        /// This method is O(skipped) and does not result in any copying.
        /// </summary>
        public ImmutableLinkedList<T> SkipWhile(Func<T, bool> predicate)
        {
            if (predicate == null) { throw new ArgumentNullException(nameof(predicate)); }

            var newHead = this._head;
            var countRemoved = 0;
            while (newHead != null && predicate(newHead.Value))
            {
                newHead = newHead.Next;
                ++countRemoved;
            }

            return new ImmutableLinkedList<T>(newHead, this._count - countRemoved);
        }

        /// <summary>
        /// Same as <see cref="Enumerable.SkipWhile{TSource}(IEnumerable{TSource}, Func{TSource, int, bool})"/>, except
        /// the return type is <see cref="ImmutableLinkedList{T}"/> and allocations are avoided.
        /// 
        /// This method is O(skipped) and does not result in any copying.
        /// </summary>
        public ImmutableLinkedList<T> SkipWhile(Func<T, int, bool> predicate)
        {
            if (predicate == null) { throw new ArgumentNullException(nameof(predicate)); }

            var newHead = this._head;
            var index = 0;
            var countRemoved = 0;
            while (newHead != null && predicate(newHead.Value, index))
            {
                newHead = newHead.Next;
                ++index;
                ++countRemoved;
            }

            return new ImmutableLinkedList<T>(newHead, this._count - countRemoved);
        }
        #endregion
        
        /// <summary>
        /// Returns an <see cref="ImmutableLinkedList{T}"/> for the index range described
        /// by <paramref name="startIndex"/> and <paramref name="count"/> (similar to
        /// <see cref="String.Substring(int, int)"/>).
        /// 
        /// This method is O(<paramref name="startIndex"/> + <paramref name="count"/>) and
        /// copies all elements in the returned sublist unless the sublist extends to the
        /// end of the current list.
        /// </summary>
        public ImmutableLinkedList<T> SubList(int startIndex, int count)
        {
            if (startIndex < 0) { throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "must be non-negative"); }
            if (count < 0) { throw new ArgumentOutOfRangeException(nameof(count), count, "must be non-negative"); }
            if (startIndex + count > this._count)
            {
                throw new ArgumentOutOfRangeException($"({nameof(startIndex)}, {nameof(count)})", $"({startIndex}, {count})", "must represent a range of indices within the list");
            }
            
            if (count == 0) { return Empty; }

            var skipped = this.Skip(startIndex);
            if (count == skipped.Count) { return skipped; }

            var current = skipped._head!.Next;
            for (var i = 1; i < count; ++i) { current = current!.Next; }
            CopyNonEmptyRange(skipped._head, current, out var subListHead, out _);
            return new ImmutableLinkedList<T>(subListHead, count);
        }

		/// <summary>
        /// Same as <see cref="Enumerable.Reverse{TSource}(IEnumerable{TSource})"/>, but the return type is <see cref="ImmutableLinkedList{T}"/>.
        /// 
        /// This method is O(n) and involves copying the entire list for lists of length two or greater.
        /// </summary>
        public ImmutableLinkedList<T> Reverse()
        {
            if (this._count < 2) { return this; }

            var current = new Node(this._head!.Value);
            for (var next = this._head.Next; next != null; next = next.Next)
            {
                current = new Node(next.Value) { Next = current };
            }

            return new ImmutableLinkedList<T>(current, this._count);
        }
    }
}

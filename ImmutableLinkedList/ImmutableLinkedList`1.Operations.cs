using System;
using System.Collections.Generic;
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

            CopyNonEmptyRange(list._head, null, out var newHead, out var newLast);
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

            CopyNonEmptyRange(this._head, null, out var newHead, out var newLast);
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
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    /// <summary>
    /// An immutable linked list data structure
    /// </summary>
    public readonly partial struct ImmutableLinkedList<T> : IReadOnlyCollection<T>, IEquatable<ImmutableLinkedList<T>>
    {
        /// <summary>
        /// The empty list
        /// </summary>
        public static ImmutableLinkedList<T> Empty => default;

        private readonly Node? _head;
        private readonly int _count;

        private ImmutableLinkedList(Node? head, int count)
        {
            this._head = head;
            this._count = count;

#if INVARIANT_CHECKS
            this._head?.Freeze();

            // verify count
            var calculatedCount = 0;
            for (var current = this._head; current != null; current = current.Next)
            {
                ++calculatedCount;
            }
            if (calculatedCount != this._count) { throw new ArgumentException($"invalid count. Received {this._count} but was {calculatedCount}"); }
#endif
        }

        /// <summary>
        /// The length of the list
        /// </summary>
        public int Count => this._count;

        /// <summary>
        /// The first element in the list. Throws <see cref="InvalidOperationException"/> if the list is empty.
        /// </summary>
        public T Head
        {
            get
            {
                if (this._count == 0) { ThrowEmpty(); }
                return this._head!.Value;
            }
        }

        /// <summary>
        /// A list consisting of all elements except the first. Throws <see cref="InvalidOperationException"/> if the list is empty.
        /// 
        /// This property is O(1) and does not require any copying.
        /// </summary>
        public ImmutableLinkedList<T> Tail
        {
            get
            {
                if (this._count == 0) { ThrowEmpty(); }
                return new ImmutableLinkedList<T>(this._head!.Next, this._count - 1);
            }
        }

        internal static ImmutableLinkedList<T> Create(T value) => new ImmutableLinkedList<T>(new Node(value), 1);

        internal static ImmutableLinkedList<T> CreateRange(IEnumerable<T> values)
        {
            if (values == null) { throw new ArgumentNullException(nameof(values)); }

            if (values is ImmutableLinkedList<T> list) { return list; }

            using (var enumerator = values.GetEnumerator())
            {
                if (!enumerator.MoveNext()) { return Empty; }

                var head = new Node(enumerator.Current);
                var last = head;
                var count = 1;
                while (enumerator.MoveNext())
                {
                    last = last.Next = new Node(enumerator.Current);
                    ++count;
                }

                return new ImmutableLinkedList<T>(head, count);
            }
        }

        /// <summary>
        /// Splits the list into head and tail in a single operation. Throws <see cref="InvalidOperationException"/> if the
        /// list is empty. This method can be invoked implicitly using tuple deconstruction syntax:
        /// <code>
        ///     var (head, tail) = list;
        /// </code>
        /// 
        /// This method runs in O(1) and does not require any copying.
        /// </summary>
        public void Deconstruct(out T head, out ImmutableLinkedList<T> tail)
        {
            if (this._count == 0) { ThrowEmpty(); }

            head = this._head!.Value;
            tail = new ImmutableLinkedList<T>(this._head.Next, this._count - 1);
        }

        /// <summary>
        /// Equivalent to <see cref="Deconstruct(out T, out ImmutableLinkedList{T})"/>, but 
        /// returns false rather than throwing if the list is empty
        /// </summary>
        public bool TryDeconstruct(out T head, out ImmutableLinkedList<T> tail)
        {
            if (this._count != 0)
            {
                head = this._head!.Value;
                tail = new ImmutableLinkedList<T>(this._head.Next, this._count - 1);
                return true;
            }

            head = default!;
            tail = default;
            return false;
        }

        /// <summary>
        /// Prevents boxing when using lists with <see cref="EqualityComparer{T}.Default"/>
        /// </summary>
        bool IEquatable<ImmutableLinkedList<T>>.Equals(ImmutableLinkedList<T> other) => this._head == other._head;

        private static void ThrowEmpty() => throw new InvalidOperationException("the list is empty");

        private static void CopyNonEmptyRange(Node head, Node? last, out Node newHead, out Node newLast)
        {
#if INVARIANT_CHECKS
            if (head == last) { throw new InvalidOperationException("range was empty"); }
#endif

            var newCurrent = newHead = new Node(head.Value);
            for (var current = head.Next; current != last; current = current.Next)
            {
                newCurrent = newCurrent.Next = new Node(current!.Value);
            }
            newLast = newCurrent;
        }
    }
}

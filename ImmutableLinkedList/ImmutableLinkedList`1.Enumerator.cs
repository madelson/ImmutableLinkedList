using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    public partial struct ImmutableLinkedList<T> : IEnumerable<T>
    {
        /// <summary>
        /// Returns an <see cref="IEnumerator{T}"/> that iterates through the list. 
        /// 
        /// This method returns a value type enumerator (similar to <see cref="List{T}.Enumerator"/>). This improves
        /// the efficiency of foreach loops but means that the <see cref="Enumerator"/> has value type semantics with
        /// respect to copying. For example, copying an <see cref="Enumerator"/> value to another value captures a
        /// "snapshot" of the enumerator state; the original variable can continue to be enumerated over while the
        /// snapshot remains at the original state (and can be advanced from there)
        /// </summary>
        public Enumerator GetEnumerator() => new Enumerator(this._head);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

		/// <summary>
        /// An enumerator over <see cref="ImmutableLinkedList{T}"/>. See <see cref="ImmutableLinkedList{T}.GetEnumerator"/>
        /// for more details
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            private Node _next;

            internal Enumerator(Node first)
            {
                this._next = first;
                this.Current = default(T);
            }

			/// <summary>
            /// The current value. Behavior is undefined before enumeration begins
            /// and after it ends
            /// </summary>
            public T Current { get; private set; }

            object IEnumerator.Current => this.Current;

			/// <summary>
            /// Cleans up any resources held by the enumerator (currently none)
            /// </summary>
            public void Dispose() { }

			/// <summary>
            /// Advances the enumerator, returning false if the end of the list has been reached
            /// </summary>
            public bool MoveNext()
            {
                if (this._next == null)
                {
                    this.Current = default(T);
                    return false;
                }

                this.Current = this._next.Value;
                this._next = this._next.Next;
                return true;
            }

            void IEnumerator.Reset() => throw new NotSupportedException();
        }
    }
}

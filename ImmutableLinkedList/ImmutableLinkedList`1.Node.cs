using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Medallion.Collections
{
    public partial struct ImmutableLinkedList<T>
    {
        [DebuggerDisplay("Value = {Value}")]
        internal sealed class Node
        {
#if !INVARIANT_CHECKS
            internal Node? Next;
#else
            private Node? _next;
            private bool _frozen;

            internal Node? Next
            {
                get => this._next;
                set => this._next = this._frozen ? throw new InvalidOperationException("frozen") : value;
            }

            internal void Freeze()
            {
                if (!this._frozen)
                {
                    this.Next?.Freeze();
                    this._frozen = true;
                }
            }
#endif

            internal readonly T Value;

            public Node(T value)
            {
                this.Value = value;
            }
        }
    }
}

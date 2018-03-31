using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Medallion.Collections
{
    [DebuggerTypeProxy(typeof(ImmutableLinkedList<>.DebugView))]
    [DebuggerDisplay("Count = {Count}")]
    public partial struct ImmutableLinkedList<T>
    {
        /// <summary>
        /// Provides a cleaner view of the <see cref="ImmutableLinkedList{T}"/> in the debugger
        /// </summary>
        internal sealed class DebugView
        {
            private readonly ImmutableLinkedList<T> _list;

            public DebugView(ImmutableLinkedList<T> list)
            {
                this._list = list;
            }

            public int Count => this._list.Count;
            public object Head => this._list.Count > 0 ? (object)this._list.Head : "{list is empty}";
            public object Tail => this._list.Count > 0 ? (object)this._list.Tail : "{list is empty}";
            
            public T[] Items
            {
                get
                {
                    // note: this can't simply use ToArray() since that gives an error in the debugger
                    var array = new T[this._list._count];
                    this._list.CopyTo(array, arrayIndex: 0);
                    return array;
                }
            }
        }
    }
}

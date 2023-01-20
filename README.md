# ImmutableLinkedList

ImmutableLinkedList is a compact .NET implementation of the immutable singly-linked list datastructure which is frequently used in functional-style programming.

ImmutableLinkedList is available for download as a [NuGet package](https://www.nuget.org/packages/ImmutableLinkedList). [![NuGet Status](http://img.shields.io/nuget/v/ImmutableLinkedList.svg?style=flat)](https://www.nuget.org/packages/ImmutableLinkedList/)

[Release notes](#release-notes)

## Features

The package exposes two public types: `ImmutableLinkedList<T>` and the non-generic static helper type `ImmutableLinkedList`.

### Creating a list

Lists can be created by starting from the empty list or by using various factory methods on `ImmutableLinkedList`:

```C#
var emptyList = ImmutableLinkedList<string>.Empty;
var singletonList = ImmutableLinkedList.Create("a");
var listFromEnumerable1 = ImmutableLinkedList.CreateRange(new[] { "a", "b", "c" });
var listFromEnumerable2 = new[] { "a", "b", "c" }.ToImmutableLinkedList();
```

A list instance can be further built upon using the `Prepend(Range)` and `Append(Range)` methods. Because the list is immutable, these methods return new lists rather than modifying the existing ones. Also because of this, prepending is much faster than appending because it does not require copying the list structure. 

### Accessing list elements

Lists are `IEnumerable`, so they can be iterated over. However, the most common access pattern for this datastructure is to consider the `Head` (first item) and the `Tail` (List composed of all items but the first). There are a few APIs for this:

```C#
var list = ImmutableLinkedList.CreateRange(new[] { "a", "b", "c" });

// C# 7 tuple destructuring
var (head, tail) = list;

// individual properties
var head = list.Head;
var tail = list.Tail;

// the above methods throw when the list is empty. You can check for this first using
// list.Count or use the following
if (list.TryDeconstruct(out var head, out var tail))
{
    // do something with head and tail
}
```

### Implementation Notes

`ImmutableLinkedList<T>` is a light-weight value type which wraps an underlying list of nodes. This structure has several benefits:
* The wrapping value type holds the list count, allowing `list.Count` to run in constant time but avoiding the memory overhead of storing count with every list node
* Clean representation of the empty list

`ImmutableLinkedList<T>` implements the `IReadOnlyCollection<T>` interface as well as the read-only parts of the `ICollection<T>` interface.

As mentioned previously, because the datastructure is immutable many operations require copying the underlying list nodes in order to build a new list. For all list operations, the implementation is aggressive in avoiding such copies where they are not necessary, thus preserving maximal shared structure between the original list and the new list being constructed. As an example `Sort` will return the original list unchanged if it is already sorted.

Related to this, `ImmutableLinkedList<T>` has *reference equality* semantics (via the `IEquatable<T>` interface).

### Comparison to other implementations

The .NET core libraries offer two types that implement this datastructure:

`Microsoft.FSharp.Collections.FSharpList<T>` implements the core functional list type for the F# language. This supports many similar operations as `ImmutableLinkedList<T>` and is the clear choice for F# projects due to it's language integration. However, this type is less compelling for C# projects as it is rather awkward to use from C# because the methods are exposed via the separate `ListModule` class rather than being implemented on `FSharpList<T>` itself. Furthermore, using this type from a C# project means taking a dependency on the entire FSharp core DLL, which may be undesirable for some. Finally, `FSharpList<T>` does not offer constant-time `Count` and therefore does not implement `IReadOnlyCollection<T>`.

`System.Collections.Immutable.ImmutableStack<T>` from the `System.Collections.Immutable` package offers another implementation of the same basic datastructure (note that `System.Collections.Immutable.ImmutableList<T>` is a very different datastructure based on a binary tree which has very different performance characteristics). `ImmutableStack<T>` is a good choice if you want to use this datastructure to represent a stack. However, it is clumsy to use it as a more general collection because it only implements operations which are directly relevant to stacks. Furthermore, `ImmutableStack<T>` does not offer constant-time `Count` and therefore does not implement `IReadOnlyCollection<T>`.

### Other operations

In addition to the operations already mentioned, `ImmutableLinkedList<T>` offers the following: `Contains`, `Remove(value)`, `RemoveAt(index)`, `RemoveAll(predicate)`, `Skip(count)`, `SkipWhile(predicate)`, `SubList(startIndex, count)`, `Reverse`, and `Sort`.

## Release notes
- 1.0.0 Initial release
- 1.0.1 Compiled as a readonly struct and with C# 8 nullable reference type annotations

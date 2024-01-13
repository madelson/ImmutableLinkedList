using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections;

/// <summary>
/// Static factory methods and extensions for <see cref="ImmutableLinkedList{T}"/>
/// </summary>
public static class ImmutableLinkedList
{
    /// <summary>
    /// Creates an <see cref="ImmutableLinkedList{T}"/> with a single element <paramref name="value"/>.
    /// </summary>
    public static ImmutableLinkedList<T> Create<T>(T value) => ImmutableLinkedList<T>.Create(value);

    /// <summary>
    /// Creates an <see cref="ImmutableLinkedList{T}"/> with the given <paramref name="values"/>.
    /// </summary>
    public static ImmutableLinkedList<T> CreateRange<T>(ReadOnlySpan<T> values) => ImmutableLinkedList<T>.CreateRange(values);

    /// <summary>
    /// Creates an <see cref="ImmutableLinkedList{T}"/> with the given <paramref name="values"/>.
    /// </summary>
    public static ImmutableLinkedList<T> CreateRange<T>(IEnumerable<T> values) => ImmutableLinkedList<T>.CreateRange(values);
    
    /// <summary>
    /// Same as <see cref="CreateRange{T}(IEnumerable{T})"/>, but exposed as an extension method
    /// </summary>
    public static ImmutableLinkedList<T> ToImmutableLinkedList<T>(this IEnumerable<T> source) => CreateRange(source);
}

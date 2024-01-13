using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Medallion.Collections.Tests;

public class ImmutableLinkedListTest
{
    [Test]
    public void TestEmpty()
    {
        ImmutableLinkedList<int>.Empty.Count.ShouldEqual(0);
        CollectionAssert.IsEmpty(ImmutableLinkedList<string>.Empty);
        ImmutableLinkedList<double>.Empty.ShouldEqual(ImmutableLinkedList<double>.Empty);
        ImmutableLinkedList<short>.Empty.ShouldEqual(default);
    }

    [Test]
    public void TestCount()
    {
        ImmutableLinkedList.Create('a').Count.ShouldEqual(1);
        ImmutableLinkedList.CreateRange(new[] { true, false, true }).Count.ShouldEqual(3);
    }

    [Test]
    public void TestHead()
    {
        ImmutableLinkedList.Create(1).Head.ShouldEqual(1);
        ImmutableLinkedList.CreateRange(new[] { 3, 2, 1 }).Head.ShouldEqual(3);
        Assert.Throws<InvalidOperationException>(() => GC.KeepAlive(ImmutableLinkedList<string>.Empty.Head));
    }

    [Test]
    public void TestTail()
    {
        ImmutableLinkedList.Create(1).Tail.ShouldEqual(ImmutableLinkedList<int>.Empty);
        ImmutableLinkedList.CreateRange(new[] { 1, 2, 3 }).Tail.AsEnumerable()
            .SequenceEqual(new[] { 2, 3 })
            .ShouldEqual(true);
        Assert.Throws<InvalidOperationException>(() => GC.KeepAlive(ImmutableLinkedList<string>.Empty.Tail));

        var list = ImmutableLinkedList.CreateRange("abc");
        list.Tail.ShouldEqual(list.Tail);
    }

    [Test]
    public void TestCreate()
    {
        ImmutableLinkedList.Create('a').Count.ShouldEqual(1);
        ImmutableLinkedList.Create(true).Single().ShouldEqual(true);
        ImmutableLinkedList.Create(default(string)).Single().ShouldEqual(null);
    }

    [Test]
    public void TestCreateBuilder()
    {
        ImmutableLinkedList<string> strings = ["a", "b", "c"];
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, strings);
        strings = [];
        CollectionAssert.IsEmpty(strings);
    }

    [Test]
    public void TestCreateRange()
    {
        Assert.Throws<ArgumentNullException>(() => ImmutableLinkedList.CreateRange(default(IEnumerable<string>)));
        var list = ImmutableLinkedList.Create('a');
        ImmutableLinkedList.CreateRange(list).ShouldEqual(list);
        ImmutableLinkedList.CreateRange(Enumerable.Empty<string>()).ShouldEqual(ImmutableLinkedList<string>.Empty);
        ImmutableLinkedList.CreateRange(Enumerable.Repeat(new object(), 1000)).Count.ShouldEqual(1000);
        ImmutableLinkedList.CreateRange(Enumerable.Range(20, 30)).AsEnumerable()
            .SequenceEqual(Enumerable.Range(20, 30))
            .ShouldEqual(true);
    }

    [Test]
    public void TestToImmutableLinkedList()
    {
        Assert.Throws<ArgumentNullException>(() => default(IEnumerable<string>).ToImmutableLinkedList());
        var list = ImmutableLinkedList.Create('a');
        list.AsEnumerable().ToImmutableLinkedList().ShouldEqual(list);
        CollectionAssert.IsEmpty(Enumerable.Empty<string>().ToImmutableLinkedList());
        Enumerable.Repeat(new object(), 1000).ToImmutableLinkedList().Count.ShouldEqual(1000);
        Enumerable.Range(20, 30).ToImmutableLinkedList()
            .AsEnumerable()
            .SequenceEqual(Enumerable.Range(20, 30))
            .ShouldEqual(true);
    }

    [Test]
    public void TestDeconstruct()
    {
        var (head, tail) = ImmutableLinkedList.CreateRange(new[] { "a", "bb", "ccc" });
        head.ShouldEqual("a");
        string.Join(",", tail).ShouldEqual("bb,ccc");

        var (headX, tailX) = ImmutableLinkedList.Create('x');
        headX.ShouldEqual('x');
        CollectionAssert.IsEmpty(tailX);

        ImmutableLinkedList.CreateRange(new[] { 'y', 'z' }).Deconstruct(out var headY, out var tailZ);
        headY.ShouldEqual('y');
        tailZ.SequenceEqual(new[] { 'z' }).ShouldEqual(true);

        Assert.Throws<InvalidOperationException>(() => ImmutableLinkedList<int>.Empty.Deconstruct(out _, out _));
    }

    [Test]
    public void TestTryDeconstruct()
    {
        ImmutableLinkedList.CreateRange(new[] { "a", "bb", "ccc" }).TryDeconstruct(out var head, out var tail).ShouldEqual(true);
        head.ShouldEqual("a");
        string.Join(",", tail).ShouldEqual("bb,ccc");

        ImmutableLinkedList.Create('x').TryDeconstruct(out var headX, out var tailX).ShouldEqual(true);
        headX.ShouldEqual('x');
        CollectionAssert.IsEmpty(tailX);

        ImmutableLinkedList.CreateRange(new[] { 'y', 'z' }).TryDeconstruct(out var headY, out var tailZ).ShouldEqual(true);
        headY.ShouldEqual('y');
        tailZ.SequenceEqual(new[] { 'z' }).ShouldEqual(true);

        ImmutableLinkedList<int>.Empty.TryDeconstruct(out var noHead, out var noTail).ShouldEqual(false);
        noHead.ShouldEqual(default(int));
        noTail.ShouldEqual(default(ImmutableLinkedList<int>));
    }

    [Test]
    public void TestHasReferenceEqualitySemantics()
    {
        Equals(ImmutableLinkedList<string>.Empty, ImmutableLinkedList<string>.Empty).ShouldEqual(true);
        Equals(ImmutableLinkedList<string>.Empty, ImmutableLinkedList<object>.Empty).ShouldEqual(false);
        Equals(ImmutableLinkedList.Create(1), ImmutableLinkedList.Create(1)).ShouldEqual(false);
        var list = ImmutableLinkedList.Create(2);
        Equals(list, list).ShouldEqual(true);
    }

    [Test]
    public void TestEnumeration()
    {
        var list = ImmutableLinkedList.CreateRange(new[] { "a", "b", "c", "d" });
        var builder = new StringBuilder();
        foreach (var element in list)
        {
            builder.Append(',').Append(element);
        }
        builder.ToString().ShouldEqual(",a,b,c,d");

        var enumerator = list.GetEnumerator();
        enumerator.Current.ShouldEqual(null);
        Assert.Throws<NotSupportedException>(() => ((IEnumerator)enumerator).Reset());
        enumerator.MoveNext().ShouldEqual(true);
        enumerator.Current.ShouldEqual("a");
        var snapshot = enumerator;
        for (var i = 0; i < 3; ++i) { enumerator.MoveNext().ShouldEqual(true); }
        enumerator.MoveNext().ShouldEqual(false);
        enumerator.Current.ShouldEqual(null);
        enumerator.MoveNext().ShouldEqual(false);
        enumerator.Current.ShouldEqual(null);
        enumerator.Dispose();

        snapshot.Current.ShouldEqual("a");
        snapshot.MoveNext().ShouldEqual(true);
        snapshot.Current.ShouldEqual("b");
    }

    [Test]
    public void TestContains()
    {
        ImmutableLinkedList<int>.Empty.Contains(1).ShouldEqual(false);
        var range = new[] { 1, 2, 3, 4 }.ToImmutableLinkedList();
        for (var i = 0; i < 10; ++i)
        {
            range.Contains(i).ShouldEqual(i >= 1 && i <= 4);
        }
    }

    [Test]
    public void TestPrepend()
    {
        ImmutableLinkedList<int>.Empty.Prepend(1).Prepend(2).Prepend(3)
            .SequenceEqual(new[] { 3, 2, 1 })
            .ShouldEqual(true);

        var list = ImmutableLinkedList.CreateRange(new[] { 'a', 'b' });
        list.Prepend(' ').Tail.ShouldEqual(list);
    }

    [Test]
    public void TestPrependRange()
    {
        ImmutableLinkedList<byte>.Empty.PrependRange(new byte[] { 5, 6, 7 })
            .PrependRange(new byte[] { 2, 3, 4 })
            .SequenceEqual(new byte[] { 2, 3, 4, 5, 6, 7 });

        var list = ImmutableLinkedList.CreateRange(new[] { "a", "b" });
        list.PrependRange(new[] { "x", "y" }).Tail.Tail.ShouldEqual(list);
    }

    [Test]
    public void TestPrependImmutableLinkedList()
    {
        ImmutableLinkedList<byte>.Empty.PrependRange(new byte[] { 5, 6, 7 }.ToImmutableLinkedList())
            .PrependRange(new byte[] { 2, 3, 4 }.ToImmutableLinkedList())
            .SequenceEqual(new byte[] { 2, 3, 4, 5, 6, 7 });

        var list = ImmutableLinkedList.CreateRange(new[] { "a", "b" });
        list.PrependRange(new[] { "x", "y" }.ToImmutableLinkedList()).Tail.Tail.ShouldEqual(list);
    }

    [Test]
    public void TestAppend()
    {
        ImmutableLinkedList<int>.Empty.Append(1).Append(2).Append(3)
            .SequenceEqual(new[] { 1, 2, 3 })
            .ShouldEqual(true);
    }

    [Test]
    public void TestAppendRange()
    {
        ImmutableLinkedList<byte>.Empty.AppendRange(new byte[] { 5, 6, 7 })
            .AppendRange(new byte[] { 2, 3, 4 })
            .SequenceEqual(new byte[] { 5, 6, 7, 2, 3, 4 });
    }

    [Test]
    public void TestAppendImmutableLinkedList()
    {
        ImmutableLinkedList<byte>.Empty.AppendRange(new byte[] { 5, 6, 7 }.ToImmutableLinkedList())
            .AppendRange(new byte[] { 2, 3, 4 }.ToImmutableLinkedList())
            .SequenceEqual(new byte[] { 5, 6, 7, 2, 3, 4 });

        var list1 = ImmutableLinkedList.Create('x');
        var list2 = ImmutableLinkedList.Create('y');
        list1.AppendRange(list2).Tail.ShouldEqual(list2);
    }

    [Test]
    public void TestRemove()
    {
        CollectionAssert.IsEmpty(ImmutableLinkedList<int>.Empty.Remove(9));

        var list = Enumerable.Range(1, 10).Append(3).ToImmutableLinkedList();
        list.Remove(100).ShouldEqual(list);
        list.Remove(1).ShouldEqual(list.Tail);
        list.Remove(3).SequenceEqual(Enumerable.Range(1, 10).Where(i => i != 3).Append(3))
            .ShouldEqual(true);
        list.Remove(3).Remove(3).SequenceEqual(list.Where(i => i != 3)).ShouldEqual(true);
    }

    [Test]
    public void TestRemoveAt()
    {
        var list = new[] { "a", "b", "c" }.ToImmutableLinkedList();
        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(list.Count));
        list.RemoveAt(0).ShouldEqual(list.Tail);
        list.RemoveAt(1).SequenceEqual(new[] { "a", "c" }).ShouldEqual(true);
        list.RemoveAt(2).SequenceEqual(new[] { "a", "b" }).ShouldEqual(true);
    }

    [Test]
    public void TestRemoveAll()
    {
        Assert.Throws<ArgumentNullException>(() => ImmutableLinkedList<int>.Empty.RemoveAll(null));
        CollectionAssert.IsEmpty(ImmutableLinkedList<string>.Empty.RemoveAll(_ => true));
        var list = Enumerable.Range(0, 10).ToImmutableLinkedList();
        list.RemoveAll(i => i == 100).ShouldEqual(list);
        list.RemoveAll(i => i < 3).ShouldEqual(list.Tail.Tail.Tail);
        list.RemoveAll(i => i >= 5).SequenceEqual(Enumerable.Range(1, 5));
        list.RemoveAll(i => i % 3 != 0).SequenceEqual(new[] { 0, 3, 6, 9 });
        CollectionAssert.IsEmpty(list.RemoveAll(_ => true));
    }

    [Test]
    public void TestSkip()
    {
        // Enumerable.Skip behavior we're trying to match
        new[] { 1 }.Skip(-1).SequenceEqual(new[] { 1 }).ShouldEqual(true);
        new[] { 1 }.Skip(2).SequenceEqual(Array.Empty<int>()).ShouldEqual(true);

        var list = ImmutableLinkedList.Create(1);
        list.Skip(-1).ShouldEqual(list);
        CollectionAssert.IsEmpty(list.Skip(2));

        var list2 = "abcd".ToImmutableLinkedList();
        list2.Skip(2).ShouldEqual(list2.Tail.Tail);
    }

    [Test]
    public void TestSkipWhile()
    {
        Assert.Throws<ArgumentNullException>(() => ImmutableLinkedList<char>.Empty.SkipWhile(default(Func<char, bool>)));
        CollectionAssert.IsEmpty(ImmutableLinkedList<char>.Empty.SkipWhile(_ => true));

        var list = new[] { 10, 15, 20, 25 }.ToImmutableLinkedList();
        list.SkipWhile(_ => false).ShouldEqual(list);
        list.SkipWhile(i => i.ToString().EndsWith("0")).ShouldEqual(list.Tail);
        list.SkipWhile(i => i < 20).ShouldEqual(list.Tail.Tail);
        CollectionAssert.IsEmpty(list.SkipWhile(_ => true));
    }

    [Test]
    public void TestSkipWhileWithIndex()
    {
        Assert.Throws<ArgumentNullException>(() => ImmutableLinkedList<char>.Empty.SkipWhile(default(Func<char, int, bool>)));
        CollectionAssert.IsEmpty(ImmutableLinkedList<char>.Empty.SkipWhile((_, index) => true));

        var list = new[] { 10, 15, 20, 25 }.ToImmutableLinkedList();
        list.SkipWhile((_, index) => false).ShouldEqual(list);
        list.SkipWhile((i, index) => index < 1).ShouldEqual(list.Tail);
        list.SkipWhile((i, index) => i == 10 || i == 25 || index == 1).ShouldEqual(list.Tail.Tail);
        CollectionAssert.IsEmpty(list.SkipWhile((_, index) => true));
    }

    [Test]
    public void TestSequenceEqual()
    {
        var comparer = new CountingEqualityComparer<int>();
        ImmutableLinkedList<int>.Empty.SequenceEqual(ImmutableLinkedList<int>.Empty, comparer).ShouldEqual(true);
        comparer.EqualsCount.ShouldEqual(0);

        comparer = new CountingEqualityComparer<int>();
        Enumerable.Range(1, 1000).ToImmutableLinkedList().SequenceEqual(Enumerable.Range(1, 1001).ToImmutableLinkedList(), comparer)
            .ShouldEqual(false);
        comparer.EqualsCount.ShouldEqual(0);

        var list = Enumerable.Range(0, 500).ToImmutableLinkedList();
        comparer = new CountingEqualityComparer<int>();
        list.SequenceEqual(list, comparer).ShouldEqual(true);
        comparer.EqualsCount.ShouldEqual(0);

        comparer = new CountingEqualityComparer<int>();
        list.SequenceEqual(list.ToArray().ToImmutableLinkedList(), comparer).ShouldEqual(true);
        comparer.EqualsCount.ShouldEqual(list.Count);

        comparer = new CountingEqualityComparer<int>();
        list.PrependRange(new[] { 5, 4, 3, 2, 1 })
            .SequenceEqual(list.PrependRange(new[] { 5, 4, 3, 2, 1 }), comparer)
            .ShouldEqual(true);
        comparer.EqualsCount.ShouldEqual(5);

        comparer = new CountingEqualityComparer<int>();
        list.Select(i => i == 200 ? 2000 : i).ToImmutableLinkedList()
            .SequenceEqual(list, comparer)
            .ShouldEqual(false);
        comparer.EqualsCount.ShouldEqual(201);
    }

    [Test]
    public void TestSubList()
    {
        var list = "abcdefghijklmnopqrstuvwxyz".ToImmutableLinkedList();

        Assert.Throws<ArgumentOutOfRangeException>(() => list.SubList(-1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.SubList(0, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.SubList(0, list.Count + 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.SubList(list.Count + 1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.SubList(15, 12));

        list.SubList(7, 0).ShouldEqual(ImmutableLinkedList<char>.Empty);
        list.SubList(0, list.Count).ShouldEqual(list);
        string.Join(string.Empty, list.SubList(3, 5)).ShouldEqual("defgh");
        list.SubList(20, 6).ShouldEqual(list.Skip(20));
    }

    [Test]
    public void TestReverse()
    {
        CollectionAssert.IsEmpty(ImmutableLinkedList<char>.Empty.Reverse());
        var singleton = ImmutableLinkedList.Create('a');
        singleton.Reverse().ShouldEqual(singleton);

        "abcdef".ToImmutableLinkedList().Reverse()
            .SequenceEqual(new[] { 'f', 'e', 'd', 'c', 'b', 'a' })
            .ShouldEqual(true);
    }

    [Test]
    public void TestImplicitlyImplementedICollectionMethods()
    {
        ICollection<string> collection = new[] { "a", "b" }.ToImmutableLinkedList();
        collection.IsReadOnly.ShouldEqual(true);
        Assert.Throws<NotSupportedException>(() => collection.Add("c"));
        Assert.Throws<NotSupportedException>(() => collection.Clear());
        Assert.Throws<NotSupportedException>(() => collection.Remove("c"));
    }

    [Test]
    public void TestCopyTo()
    {
        var list = new[] { 10, 12, 14 }.ToImmutableLinkedList();
        Assert.Throws<ArgumentNullException>(() => list.CopyTo(null, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(new int[10], -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(new int[10], 11));
        Assert.Throws<ArgumentException>(() => list.CopyTo(new int[5], 3));

        var array = Enumerable.Range(0, 7).ToArray();
        list.CopyTo(array, 3);
        array.SequenceEqual(new[] { 0, 1, 2, 10, 12, 14, 6 }).ShouldEqual(true);

        Assert.DoesNotThrow(() => ImmutableLinkedList<string>.Empty.CopyTo(new string[0], 0));
        Assert.DoesNotThrow(() => ImmutableLinkedList<string>.Empty.CopyTo(new string[10], 10));
    }

    [Test]
    public void TestDebugView()
    {
        var list = Enumerable.Range(1, 3).ToImmutableLinkedList();
        var debugView = new ImmutableLinkedList<int>.DebugView(list);
        debugView.Items.SequenceEqual(list).ShouldEqual(true);
        debugView.Count.ShouldEqual(list.Count);
        debugView.Head.ShouldEqual(list.Head);
        debugView.Tail.ShouldEqual(list.Tail);

        var emptyDebugView = new ImmutableLinkedList<int>.DebugView(ImmutableLinkedList<int>.Empty);
        CollectionAssert.IsEmpty(emptyDebugView.Items);
        emptyDebugView.Count.ShouldEqual(0);
        emptyDebugView.Head.ShouldEqual("{list is empty}");
        emptyDebugView.Tail.ShouldEqual("{list is empty}");
    }

    [Test]
    public void TestSort()
    {
        CollectionAssert.IsEmpty(ImmutableLinkedList<string>.Empty.Sort());
        var singleton = ImmutableLinkedList.Create("a");
        singleton.Sort().ShouldEqual(singleton);

        var shortList = new[] { 3, 1, 4, 2, 5 }.ToImmutableLinkedList();
        var sortedShortList = shortList.Sort();
        sortedShortList.SequenceEqual(shortList.OrderBy(x => x)).ShouldEqual(true);
        sortedShortList.Skip(4).ShouldEqual(shortList.Skip(4));

        var longAlreadySorted = Enumerable.Range(0, 100).ToImmutableLinkedList();
        longAlreadySorted.Sort().ShouldEqual(longAlreadySorted);

        var tailAlreadySorted = Enumerable.Range(0, 100).Reverse()
            .Concat(Enumerable.Range(200, 1000))
            .ToImmutableLinkedList();
        tailAlreadySorted.Sort().SequenceEqual(tailAlreadySorted.OrderBy(i => i)).ShouldEqual(true);
        tailAlreadySorted.Sort().Skip(100).ShouldEqual(tailAlreadySorted.Skip(100));
    }

    [Test]
    public void TestSortWithCustomComparer()
    {
        CollectionAssert.AreEqual(
            actual: new[] { "xx", "a", "yy", "X", "AA", "Y", "x" }.ToImmutableLinkedList().Sort(StringComparer.OrdinalIgnoreCase),
            expected: new[] { "a", "AA", "X", "x", "xx", "Y", "yy" }
        );

        // stability
        var lastDigitComparer = Comparer<int>.Create((a, b) => (a % 10).CompareTo(b % 10));
        var sortedByLastDigit = new[] { 1, 20, 23, 3, 33, 11, 21, 31, 41, 0, 4 }.ToImmutableLinkedList().Sort(lastDigitComparer);
        CollectionAssert.AreEqual(actual: sortedByLastDigit, expected:  new[] { 20, 0, 1, 11, 21, 31, 41, 23, 3, 33, 4 });
    }

    [Test]
    public void TestSortPerformance()
    {
        const int Count = 10000;

        var comparer = new CountingComparer<int>();
        var alreadySorted = Enumerable.Range(0, Count).ToImmutableLinkedList();
        alreadySorted.Sort(comparer).ShouldEqual(alreadySorted);
        comparer.CompareCount.ShouldEqual(Count - 1, "already sorted");

        comparer = new CountingComparer<int>();
        var reverseSorted = alreadySorted.Reverse();
        alreadySorted.Sort(comparer).SequenceEqual(alreadySorted).ShouldEqual(true);
        comparer.CompareCount.ShouldEqual(Count - 1, "reverse sorted");

        comparer = new CountingComparer<int>();
        var allEqual = Enumerable.Repeat(10, Count).ToImmutableLinkedList();
        allEqual.Sort(comparer).ShouldEqual(allEqual);
        comparer.CompareCount.ShouldEqual(Count - 1, "all equal");

        comparer = new CountingComparer<int>();
        var oneElementOutOfPlace = Enumerable.Range(1, Count - 1).Append(0).ToImmutableLinkedList();
        oneElementOutOfPlace.Sort(comparer).SequenceEqual(alreadySorted).ShouldEqual(true);
        Assert.That(comparer.CompareCount, Is.LessThan(3 * Count), "one out of place");

        comparer = new CountingComparer<int>();
        var randomlyOrdered = Enumerable.Range(0, Count).OrderBy(i => new Random(i).Next()).ToImmutableLinkedList();
        randomlyOrdered.Sort(comparer).SequenceEqual(alreadySorted).ShouldEqual(true);
        Assert.That(comparer.CompareCount, Is.LessThanOrEqualTo((int)Math.Ceiling(Count * Math.Log(Count, 2))), "random");
    }

    [Test]
    public void FuzzTestSort()
    {
        var random = new Random(12345);

        for (var i = 0; i < 1000; ++i)
        {
            var max = random.Next(1000);
            var values = Enumerable.Range(0, random.Next(0, 1000))
                .Select(_ => random.Next(max))
                .ToArray();
            values.ToImmutableLinkedList().Sort()
                .SequenceEqual(values.OrderBy(x => x))
                .ShouldEqual(true);
        }
    }

    [Test]
    public void FuzzTestSortPreservesSortedTails()
    {
        var random = new Random(54321);

        for (var i = 0; i < 1000; ++i)
        {
            var count = random.Next(2, 2000);
            var tailCount = random.Next(1, count);
            var values = Enumerable.Range(0, count - tailCount)
                .Select(_ => random.Next(count))
                .Concat(Enumerable.Range(count, tailCount))
                .ToImmutableLinkedList();
            var sortedValues = values.Sort();
            sortedValues.SequenceEqual(values.OrderBy(x => x)).ShouldEqual(true);
            values.Skip(count - tailCount).ShouldEqual(sortedValues.Skip(count - tailCount));
        }
    }

    /// <summary>
    /// Verifies that we follow the same optimization as .NET 8 collections regarding
    /// the empty boxed enumerator.
    /// </summary>
    [Test]
    public void TestEmptyEnumeratorThroughInterfaceDoesNotAllocate()
    {
        // warm up
        IEnumerable<string> empty = ImmutableLinkedList<string>.Empty;
        var emptyEnumerator = empty.GetEnumerator();

        Assert.That(emptyEnumerator, Is.SameAs(empty.GetEnumerator()));

#if NETCOREAPP
        // see https://github.com/dotnet/runtime/issues/96836#issuecomment-1889153868
        [MethodImpl(MethodImplOptions.NoInlining)]
        static long MeasureBytes(IEnumerable<string> empty)
        {
            var bytes = GC.GetAllocatedBytesForCurrentThread();
            empty.GetEnumerator();
            return GC.GetAllocatedBytesForCurrentThread() - bytes;
        }
        
        Assert.That(MeasureBytes(empty), Is.EqualTo(0));
#endif
    }
}

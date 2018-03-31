﻿using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medallion.Collections.Tests
{
    public class ImmutableLinkedListTest
    {
        [Test]
        public void TestEmpty()
        {
            ImmutableLinkedList<int>.Empty.Count.ShouldEqual(0);
            Assert.IsEmpty(ImmutableLinkedList<string>.Empty);
            ImmutableLinkedList<double>.Empty.ShouldEqual(ImmutableLinkedList<double>.Empty);
            ImmutableLinkedList<short>.Empty.ShouldEqual(default(ImmutableLinkedList<short>));
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
            Assert.IsEmpty(Enumerable.Empty<string>().ToImmutableLinkedList());
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
            Assert.IsEmpty(tailX);

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
            Assert.IsEmpty(tailX);

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
    }
}
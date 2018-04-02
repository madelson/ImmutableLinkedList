using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    public partial struct ImmutableLinkedList<T>
    {
        /// <summary>
        /// Returns a new list with the elements sorted. The sort is stable.
        /// 
        /// This method uses the merge sort algorithm and is O(nlg(n)). Elements are only copied as
        /// needed. For example, if a trailing portion of the original list remains unchanged in
        /// the sorted output, those elements will not be copied. As an extension of this, calling
        /// this method on an already-sorted list results in no copying (the original list is returned).
        /// </summary>
        public ImmutableLinkedList<T> Sort(IComparer<T> comparer = null)
        {
            if (this._count < 2) { return this; }

            SortHelper(this._head, this._count, comparer ?? Comparer<T>.Default, out var sorted, out var next);
#if INVARIANT_CHECKS
            if (next != null) { throw new InvalidOperationException("sanity check"); }
#endif

            return new ImmutableLinkedList<T>(sorted.First, this._count);
        }

        private static void SortHelper(
            Node head,
            int count,
            IComparer<T> comparer,
            out SortedSegment sorted,
            out Node next)
        {
            // base case
            if (count == 1)
            {
                sorted = new SortedSegment { First = head, LastCopied = null, Last = head };
                next = head.Next;
                return;
            }

            // sort the first half
            var firstHalfCount = count >> 1;
            SortHelper(head, firstHalfCount, comparer, out var sortedFirstHalf, out var secondHalfHead);

            // sort the second half
            SortHelper(secondHalfHead, count - firstHalfCount, comparer, out var sortedSecondHalf, out next);
            
            // merge

            // see if we can short-circuit. Don't bother when count is small, since then just merging is equally cheap
            if (TryShortCircuitMerge(ref sortedFirstHalf, ref sortedSecondHalf, count, comparer, out sorted))
            {
                return;
            }

            // full-on merge
            Merge(ref sortedFirstHalf, ref sortedSecondHalf, comparer, out sorted);
        }
        
        /// <summary>
        /// An optimistic version of <see cref="Merge(ref SortedSegment, ref SortedSegment, IComparer{T}, out SortedSegment)"/> which
        /// can skip most of the work in the case where one segment goes entirely before the other. This allows us to achieve O(n)
        /// performance for mostly-sorted or mostly-reverse sorted input and generally take advantage of existing sorted subsequences
        /// in the data.
        /// </summary>
        private static bool TryShortCircuitMerge(
            // passed by ref to avoid copying
            ref SortedSegment segment1,
            ref SortedSegment segment2,
            int count,
            IComparer<T> comparer,
            out SortedSegment merged)
        {
            // see if the segments are already in order
            if (comparer.Compare(segment1.Last.Value, segment2.First.Value) <= 0)
            {
                if (segment1.Last.Next != segment2.First)
                {
                    // we only have to linke the two if they aren't already linked
                    EnsureFullyCopied(ref segment1);
                    segment1.Last.Next = segment2.First;
                }
                merged = new SortedSegment { First = segment1.First, LastCopied = segment2.LastCopied, Last = segment2.Last };
                return true;
            }

            // see if all of second goes before all of first (always true if count is 2)
            if (count == 2 || comparer.Compare(segment2.Last.Value, segment1.First.Value) <= 0)
            {
                // since we're doing a swap, both segments must be fully copied
                EnsureFullyCopied(ref segment1);
                EnsureFullyCopied(ref segment2);
                segment2.Last.Next = segment1.First;
                merged = new SortedSegment { First = segment2.First, Last = segment1.Last, LastCopied = segment1.Last };
                return true;
            }

            merged = default(SortedSegment);
            return false;
        }

        /// <summary>
        /// The standard merge algorithm, adjusted to manage proper copying. Assumes that
        /// <see cref="TryShortCircuitMerge(ref SortedSegment, ref SortedSegment, int, IComparer{T}, out SortedSegment)"/>
        /// has already been called and returned false.
        /// </summary>
        private static void Merge(
            // passed by ref to avoid copying
            ref SortedSegment segment1,
            ref SortedSegment segment2,
            IComparer<T> comparer,
            out SortedSegment merged)
        {
            // if we need to do a full merge, then no nodes in segment1 are retaining
            // their positions. Therefore segment1 must be fully copied
            EnsureFullyCopied(ref segment1);
            var next1 = segment1.First;
            var next2 = segment2.First;
            var isNext2Copied = segment2.LastCopied != null;
            merged = default(SortedSegment);
            do
            {
                if (comparer.Compare(next1.Value, next2.Value) <= 0)
                {
                    merged.Last = merged.First == null
                        ? merged.First = next1
                        : merged.Last.Next = next1;
                    next1 = next1 == segment1.Last ? null : next1.Next;
                }
                else
                {
                    // this loop runs so long as there are nodes remaining in both 1 and 2.
                    // In that case, any node added as part of this loop will be followed by
                    // at least one node from the other segment. Therefore, only copies may
                    // be added

                    Node copiedNext2;
                    if (isNext2Copied)
                    {
                        copiedNext2 = next2;
                        if (next2 == segment2.LastCopied)
                        {
                            isNext2Copied = false;
                        }
                    }
                    else
                    {
                        copiedNext2 = new Node(next2.Value);
                    }

                    merged.Last = merged.First == null
                        ? merged.First = copiedNext2
                        : merged.Last.Next = copiedNext2;
                    next2 = next2 == segment2.Last ? null : next2.Next;
                }
            }
            while (next1 != null && next2 != null);
            
            // add the remainder
            if (next1 == null)
            {
                // the remainder is the rest of segment2

                // if we are still in the copied region of segment2, then the last
                // copied node is the last copied node from segment 2 (which we haven't reached).
                // Otherwise, the last copied node is simply the last node added in the loop, since
                // only copies are added there
                merged.LastCopied = isNext2Copied ? segment2.LastCopied : merged.Last;
                merged.Last.Next = next2;
                merged.Last = segment2.Last;
            }
            else
            {
                // the remainder is the rest of segment1
                merged.Last.Next = next1;
                // In this case, the last copied node is the last node in merged since all of segment1 has been copied
                merged.LastCopied = merged.Last = segment1.Last;
            }
        }

        private static void EnsureFullyCopied(ref SortedSegment segment)
        {
            if (segment.LastCopied != segment.Last)
            {
                FullyCopy(ref segment);
            }
        }

        private static void FullyCopy(ref SortedSegment segment)
        {
            if (segment.LastCopied == null)
            {
                CopyNonEmptyRange(segment.First, segment.Last.Next, out segment.First, out segment.Last);
            }
            else
            {
                CopyNonEmptyRange(segment.LastCopied.Next, segment.Last.Next, out var firstCopied, out segment.Last);
                segment.LastCopied.Next = firstCopied;
            }
            segment.LastCopied = segment.Last;
        }

        private struct SortedSegment
        {
            public Node First, LastCopied, Last;
        }
    }
}

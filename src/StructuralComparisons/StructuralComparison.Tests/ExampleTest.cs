using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using FsCheck.NUnit;
using Sellars.Collections.Structural;

namespace StructuralComparison.Tests
{
    using Property = FsCheck.NUnit.PropertyAttribute;

    [TestFixture]
    public class ExampleTest
    {
        [Property]
        public bool RevRev(int[] xs) =>
            xs.Reverse().Reverse().SequenceEqual(xs);

        [Property]
        public bool NotStructurablyEquatable(int[] xs)
        {
            var set = new[] { xs.ToImmutableArray() }.ToHashSet();
            return xs.Length == 0 || !set.Contains(xs.ToImmutableArray());
        }

        [Property]
        public bool HasConsistentHashCodeWhenConstructedWithTheSameElements(int[] xs) =>
            xs.ToImmutableHashSet().ToStructural().GetHashCode() ==
                xs.ToImmutableHashSet().ToStructural().GetHashCode();

        [Property]
        public bool UnionHasConsistentHashCode(int[] xs, int[] ys, byte extraTimes) =>
            xs.ToImmutableHashSet().ToStructural().Union(Enumerable.Repeat(ys, extraTimes + 1).SelectMany(ys1 => ys1)).GetHashCode() ==
            ImmutableHashSet<int>.Empty.ToStructural().Union(ys).Union(xs).GetHashCode();

        [Property]
        public bool IntersectHasConsistentHashCode(int[] xs, int[] ys, byte extraTimes) =>
            xs.ToImmutableHashSet().ToStructural().Intersect(Enumerable.Repeat(ys, extraTimes + 1).SelectMany(ys1 => ys1)).GetHashCode() ==
            xs.ToImmutableHashSet().ToStructural().Intersect(ys).GetHashCode();

        [Property]
        public void IntersectHasSameElements(int[] xs, int[] ys, byte extraTimes)
        {
            var a = xs.ToImmutableHashSet().ToStructural().Intersect(Enumerable.Repeat(ys, extraTimes + 1).SelectMany(ys1 => ys1));
            var b = xs.ToImmutableHashSet().ToStructural().Intersect(ys);
            Assert.That(a, Is.EquivalentTo(xs.Union(ys)));
            Assert.That(a.SymmetricExcept(b), Is.Empty);
            Assert.That(a, Is.EquivalentTo(b));
        }

        [Property]
        public void UnionHasSameElements(int[] xs, int[] ys, byte extraTimes)
        {
            var a = xs.ToImmutableHashSet().ToStructural().Intersect(Enumerable.Repeat(ys, extraTimes + 1).SelectMany(ys1 => ys1));
            var b = xs.ToImmutableHashSet().ToStructural().Intersect(ys);
            Assert.That(a, Is.EquivalentTo(xs.Intersect(ys)));
            Assert.That(a.SymmetricExcept(b), Is.Empty);
            Assert.That(a, Is.EquivalentTo(b));
        }

        [Property]
        public void InterestingHashCode(int[] xs) =>
            Assert.That(xs.ToImmutableHashSet().ToStructural().GetHashCode(), Is.Not.Zero);
    }
}

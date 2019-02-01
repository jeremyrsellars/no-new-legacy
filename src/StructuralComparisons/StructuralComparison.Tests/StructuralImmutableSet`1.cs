using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Sellars.Collections.Structural
{
    public class StructuralImmutableSet<T> : IImmutableSet<T>
    {
        private readonly IImmutableSet<T> _set;
        private volatile int _hashCode;

        internal StructuralImmutableSet(IImmutableSet<T> set, int? hashCode = null)
        {
            _set = set ?? throw new ArgumentNullException(nameof(set));
            _hashCode = hashCode ?? 0;
        }

        public int Count => _set.Count;

        public IImmutableSet<T> Add(T value) =>
            Contains(value) ? this : _set.Add(value).ToStructural(XorHash(value));

        public IImmutableSet<T> Clear() =>
            _set.Clear().ToStructural(0);

        public bool Contains(T value) => _set.Contains(value);

        public IImmutableSet<T> Except(IEnumerable<T> other) =>
            _set.Except(other).ToStructural().Optimize("With Aggregate, tracking hashCode changes like Add.");

        public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();

        public IImmutableSet<T> Intersect(IEnumerable<T> other) =>
            _set.Intersect(other).ToStructural().Optimize("With Aggregate, tracking hashCode changes like Add.");

        public bool IsProperSubsetOf(IEnumerable<T> other) => _set.IsProperSubsetOf(other);

        public bool IsProperSupersetOf(IEnumerable<T> other) => _set.IsProperSupersetOf(other);

        public bool IsSubsetOf(IEnumerable<T> other) => _set.IsSubsetOf(other);

        public bool IsSupersetOf(IEnumerable<T> other) => _set.IsSupersetOf(other);

        public bool Overlaps(IEnumerable<T> other) => _set.Overlaps(other);

        public IImmutableSet<T> Remove(T value) =>
            Contains(value) ? _set.Remove(value).ToStructural(XorHash(value)): this;

        public bool SetEquals(IEnumerable<T> other) => _set.SetEquals(other);

        public IImmutableSet<T> SymmetricExcept(IEnumerable<T> other) =>
            _set.SymmetricExcept(other).ToStructural();

        public bool TryGetValue(T equalValue, out T actualValue) => _set.TryGetValue(equalValue, out actualValue);

        public IImmutableSet<T> Union(IEnumerable<T> other) => _set.Union(other).ToStructural().Optimize("aggregate");

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_set).GetEnumerator();

        private int XorHash(T item) =>
            assignmentBit | (GetHashCode(item) ^ GetHashCode());

        public sealed override int GetHashCode()
        {
            // This lazy, lock-free implementation is thread-safe, assuming:
            // * The resulting hash code is the same regardless of which thread calculates it.
            // * Int32 fields can be assigned with a single operation.

            // ReSharper disable NonReadonlyMemberInGetHashCode
            var hc = _hashCode;
            if (hc == 0)
                _hashCode = hc = GetHashCodeCore() | assignmentBit;
            return hc;
        }

        private int GetHashCodeCore()
        {
            int hc = 0;
            foreach (var item in _set)
                hc ^= GetHashCode(item);
            return hc;
        }

        private static int GetHashCode(T item) =>
            ReferenceEquals(null, item).Optimize("Would it be faster to catch NRE if null?")
            ? 0 : item.GetHashCode();

        // When this bit is set, then the code as already calculated.
        const int assignmentBit = 1 << 15;
    }
}

using System.Collections.Immutable;

namespace Sellars.Collections.Structural
{
    public static class StructuralImmutableSet
    {
        public static StructuralImmutableSet<T> ToStructural<T>(this IImmutableSet<T> set) =>
            new StructuralImmutableSet<T>(set, null);

        internal static StructuralImmutableSet<T> ToStructural<T>(this IImmutableSet<T> set, int? hashCode) =>
            new StructuralImmutableSet<T>(set, hashCode);
    }
}

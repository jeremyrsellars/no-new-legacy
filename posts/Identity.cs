    public class Identity<TSelf, TData> : IEquatable<TSelf>
        where TSelf : Identity<TSelf, TData>
        where TData : IEquatable<TData>
    {
        private readonly TData _value;

        public Identity(TData value)
        {
            if (ReferenceEquals(value, null))
                throw new ArgumentNullException(nameof(value));

            _value = value;
        }

        public virtual bool Equals(TSelf other) =>
            other != null
            && Value.Equals(other.Value);

        public override bool Equals(object other) => Equals(other as TSelf);

        public sealed override int GetHashCode()
        {
            // GetHashCode is effectively a 31-bit hashcode
            // calculated from the Value, by default.

            // When this bit is set, then the code as already calculated.
            const int assignmentBit = 1 << 15;

            // This lazy, lock-free implementation is thread-safe, assuming:
            // * The resulting hash code is the same regardless of which thread calculates it.
            // * Int32 fields can be assigned with a single operation.

            // ReSharper disable NonReadonlyMemberInGetHashCode
            var hc = _hashCode;
            if (hc == 0)
                _hashCode = hc = GetHashCodeCore() | assignmentBit;
            return hc;
        }

        protected virtual int GetHashCodeCore() => Value.GetHashCode();

        public override string ToString() =>
            $"{ShortTypeName}[{Value}]";

        protected TData Value => _value;

        [NonSerialized]
        private int _hashCode;

        protected static string ShortTypeName { get; } = typeof(TSelf).Name;
    }

    public class Identity<TSelf, TData1, TData2> : Identity<TSelf, TData1>
        where TSelf : Identity<TSelf, TData1, TData2>
        where TData1 : IEquatable<TData1>
        where TData2 : IEquatable<TData2>
    {
        private readonly TData2 _value2;

        public Identity(TData1 value, TData2 value2)
            : base(value)
        {
            if (ReferenceEquals(value2, null))
                throw new ArgumentNullException(nameof(value2));
            _value2 = value2;
        }

        protected TData2 Value2 => _value2;

        public override string ToString() =>
            $"{ShortTypeName}[{Value},{Value2}]";

        public override bool Equals(TSelf other) =>
            other != null
            && Value.Equals(other.Value)
            && Value2.Equals(other.Value2);

        protected override int GetHashCodeCore()
        {
            var hash = 17;
            hash = hash * 23 + Value.GetHashCode();
            hash = hash * 23 + Value2.GetHashCode();
            return hash;
        }
    }

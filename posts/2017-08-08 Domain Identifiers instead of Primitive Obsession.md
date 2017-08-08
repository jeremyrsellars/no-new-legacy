---
title: "Domain Identifiers instead of 'Primitive Obsession'"
date: "2017-08-08"
tags: ["domain design"]
categories : ["Programming"]
---

## Identity

Here's a class that can make it easy to make Domain Identifiers instead of having "[Primitive Obsession](http://wiki.c2.com/?PrimitiveObsession)"

This is part of an attempt to use Domain Values and Identities instead of primitives like strings to make member signatures more informative.

It also attempts to reduce tedious-to-read and easy-to-screw-up boilerplate code.

```c-sharp
    // Extend this to turn a primitive into a domain entity identifier
    public class Identity<TSelf, TData> : IEquatable<TSelf>
        where TSelf : Identity<TSelf, TData>
        where TData : IEquatable<TData>
    {
        public Identity(TData value)
        {
            if (ReferenceEquals(value, null))
                throw new ArgumentNullException(nameof(value));

            _value = value;
        }

        public virtual bool Equals(TSelf other); // elided

        public override bool Equals(object other) => Equals(other as TSelf);

        public sealed override int GetHashCode(); // implemented correctly and cached immutably.

        protected virtual int GetHashCodeCore() => Value.GetHashCode();

        public override string ToString(); elided

        // Value non-public so you can use a name that makes sense for your domain
        protected TData Value => _value;
    }

    // Extend this to make a composite key for a domain entity
    public class Identity<TSelf, TData1, TData2> : Identity<TSelf, TData1>
        where TSelf : Identity<TSelf, TData1, TData2>
        where TData1 : IEquatable<TData1>
        where TData2 : IEquatable<TData2>
    {
        // elided
    }
```

See [full source](#identity.cs) below.

> Let's break it down.

1. It introduces the semantic "Identity"
  * An atomic "value/equatable" reference type that can be used in (nearly) any idiomatic C# way to identify an entity.
  * Implementations should be immutable.
  * Note, unlike System.Tuple`1, this doesn't publicly expose the id field, implying that the extending class gets to use a name well-suited to the problem domain (instead of the inheritance hierarchy.)
2. It introduces the semantic "Identified" (types which extend Identified) which 
  * A class that provides minimalist `GetHashCode` and `ToString` implementations.
  * Implementations should be immutable.
  * Note, unlike System.Tuple`1, this doesn't publicly expose the id field, implying that the extending class gets to use a name well-suited to the problem domain (instead of the inheritance hierarchy.)
  * Note, we may wish to introduce a `public interface IIdentified<TIdentity> {TIdentity Identity {get;}}` which Identified could implement privately.
3. It introduces the semantic "IdentifiedValue" (types which extend IdentifiedValue)
  * A class that provides minimalist `GetHashCode` and `ToString` implementations and forces inheritor to implement an `Equals(T other)` which implies thorough value/equality semantics.
  * Implementations should be immutable.

Ramifications:

1. Identities can be used safely and consistently in common idioms:
  * `Hashtable`
  * `Dictionary<K,V>`
  * `HashSet<K>`
  * `object a,b; a == b`;

2. Entity "Values" can implement structural equality once in `EqualsCore` and benefit from "better than System.Object" default implementations from `GetHashCode`, `Equals`, and `ToString()`.

3. Entity objects benefit from "better than System.Object" default implementations of `GetHashCode`, and `ToString()`, and use `Object.Equals(object)` by default.

4. The inheritor gets to pick the name of the members (identity is protected, not public).


# Full Source

<a id="identity.cs"/>

```c-sharp
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
```

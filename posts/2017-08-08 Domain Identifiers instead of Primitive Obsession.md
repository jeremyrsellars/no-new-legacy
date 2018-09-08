---
title: "Domain Identifiers instead of 'Primitive Obsession'"
date: "2017-08-08"
revised: "2018-09-07"
description: In strongly-typed programming paradigms, consider introducing domain identifier types instead of using primitives like strings.
tags: ["domain design", "code smell"]
categories : ["Programming"]
---

#### Summary

In strongly-typed programming paradigms, consider introducing domain identifier types instead of using primitives like strings.

### The Problem

> "[Primitive Obsession](http://wiki.c2.com/?PrimitiveObsession)" is using primitive data types to represent domain ideas.
>
> – From Ward Cunningham's wiki.

In strongly-typed paradigms, this can be considered a "code smell".  Of course, there are other paradigms and contexts, but in this post, I'll share an Object-Oriented opinion and assume we are discussing how to use a strongly-typed language, like C# or Java, to model the problem domain in "plain old C# objects."

There are some reasons to consider avoiding the use of primitive types, but first, what's a primitive type?

### What is a primitive type?

A "primitive" is one of the building blocks your language provides.  This is mostly just the "language" (C#, Java), not the libraries, or even the standard library.  So primitives are things like `Integer`, `long`, `float`, `string`, etc..  Data structures, like maps and sets from a library might be called "primitives" for the purpose of this discussion if they are used to *represent* a domain object (instead of using a class).

To be clear, these primitives are essential for building software; the issue is using them to describe or model the domain or API.

### Quick Example

    var bodyMassIndex = CalculateBMI(36,   // Is that kg or pounds?
                                     300); // Inches, feet, centimeters?
    // and did I get the parameters in the right order?

(Ha ha, don't even get me started on why BMI isn't a good calculation to judge your health by, or that 98.6 F is a "normal" temperature.)

So, here we see some of the problems with using primitives as part of a method signature.  (Also, we see why languages with position-based argument lists can be problematic, but that's an issue for another day.)

So, what are some ways of addressing these problems?

You can create classes that indicate in the type signature what domain concept is being represented, in this case, unit of length.

    public class Length {
      public static Length FromInches(float inches) => new Length(inches * 2.54);
      public static Length FromCentimeters(int cm) => new Length(cm);
      private Length(float cm)// private constructor saves dimension
    }

This gives a nicer function signature.

    float CalculateBMI(Length height, Weight weight)

If your compiler is equipped with a type checker, you also get some help detecting out-of-order parameters.

    var weight = Weight.FromPounds(100);
    var height = Length.FromInches(100);
    var bmi = CalculateBMI(weight, height); // doesn't compile! Yeah!

### Further Up and Further In

So, what if we went further down the road of Object Oriented and make an BMICalculator object instead of a function.

    public class BMICalculator {
      public int HeightCM;
      public int WeightKG;
      public float CalculateBMI() => // do some math.
    }

Well, it's not pretty, but it works.

### A Common Example: Entity Identifiers

One of the consistent violators that I see quite often is the use of a primitive to identify an entity in the business model.  For example, a string like `Domain.Name="example.com"`.  By using a primitive, you are left with the primitive type's equality semantics, hashcode calculation, and string representation.  So, while internet treats `"example.com" == "EXAMPLE.COM"` as the same, `System.String` does not.

To get around this, we'll often see conversion to a canonical version (e.g. normalized to lower case with `domain.ToLowerCase()`), or, more likely, `string.Compare(domain1, domain2, StringComparison.OrdinalIgnoreCase) == 0`.  Yuck!  

Yeah, I know.  Code reuse lets us "Write it once and use it everywhere."  But really, does that work?  What about when you want to put it in a `Dictionary<string, DomainEntry>`?  Then the hashing system needs an appropriate hash code generator and an equality checker.

Then, assuming you've written all of this, then you probably should train the team so they know which types/comparison/function/type to use.

Now, what if Joe and Sally choose different comparison?  What class of bugs can occur on French computers where lowercase works a bit differently?

So, this is can become a big issue.

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

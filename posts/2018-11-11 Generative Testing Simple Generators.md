---
title: "Generative Testing Part 3 – Simple Generators"
date: "2018-11-11"
description: "'Generators' build data used for testing.  We assert that some properties are true of our programs, and the testing library checks those properties hold for many possible inputs."
tags: ["testing", "generative testing", "clojure.spec", "nUnit", "FsCheck"]
categories : ["Programming", "Craft"]
---

[In part 2]({{urls.base_path}}posts/2018-09-11-generative-testing-properties/) of this discussion of "generative testing", we discussed skipped the first word, "generative", and went straight to "testing."  This time, we'll move back to automatic test-case generation.  

Unlike example-based unit tests, we may not already know the "correct" answer for any given input, especially if the data are generated randomly.  If we have an oracle, we can use it to compute the correct answer.  Without the oracle, we can only test facts (properties) that should be true in every case.

From the plain English root verbs of "generative testing", we can surmise there are at least two actions to explore: 'generate' and 'test'.  In a nut shell, the actions are:

1. Generate some test scenarios (data for parameterized tests).
    * Generate some strings from combinations of "a", "b", and "c".
2. Run the program with that data, and test that some assumptions (properties) prove true for each scenario.
    * See if the program produces the same answer as the oracle, or that other properties hold.

# First Generators

Less talk, more code!

For comparison's sake, we'll try 2 libraries in different language ecosystems.

* Clojure: [Clojure.Spec](https://clojure.org/guides/spec), which generates data according to a specification using a QuickCheck-inspired property-based testing library called [Test.Check](https://github.com/clojure/test.check).
* C#/.Net: [FsCheck](https://fscheck.github.io/FsCheck) is a QuickCheck-inspired property based testing framework implemented in F#.  It has good support for C#, as well.

Let's demo an integer generator in each library to get a feel for what's involved.

## Clojure Test.Check Generators

We will be using a relatively new part of Clojure, called clojure.spec.  The property-based testing capabilities are powered by a library named [test.check](https://github.com/clojure/test.check).  This article will only call out the distinction when necessary.  Suffice it to say that Clojure.spec is useful for describing data without resorting to strong types.  Most anything that is a description of the data is spec-related, while most things data-generation-related are test.check.

Let's see how to create and use a generator.  In this case, we'll ask clojure.spec for an integer generator.  It creates an instance of `clojure.test.check.generators.Generator`, then generate some examples with it.

Clojure's REPL program makes it easy to demonstrate small examples.  They idea is that it
* Reads an expression that you type after the prompt (like `user=>`)
* Evaluates the expression, computing the result
* Prints it to the screen
* Loop back to the beginning to read again

So, let's evaluate a few expressions at the [REPL](https://clojure.org/guides/repl/introduction).  If you want to follow along, you'll want to `require` the right namespaces.  See the `:require` section of [sheepish.d-parameterized-test-with-properties](https://github.com/jeremyrsellars/no-new-legacy/blob/master/src/sheepish/test/sheepish/d_parameterized_test_with_generators.cljc#L2) or the [spec Guide](https://clojure.org/guides/spec).

```clojure
user=> (s/gen int?)
#clojure.test.check.generators.Generator{:gen #object[clojure.test.check.generators$such_that$fn__1825 0x633837ae "clojure.test.check.generators$such_that$fn__1825@633837ae"]}
user=> (gen/generate (s/gen int?))
123
user=> (take 3 (repeatedly #(gen/sample (s/gen int?) 3)))
(60273 -94 -3)
user=> (gen/sample (s/gen int?) 3))
(0 -1 1)
```

This generates a new value, or list of values, each time, which already sounds more interesting than example-based unit tests.

So, you may notice that the result from `gen/generate` seems more surprising than that of `gen/sample`.  test.check usually starts by generating smaller, more normal values.  We'll come back to this concept later in the series.

1. So, one way to make a generator for integer values is `(s/gen int?)`.  This returns a test.check generator.
2. To use that generator, we use `(gen/generate (s/gen int?))` to generate a value.
3. To generate several, we can lazily take a few "bigger" examples from an infinite lazy sequence, or a few simpler examples with `gen/sample`.

## FsCheck Generators

Let's see how to create and use a generator in FsCheck.  In this case, we'll ask clojure.spec for a int generator.  It creates a `clojure.test.check.generators.Generator`, then generate some examples with it.

C#'s type system may help describe what is going on for our type-oriented friends.

```csharp
var size = 42; // ignore for now
var exampleCount = 3; // Generate 3 random integers.
Gen<int> generator = Gen.Choose(int.MinValue, int.MaxValue);  // Gen.Choose(low, hi);
IEnumerable<int> examples = generator.Sample(size, exampleCount);
```

FsCheck generators have a size parameter that can help control the size of data generated, to prevent examples from being too simple to expose errors or too complex to execute quickly, like the size of lists, among other things.  We'll come back to that later because it's very dependent on the type of generator being used.  If you can't wait, here's the [documentation](https://fscheck.github.io/FsCheck/TestData.html#The-size-of-test-data).

1. So, one way to make a generator for integer values is `var gen = Gen.Choose(low, hi)`.  This returns an instance of `Gen<int>`.
2. To generate 3 examples, we use `gen.Sample(someSize, 3)`.

# More example generators

## Generate null/nil

```clojure
(s/gen nil?)
```

With FsCheck, you can make a constant generator that always returns the same value that is passed in.

```csharp
Gen.Constant<string>(null);
```

## Choose between alternatives

Imagine you have a few values and you want to generate a random choice between those.

The default generate for a set in clojure.spec is a choice of the members.

```clojure
(s/gen #{"bears" "beets" "Battlestar Galactica"})
```

With FsCheck, making the choice between options can be done with a combination of generators, with `Gen.OneOf`.  In this case, it chooses between "constant" generators which always return the same value.

```csharp
// Equal probability
Gen<string> gen = Gen.OneOf(
  Gen.Constant("bears"),
  Gen.Constant("beets"),
  Gen.Constant("Battlestar Galactica"));
// Weighted probability
Gen<string> wgen = Gen.Frequency(
  Tuple.Create(2, Gen.Constant("bears")),
  Tuple.Create(1, Gen.Constant("beets")),
  Tuple.Create(1, Gen.Constant("Battlestar Galactica")));
```

A lot of the data generation capabilities of these libraries comes from combining simpler generators to create more capable generators.

# Next time

Next time, we'll build some more interesting generators.

# Source code

If you want to follow along, the source code is here:

* Clojure: https://github.com/jeremyrsellars/no-new-legacy/blob/master/src/sheepish/test/sheepish/d_parameterized_test_with_generators.cljc
* C#: https://github.com/jeremyrsellars/no-new-legacy/blob/master/src/Sheepish.net/Sheepish.CSharp/D_Parameterized_Test_With_Generators.cs

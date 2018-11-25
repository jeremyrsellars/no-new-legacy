---
title: "Generative Testing Part 4 – Composing Generators"
date: "2018-11-15"
description: "'Generators' build data used for testing.  We assert that some properties are true of our programs, and the testing library checks those properties hold for many possible inputs."
tags: ["testing", "generative testing", "clojure.spec", "nUnit", "FsCheck"]
categories : ["Programming", "Craft"]
---

[In part 3]({{urls.base_path}}posts/2018-11-11-generative-testing-simple-generators) of this discussion of "generative testing", we started generating a simple types of data, integers and constants, but in order generate data useful for complicated applications, we'll need to assemble these generators to do much more powerful tasks.

# Transforming Generators

We know sheepish has only `a` and `b` characters, so let's take a list of random choices of these and convert them to a string.  In both examples, we'll build a generator that applies a function to the output of another generator.

1. Gen1: Choose between `a` and `b`.
2. Gen2: Make a list of outputs of Gen1.
3. Gen3: Apply a function to the output of Gen2 (convert to string).
4. Take a sample.

With FsCheck, the Gen class offers extension methods, much like LINQ's `.ToList()`, like `.NonEmptyListOf()`, or just `.ListOf()` which return a generator that can generate a list.  It is important to note that this returns a generator, not a list.  Also, FsCheck provides its own `Gen.Select` extension method that creates a generator that applies a function to the output of another generator.  Let's see how it reads.

```csharp
        static IEnumerable<string> SheepishAlphabetExamples =>
            Gen.OneOf(Gen.Constant('a'),
                      Gen.Constant('b'))    // Choose beetween a and b
                .ListOf()                   // Make a list like [a, b, a]
                                            // Convert to string;
                .Select(chars => new string(chars.ToArray()))  
                                            // To test case
                .Sample(4, 7);              // take 7 examples
```

In Clojure, to specify a list of a spec, use `(s/coll-of some-spec)`.  To apply a function to the output of a generator, use `(gen/fmap your-fn your-gen)`.

```clojure
(gen/sample
  (gen/fmap string/join
  	(s/gen (s/coll-of #{"a" "b"})))
  7)
; produces something like:
; ("bb" "aaaaaba" "bbbbbaabaaabbbaab" "baaabbab" "abbabaabbbbababbabba" "aaaaabbbbbbbb" "abbaaabababbabb")
```

# Combining Generators

We have already used an example of combining generators:

```csharp
            Gen.OneOf(Gen.Constant('a'),
                      Gen.Constant('b'))    // Choose beetween a and b
```

In order to generate composite data for non-trivial applications, we'll need to use more powerful generators.  These are built up from single-responsibility functions provided by the libraries.

Here are some generators for testing the card game of "Hearts," which uses a standard 52-card deck of playing cards.  Some of the generators listed below are built from random card generator, `Gen<Card> cardGen` in C# or `(s/gen ::card)`.

<style id='combinators' class='before-alternating-table'></style>

|Description|FsCheck (C#)|Clojure `(s/gen …)`|
|-----------|------------|-------------------|
|select one from several generators| `Gen.OneOf(heartGen, queenSpadeGen, cardGen)`| `(s/or :heart ::heart-card`<br>`      :queen-spade ::queen-of-spades`<br>`      :zero-point-card ::card)`|
|exact-length list| `cardGen.ListOf(5)`| `(s/coll-of ::card :count 5)`|
|pair tuple| `cardGen.Two()`| `(s/coll-of ::card :count 2)`|
|triple tuple| `cardGen.Three()`| `(s/coll-of ::card :count 3)`|
|quadruple tuple| `cardGen.Four()`| `(s/coll-of ::card :count 4)`|
|single| `Gen.Elements(new Card[]{exampleCard1,exampleCard2})`| `::card`|
|size-driven single| `Gen.GrowingElements(new Card[]{exampleCard1,exampleCard2})`| `::card`|
|size-driven list| `cardGen.ListOf()`| `(s/coll-of ::card)`<br>`(s/* ::card)`|
|size-driven list, non-empty| `cardGen.NonEmptyListOf(size)`| `(s/coll-of ::card :min-count 1)`<br>`(s/+ ::card)`|
|constant| `Gen.Constant(queenOfSpades)`| `#{some-value}`|
|satisfying constraint| `cardGen.Where(c => c.Suit == Suit.Hearts)`| `(s/and ::card is-heart?)`|
|satisfying constraint<br>(without throwing)| `cardGen.Where(c => c.Suit == Suit.Hearts`<br>`                  && c.Suit == Suit.Clubs)`<br>`// impossible or improbable`| `Use a custom generator`|
|random permutations| `Gen.Shuffle(new Card[]{exampleCard1,exampleCard2})`| `(gen/shuffle xs)`|

This list is a good place to start along the path to transforming and combining generators.  Further description of the above, from the FsCheck perspective, can be found in [FsCheck Test Data: Useful-Generator-Combinators](https://fscheck.github.io/FsCheck/TestData.html#Useful-Generator-Combinators).

You may note several similarities and differences between the .Net and Clojure versions.

* List types: `Tuple<T1,...>` and `IList<T>` (.Net) vs. arbitrary types of sequence (Clojure)
	* The FsCheck library sometimes use .Net's Tuple type, which provide O(1) access time, or `IList<T>` for variable-length/longer lists.  The type of the data structure is encoded into the type signature of the extension methods like `ListOf`, `Two`, `Three`, `Four`.
	* In contrast, Clojure specs may specify the type of container, with different characteristics (like vectors, lists, and sets).  Clojure usually uses vectors to mimic Tuples, which also offer O(1) in most of these use cases.  In this example, a vector is generated: `(s/coll-of int? :into [])`.  But, it could just as easily be generated as a sorted-set with `:into (sorted-set)`.
* Alternation: To generate a random one of 3 options, FsCheck takes 3 parameters, while Clojure spec takes 6 – twice as many parameters.
	* For example, in `(s/or :a-number int?, :a-string string?)`, the "extra" parameters provide labels for conforming a value, a process similar to destructuring.  This is out of scope for the generating data (and this blog), but can sure come in handy to switch behavior based on which spec matches.

* To-do: Describe some other options for `(s/every )`

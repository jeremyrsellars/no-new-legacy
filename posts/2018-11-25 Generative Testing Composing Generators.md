---
title: "Generative Testing Part 4 – Composing Generators"
date: "2018-11-25"
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
|alternative generators| `Gen.OneOf(heartGen, queenSpadeGen, cardGen)`| `(s/or :heart ::heart-card`<br>`      :queen-spade ::queen-of-spades`<br>`      :zero-point-card ::card)`|
|exact-length list| `cardGen.ListOf(5)`| `(s/coll-of ::card :count 5)`|
|homogeneous pair tuple| `cardGen.Two()`| `(s/coll-of ::card :count 2)`|
|homogeneous triple tuple| `cardGen.Three()`| `(s/coll-of ::card :count 3)`|
|homogeneous quadruple tuple| `cardGen.Four()`| `(s/coll-of ::card :count 4)`|
|heterogeneous tuple| `Gen.zip(suitGen, rankGen)`|`(s/cat :suit ::suit, :rank ::rank)`<br>`(gen/tuple (s/gen int?) (s/gen string?))`|
|element from list| `Gen.Elements(new Card[]{exampleCard1,exampleCard2})`| `(gen/elements [example-card-1 example-card-2])`|
|size-driven single| `Gen.GrowingElements(new Card[]{exampleCard1,exampleCard2})`| |
|size-driven list| `cardGen.ListOf()`| `(s/coll-of ::card)`<br>`(s/* ::card)`|
|size-driven list, non-empty| `cardGen.NonEmptyListOf(size)`| `(s/coll-of ::card :min-count 1)`<br>`(s/+ ::card)`|
|constant| `Gen.Constant(queenOfSpades)`| `#{some-value}`|
|satisfying constraint| `cardGen.Where(c => c.Suit == Suit.Hearts)`| `(s/and ::card is-heart?)`|
|satisfying constraint<br>(without throwing)| `cardGen.Where(c => c.Suit == Suit.Hearts`<br>`                  && c.Suit == Suit.Clubs)`<br>`// impossible or improbable`| `Use a custom generator`|
|random permutations| `Gen.Shuffle(new Card[]{exampleCard1,exampleCard2})`| `(gen/shuffle xs)`|


This list is a good place to start along the path to transforming and combining generators.  Further description of the above, from the FsCheck perspective, can be found in [FsCheck Test Data: Useful-Generator-Combinators](https://fscheck.github.io/FsCheck/TestData.html#Useful-Generator-Combinators).

You may note several similarities and differences between the .Net and Clojure versions.

* List types: `Tuple<T1,...>` and `IList<T>` (.Net) vs. arbitrary types of sequence (Clojure)
	* The FsCheck library sometimes use .Net's `System.Tuple` type, which provide constant `O(1)` access time, or `IList<T>` for variable-length/longer lists.  The type of the data structure is encoded into the type signature of the extension methods like `ListOf`, `Two`, `Three`, `Four`.
	* In contrast, Clojure specs may specify the type of container, with different characteristics (like vectors, lists, and sets).  Clojure usually mimics Tuples with vectors, which also offer constant time access in these use cases.  In this example, a vector is generated: `(s/coll-of int? :into [])`.  In fact, the default collection type for `s/coll-of` is a vector, so `:into []` is optional.  But, it could just as easily be generated as a sorted-set with `:into (sorted-set)`.  In FsCheck, this would require two steps: generating a collection, then transforming the result type with `.Select`, as in `intGen.ListOf(5).Select(lst => new SortedSet<int>(lst))`.  For more, optional parameters of [`s/coll-of`](https://clojure.github.io/spec.alpha/clojure.spec.alpha-api.html#clojure.spec.alpha/coll-of), see [`s/every`](https://clojure.github.io/spec.alpha/clojure.spec.alpha-api.html#clojure.spec.alpha/every) for a detailed description of the options for how to customize collection-data generation.
* Labeled tuples and conformance (Clojure spec) vs Anonymous tuples (.Net): To generate a random value from 2 generators, FsCheck's `Gen.OneOf` takes 2 parameters, while Clojure spec `s/or` takes 4 – twice as many parameters.  This is also true of homogeneous tuples with `s/cat`.  Both produce similar unlabeled data, but in Clojure the values can be run 'backwards' through a spec to produce labeled data.  This is called conformance.
    * Since FsCheck uses `System.Tuple`, which doesn't support "naming" the ordinals, it uses anonymous labels like `tuple.Item1`, `.Item2`, etc. so the semantic meaning of `Item2` may not be obvious in the code or during runtime introspection.  Often the generic type serves to label the data, but when a tuple is used to model `float heightCm` and `float weightKg`, this can be confusing, since the use is embedded in the type.  Consider using [Domain Identifiers]({{urls.base_path}}posts/2017-08-08-domain-identifiers-instead-of-primitive-obsession) for some better ways to model with types.
    * The Clojure "alternative" generator, i.e. `(s/or :a-number int?, :a-string string?)`, where a single value is returned that matches one spec `1` or another `"a"`. The "extra" parameters provide labels for conforming a value, a process similar to destructuring.  This is out of scope for data generation (and this blog), but can sure come in handy to switch behavior based on which spec matches.
    * For the heterogeneous tuple example, in `(s/cat :a-number int?, :a-string string?)` where a vector tuple like `[1 "a"]` is returned containing a value matching each spec in order, the "extra" parameters also provide labels when conforming a value for destructuring or runtime introspection.

# Pulling it all together

C#
---------

Now, let's build a card generator.  A card will be modeled with a type and enums in C# (like `new Card{Suit=Suit.Diamonds, Rank=Rank.Ten})`:

```csharp
    public struct Card
    {
        public Suit Suit;
        public Rank Rank;
    }
    public enum Suit
    {
        Clubs, Hearts, Spades, Diamonds
    }
    public enum Rank
    {
        King, Queen, Jack, Ten, Nine, Eight, Seven, Six, Five, Four, Three, Two, Ace
    }
```

Generate example cards by defining and combining some generators with some of the functions described above.  `.Elements` chooses from a list of suits or ranks.  `Gen.zip` and `.Select` are used to create a Tuple, and then map a Tuple to a card.

```csharp
    class E_Hearts_Card_Generators
    {
        static Gen<Suit> suitGen =
            Gen.Elements((Suit[])Enum.GetValues(typeof(Suit)));
        static Gen<Rank> rankGen =
            Gen.Elements((Rank[])Enum.GetValues(typeof(Rank)));

        static Gen<Card> heartGen =
            rankGen
            .Select(rank => new Card { Rank = rank, Suit = Suit.Hearts });

        static Gen<Card> cardGen =
            Gen.zip(rankGen, suitGen)
            .Select(c => new Card { Rank = c.Item1, Suit = c.Item2 });

        static IReadOnlyList<Card> CreateRandomCards(int count) =>
            cardGen.Sample(100, count);
    }
```

### Clojure

In Clojure, we might model a card as map and keywords in Clojure, like `{:suit :diamonds, :rank :ten}`.

These specs might similarly describe card data.

```clojure
(s/def ::suit #{:clubs :hearts :spades :diamonds})
(s/def ::rank #{:king :queen :jack :ten :nine :eight :seven :six :five :four :three :two :ace})
(s/def ::card (s/keys :req-un [::suit ::rank]))
```

Let's see them in action:

```clojure
(defn generate-examples
  "Simple function to generate some example values that satisfy a spec."
  [spec example-count]
  (let [generator (s/gen spec)
        generate-example #(gen/generate generator)
        examples (repeatedly generate-example)]
    (take example-count examples)))

; user=> (generate-examples ::suit 8)   ; Generate 8 suits
(:clubs :diamonds :hearts :clubs :clubs :diamonds :diamonds :hearts)
; user=> (generate-examples ::rank 8)   ; 8 ranks
(:five :nine :six :three :seven :nine :ace :six)
; user=> (generate-examples ::card 3)   ; 3 cards
({:suit :diamonds, :rank :ten}
 {:suit :hearts,   :rank :three}
 {:suit :diamonds, :rank :queen})
```

# Back to the sheep pen

So, to generate more tricky sheepish and sheepish-like data, we may want to combine several generators.

Next time, we'll compose better sheepish-like strings in an attempt to trick the `sheep-bleat?` function.

# Source code

If you want to follow along, the source code is here:

* Clojure: https://github.com/jeremyrsellars/no-new-legacy/blob/master/src/sheepish/test/sheepish/e_hearts_card_generators.cljc
* C#: https://github.com/jeremyrsellars/no-new-legacy/blob/master/src/Sheepish.net/Sheepish.CSharp/E_Example_Generators.cs

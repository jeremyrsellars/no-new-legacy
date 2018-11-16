---
title: "Generative Testing Part 4 – Composing Generators"
date: "2018-11-15"
description: "'Generators' build data used for testing.  We assert that some properties are true of our programs, and the testing library checks those properties hold for many possible inputs."
tags: ["testing", "generative testing", "clojure.spec", "nUnit", "FsCheck"]
categories : ["Programming", "Craft"]
---

[In part 3]({{urls.base_path}}posts/2018-11-15-generative-simple-generators/) of this discussion of "generative testing", we started generating a simple types of data, integers and constants, but in order generate data useful for complicated applications, we'll need to assemble these generators to do much more powerful tasks.

# Transforming Generators

We know sheepish has only `a` and `b` characters, so let's take a list of random choices of these and convert them to a string.  In both examples, we'll build a generator that applies a function to the output of another generator.

1. Gen1: Choose between `a` and `b`.
2. Gen2: Make a list of outputs of Gen1.
3. Gen3: Apply a function to the output of Gen2 (convert to string).
4. Take a sample.

With FsCheck, the Gen class offers extension methods, much like LINQ's `.ToList()`, like `.NonEmptyListOf()`, or just `.ListOf()` which return a generator that can generate a list.  It is important to note that this returns a generator, not a list.  Also, FsCheck provides its own `Gen.Select` extension method that creates a generator that applies a function to the output of another generator.  Let's see how it reads.

```csharp
        static IEnumerable<string> SheepishExamples =>
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

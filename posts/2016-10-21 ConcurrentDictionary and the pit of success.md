---
title: "ConcurrentDictionary and the Pit of Success"
date: "2016-10-21"
revised: "2016-10-23"
description: When a class attempts to implement two different, incompatible usage patterns, it can lead to some nasty surprises.  This is part 1 in a series about exploring how a small violation of the Liskov Substitution Principal can lead to unexpected bugs.  ConcurrentDictionary can be accidentally used incorrectly because of a subtle Liskov Substitution Principal violation.
tags: ["state-management", "SOLID-principals", "Liskov-substitution-principal", "multi-threaded-code", ".net", "c#", "abstraction"]
categories : ["Programming", "Professionalism"]
---

When it comes to managing state in .NET, ConcurrentDictionary is not a silver bullet.  When a data structure attempts to implement two different, incompatible usage patterns, it can lead to some nasty surprises.  This post explores how a "small" violation of the Liskov Substitution Principal can lead to unexpected bugs.  ConcurrentDictionary can be accidentally used incorrectly because of a subtle Liskov Substitution Principal violation.

# Thread-safety of ConcurrentDictionary

Imagine implementing a cache with a dictionary.  Some threads add dictionary entries, and a maintenance thread selectively removes old entries.  Perhaps you wonder, what about that new namespace `System.Collections.Concurrent`... Would the [ConcurrentDictionary](https://msdn.microsoft.com/en-us/library/dd287191%28v=vs.110%29.aspx) let any thread mutate it without having to use explicit locking or having to think of the consequences?

## Research
I did not know whether the ConcurrentDictionary could be enumerated safely while being modified in another thread.  I set out to answer the question "Is ConcurrentDictionary any more thread-safe than Dictionary?"  `ConcurrentDictionary`'s [thread-safety disclaimer](https://msdn.microsoft.com/en-us/library/dd287191%28v=vs.110%29.aspx) does *not* indicate to me that it would be safe to enumerate, but it *has to be* safe, right? (Spoiler!  Answer: not always).

> All public and protected members of ConcurrentDictionary&lt;TKey, TValue&gt; are thread-safe and may be used concurrently from multiple threads. However, **members accessed through one of the interfaces the ConcurrentDictionary&lt;TKey, TValue&gt; implements, including extension methods, are not guaranteed to be thread safe** and may need to be synchronized by the caller.

(*emphasis* mine)

This guarantee applies to [ConcurrentDictionary.ToArray()](https://msdn.microsoft.com/en-us/library/dd287109%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396), so that would seem a safe choice to convert the dictionary to something enumerable.  But what about using `GetEnumerator` (like from a `foreach` statement in C#)?

What about the [enumerator](https://msdn.microsoft.com/en-us/library/dd287131%28v=vs.110%29.aspx) used by `foreach`?

> The enumerator returned from the dictionary is safe to use concurrently with reads and writes to the dictionary, however it does not represent a moment-in-time snapshot of the dictionary. The contents exposed through the enumerator may contain modifications made to the dictionary after `GetEnumerator` was called.

I set up a small console project to see how enumeration would work while concurrently adding or removing entries using:

* C#'s `foreach` (`.GetEnumerator()`)
* Linq's `.ToList()`
* F#'s `ofSeq`

```c-sharp
static void Main()
{
    WriteLine("TEST #1 - Enumerate while adding");
    WriteLine("-------------------------------------------------------------------------------");
    WriteLine("Add many entries in one thread, while reading entries with other threads.");
    WriteLine();
    MainAdd();

    dict.Clear();

    WriteLine();
    WriteLine("TEST #2 - Enumerate while removing");
    WriteLine("-------------------------------------------------------------------------------");
    WriteLine("Remove many entries in one thread, while reading entries with other threads.");
    WriteLine();
    MainRemove();
}

static void MainAdd()
{
    var threads = new[] {
        new Thread(AddEntries),
        new Thread(EnumerateWithForeachCount),
        new Thread(EnumerateWithLinqCount),
        new Thread(EnumerateWithFsharpCount),
    };
    foreach (var t in threads)
        t.Start();
    foreach (var t in threads)
        t.Join();
}

static void MainRemove()
{
    AddEntries();
    var threads = new[] {
        new Thread(RemoveEntries),
        new Thread(EnumerateWithForeach0),
        new Thread(EnumerateWithLinq0),
        new Thread(EnumerateWithFsharp0),
    };
    foreach (var t in threads)
        t.Start();
    foreach (var t in threads)
        t.Join();
}
```

You can see the rest of the code here in the [gist](https://gist.github.com/a3394f791ae1759d19a40beacf5e143c).  Here are the results.

Now original the project that raised this question was written in F#.  But since more people "speak" C#, that's the language I used for my test.  **So I started with a Linq implementation and it threw an exception that I did not expect.**  So I tried almost the same thing, but this time with the F# libraries used in idiomatic F# (easily referenced from the C# project). It worked just fine (I was not able to make it fail with FSharp's `SeqModule.OfSeq`, or with a Linq `where` filter, or by using the enumerator directly.)

Here is a more complete test program output, now that I understand the problem.

#### TEST #1 - Enumerate while adding

Add many entries in one thread, while reading entries with other threads.

* Linq    - Failed with:
   * System.ArgumentException: The index is equal to or greater than the length of the array, or the number of elements in the dictionary is greater than the available space from index to the end of the destination array.
   * at System.Collections.Concurrent.ConcurrentDictionary&#x60;2.System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey,TValue>> **.CopyTo(KeyValuePair&#x60;2&#91;&#93; array, Int32 index)**
   * at System.Collections.Generic.List&#x60;1..ctor(IEnumerable&#x60;1 collection)
   * at System.Linq.Enumerable.ToList&#91;TSource&#93;(IEnumerable&#x60;1 source)
   * at ConcurrentDictTest.Program.EnumerateWithLinq(Int32 expectedCount)
* Done adding 100000
* Foreach - Successfully enumerated. Count: 100000
* F#      - Successfully enumerated. Count: 100000

#### TEST #2 - Enumerate while removing

Remove many entries in one thread, while reading entries with other threads.

* Done adding 100000
* Linq    - Failed with:
   * System.Exception: **Somehow the dictionary returned a null.**
   * at ConcurrentDictTest.Program.EnumerateWithLinq(Int32 expectedCount)
* Done removing.  Count:  0
* F#      - Successfully enumerated. Count: 0
* Foreach - Successfully enumerated. Count: 0

### Linq's .ToList() fails, and F# succeeds.  What is going on?!

Digging deeper, I found that when I used `ConcurrentDictionary.ToList()` (Linq extension methed) to create a `List<T>`, it threw an exception with `ICollection<T>.CopyTo` in the stack trace.  So that is verifiably *not thread safe*.

## Analysis: Where is the design flaw?

Linq's `List<T> Enumerable.ToList<T>(this IEnumerable<T> items)` is an extension method that takes an Enumerable and returns a list.  It ultimately calls the List<T> constructor, passing in the IEnumerable, which in turn attempts a performance optimization by using ICollection<T>.CopyTo to copy into the new list's internal array.  So Linq/List is using ConcurrentDictionary, despite the documented warning saying "extension methods are not guaranteed to be thread safe".

#### Is ConcurrentDictionary at fault?

Wait ConcurrentDictionary's thread-safety disclaimer told as much - Only trust the explicit members, not the members accessed through an implemented interface (like `ICollection<T>`). So surely `ConcurrentDictionary` is not at fault. (But please delay judgement.)

### Ok, so Linq is using ConcurrentDictionary wrong.  Is it Linq's fault?

In this example, `List(IEnumerable<T>)` is using ConcurrentDictionary in an unsafe way and it is even documented.  Should we consider this performance optimization premature and dangerous?

### Judgement: List vs. ConcurrentDictionary

List's `List(IEnumerable<T>)` constructor uses a valid, type-safe transformation (is/cast).  List is not at fault.

The problem is not a documentation problem, and not a implementation problem - the implementation of the interface is correct.  **The problem is a semantic conflict.**

There is an implicit, semantic assumption (effectively a requirement of how to use the class correctly) in `ICollection<T>` - that the collection won't shrink or grow between asking its count and copying its contents.  This concurrent data structure also attempts to implement the thread-safe-mutation semantic, but it fails to deliver on `ICollection<T>`'s semantic assumption.

## Liskov Substitution Principal

This is where Barbara Liskov's notion of "behavioral" subtyping, commonly known as the Liskov Substitution Principal, shines as a principal that helps us avoid this kind of design flaw.

This is explored in [Part 2 - Liskov Substitution Principal and the Pit of Success]({{urls.base_path}}posts/2016-10-23-liskov-substitution-principal-and-the-pit-of-success).

> If you call something a (subtype of) duck, it had better look like a duck and quack like one.

# Concluding Remarks

* We ended up determining that ConcurrentDictionary was safe to use in this case and was prettier than the alternatives (in this case *rewriting* with immutability or using explicit locking).  So, we used it.
* ConcurrentDictionary can easily be used incorrectly with idiomatic C# (Linq). So much for making it easy to fall into the "pit of success"!
* ConcurrentDictionary can be a useful tool, but it is not a "silver bullet."  I consider it "advanced" (in a bad way) and I will avoid it when better a better solution presents itself.
* This is not meant to be a rant.  Many .Net collection types have LSP violations, but that was a necessary evil given the historical context.  They are more versatile that way.  They are tools - tools that can be used well or used poorly.  They have evolved over the years and their maintainers valiantly strive to maintain backwards-compatibility so we developers can get the most from the .Net Framework.  These are not easy tasks and I want to applaud their efforts and thank them for a job well done.  I am confident that these LSP violations would be better-addressed if they had it to do over again.
* From my perspective, [`System.Collections.Immutable.ImmutableDictionary`](https://msdn.microsoft.com/en-us/library/dn467181%28v=vs.111%29.aspx?f=255&MSPPError=-2147217396) is much easier to wrap my head around because it is thread safe, end of story.  I like options that are easier to understand and easier to implement (code is written once and read often).

<a id="pit-of-success"></a>


I briefly mentioned the "Pit of Success".  Usually, professional programmers do not labor in isolation.  When professionals design software and interfaces, we don't just dig a bear trap, post a sign at the trailhead, and walk away.  We don't rely on our peers to read the documentation and assume they will avoid the troublesome spots we've created. Professionals follow the principal of least surprise.

Legacy Software is scary to change because even the smallest change carries the fear of breaking something.  By using SOLID principals, professionals make it easier to use the APIs correctly and take away the opportunities for surprise.

## Next

In [Part 2 - Liskov Substitution Principal and the Pit of Success]({{urls.base_path}}posts/2016-10-23-liskov-substitution-principal-and-the-pit-of-success), we explore how to avoid unexpected integration bugs by following the Liskov Substitution Principal.

Thanks for reading.  Cheers,

Jeremy

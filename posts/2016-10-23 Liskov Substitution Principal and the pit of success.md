---
title: "Liskov Substitution Principal and the Pit of Success"
date: "2016-10-23"
revised: "2016-10-24"
description: When a data structure attempts to implement two different, incompatible usage patterns, it can lead to some nasty surprises.  In part 2 we explore how to avoid unexpected integration bugs by following the Liskov Substitution Principal.
tags: ["state-management", "SOLID-principals", "Liskov-substitution-principal", "abstraction"]
categories : ["Programming", "Professionalism"]
---

When a data structure attempts to implement two different, incompatible usage patterns, it can lead to some nasty surprises.  In part 2 we explore how to avoid unexpected integration bugs by following the Liskov Substitution Principal.

## Recap of Part 1 - ConcurrentDictionary: a Tale of Two Usage Patterns

In [part 1]({{urls.base_path}}posts/2016-10-21-concurrentdictionary-and-the-pit-of-success), we considered the implicit, semantic assumption (effectively a requirement of how to use the class correctly) in `ICollection<T>`.  In this case, it assumed that the collection wouldn't shrink or grow between asking its count and copying its contents.  The ConcurrentDictionary attempted to expose both a thread-safe, mutable Dictionary usage-pattern, but it failed to deliver on `ICollection<T>`'s semantic assumption.

This is where Barbara Liskov's notion of "behavioral" subtyping, commonly known as the Liskov Substitution Principal, shines as a principal that helps us avoid this kind of design flaw.

# Liskov Substitution Principal

> Liskov's notion of a behavioral subtype defines a notion of substitutability for objects; that is, if S is a subtype of T, then objects of type T in a program may be replaced with objects of type S without altering any of the desirable properties of that program (e.g. correctness).

From [Wikipedia](https://en.wikipedia.org/wiki/Liskov_substitution_principle)

Here is my rephrasing of the LSP, which trades a bit of fidelity for ease of remembering:

> If you call something a (subtype of) duck, it had better look like a duck and quack like one.

In other words to make one type substitutable for another - in following the Liskov Substitution Principal:

* If you implement an interface, implementations (sub-types) of that interface must honor the semantic assumptions in the interface.
* The same applies for many similar abstractions:
  * Derived classes
  * Action delegates, lambda functions, etc.
  * Implementations of [protocols](http://clojure.org/reference/protocols)
* Consider the required usage patterns (semantic assumptions), including:
  * Required sequence of operations
  * Synchronized access and assumed/implicit critical section
  * The types of exceptions thrown or caught
  * Out-of-band resources (implicit connections and transactions, configuration).
      * There may be value in making these implicit, but where should these come from? (This sounds like a good follow-up blog post.)
  * The availability of libraries, Nuget packages, etc.
  * [Covariance and contravariance](https://en.wikipedia.org/wiki/Covariance_and_contravariance_%28computer_science%29)  are checked by the C# compiler, e.g. `IEnumerable<out T>`, `Action<in T>`.  In less formal types or dynamic languages, take care to handle these appropriately, e.g. especially around the required keys of a dictionary or the types of the values in a vector, set, or map.
  * Pre-conditions and post-conditions should be honored.  In more powerful specification systems, pre-conditions and post-conditions can be tested
      * Relational specs (like [clojure.spec](http://clojure.org/guides/spec#_higher_order_functions) and [.Net Code Contracts](https://msdn.microsoft.com/en-us/library/dd264808%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396)) can encode some of these semantic assumptions into run-time/test-time assertions.
      * [Dependent types](https://en.wikipedia.org/wiki/Dependent_type) in languages like [Idris](http://www.idris-lang.org/) can encode some of these semantic assumptions into the type definition, thereby enforcing valid usage at compile time, but I imagine this comes with its own set of substitutability constraints.

## Related SOLID principals

### Interface Segregation Principal and Single Responsibility Principal

It is easier to follow the LSP when there are fewer semantic assumptions and/or fewer members in the interface.  This brings up the Interface Segregation Principal, which leads to more interfaces with fewer members.  The interfaces aren't so much about the "thing" (entity or noun) as they are the usage patterns.  For example, instead of having a single `IAuditRepository`, it can be helpful to break it further into:

1. An interface about recording (inserting) audit records might have a single method.  `IAuditRecorder`
1. An interface about retrieving existing audit records for display or analysis might be read-only.  `IAuditReader`
1. An interface about exporting audit records to a different system. `IAuditExporter`

In practice, the implementation of these interfaces may be provided by a single `class AuditRepository : IAuditRecorder, IAuditReader`, but it does not have to be.  By isolating the responsibilities (Single Responsibility Principal) and behavior patterns (Interface Segregation Principal), the system will be more "soft," flexible, and easy to change.

### Open-Closed Principal

The LSP encourages alternative implementations to support the original usage pattern(s).  This allows the dependent parts of the application to be open to extension without requiring modification.  

For out-of-band resources like implicit database connections and transactions, and configuration, it is often okay to support these in the main module (the module on which nothing else depends).  Another common way to support these is with a dependency injection framework or extensibility framework.

# Concluding Remarks

<a id="pit-of-success"></a>

In [part 1]({{urls.base_path}}posts/2016-10-21-concurrentdictionary-and-the-pit-of-success#pit-of-success), I mentioned the "Pit of Success" that endeavors to make it easy to use the api.  Usually, programmers are a part of a team.  Even those who work in a consulting capacity are building something that will eventually be handed off to others for maintenance and possibly extension.  When professionals design software and abstractions, they are making it easy to do "something" - and hopefully that includes "proper usage".  Professionals follow the principal of least surprise.

Software can become scary to change when it has a history of breaking with a seemingly innocuous change, like substituting one implementation for another.  The Liskov Substitution Principal warns us to consider the implicit requirements, semantic behaviors, and usage patterns.  By using SOLID principals, professionals make it easier to use the abstractions correctly and take away the opportunities for surprise.

Thanks for reading.  Cheers,

Jeremy

## More on these Subjects

* [Liskov Substitution Principal](https://en.wikipedia.org/wiki/Liskov_substitution_principle#Principle) - if you call something a duck, it had better look like a duck and quack like one.
* [SOLID Principals of Object-Oriented Design](https://en.wikipedia.org/wiki/SOLID_%28object-oriented_design%29#Overview)

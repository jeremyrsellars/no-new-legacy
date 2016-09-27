---
title: "Functional State Management (in .Net)"
date: "2016-09-27"
description: Bring some sanity to state management in C# by borrowing functional programming concepts.
tags: ["fp", "atoms", "actor model", "structural equality", "communicating sequential processes"]
categories : ["professionalism", "programming"]
---

# Functional State Management (in .Net)

## Terms
* Identity = an entity that has a state.  (In C#, this could be a variable, field, or object)
* State = the value at a point in time. (As of the time of this writing, today's date is 2016-09-26)
* Value = something that doesn't change. (42, 2016-07-01)

These definitions are inspired by http://clojure.org/about/state

## What is state?
"State" - the implicit parameter(s) to a function (everything it needs in order to calculate the function that isn't an explicit argument)

## Status Quo (How is state usually managed in C#)

Let's walk through an example of the HR department trying to make team members happy by providing their favorite drinks in the kitchen.  On birthdays and service anniversaries, the team members are invited to the kitchen for a free celebratory beverage.

Over the years, HR introduces new policies, and you never know what they'll ask for next, so this example goes from easy to hard as more requirements are added.

## Favorite Drink Project (Initial Scope)

Surveys are given:

* To new hires on their first day.
* Annually (leading up to service anniversaries).
* Following every monthly meeting.

The surveys are processed in batches.

### Bread and Butter (Assignment)
The most idiomatic way of managing state in C# is through assignment.

```csharp
class TeamMember
{
    Drink _favoriteBeverage;
    public Drink FavoriteBeverage => _favoriteBeverage;
    public void UpdateFavorites(TeamSurvey survey)
    {
        _favoriteBeverage = survey.FavoriteDrink;
    }
}
```

Constraints:

* Only one thread may observe or change state at a time, or risk inconsistency.
* The developer must know when it is ok and when it isn't ok to use/change state.

## Favorite Drink Project (Phase 2 - support concurrency) -- Interlocked Thread Safe
Now, let's say there's a bunch of autonomous agents in your process all processing the survey results.  Also, let's say HR didn't prevent team members from submitting 100 surveys at the same time (to win the X-box drawing).

If you like "lock-free" (and you like to block all your threads at once), you might use Interlocked.

```csharp
class TeamMember
{
    Drink _favoriteBeverage;
    public Drink FavoriteBeverage
    {
        get
        {
            Drink d = null;
            Interlocked.Exchange(ref d, _favoriteBeverage);
            return d;
        }    
    }
    public void UpdateFavorites(TeamSurvey survey)
    {
        Interlocked.Exchange(ref _favoriteBeverage, survey.FavoriteFood);
    }
}
```

Constraints:

* Only one piece of state may be changed atomically.  Consistency across multiple values is very difficult.
* Future developers and maintainers must know (and remember) to use `Interlocked` to use/change state.

## Favorite Drink Project (Email when in stock) - Lock
Now, imagine that the requirements change and HR wants to give a complimentary beverage to each team member on birthdays and service anniversaries, but only if the kitchen stocks the beverage.

So now that we need a flag to indicate whether the kitchen keeps the beverage in stock, interlocked may be a bad idea since related variables are not updated atomically.  You can imagine a race condition when a team member submits 50 surveys for Mountain Dew and 50 for some fine Cabernet.  One is usually stocked, and the other isn't.  The team member might be in for a let-down when the congratulatory email says the wine is available.

```csharp
class TeamMember
{
    Drink _favoriteBeverage;
    bool _favoriteBeverageIsStocked;
    public Drink FavoriteBeverage
    {
        get
        {
            lock(_lock)
            {
                return _favoriteFood;
            }
        }    
    }
    public void UpdateFavorites(TeamSurvey survey, Func<Drink,bool> drinkIsStocked)
    {
        lock(_lock)
        {
            _favoriteFood = survey.FavoriteFood;
            _favoriteBeverageIsStocked = drinkIsStocked(_favoriteBeverage);
        }
    }
}
```

Constraints:

* Deadlocks are possible if there are multiple locks.
* Future developers and maintainers must know (and remember) to use `lock` to use/change state.
* Future developers and maintainers must know (and remember) not to introduce another lock or risk deadlocks.

### Summary of the idiomatic (C#)
We observe in the Favorite Beverages example, that the familiar/idiomatic ways of managing state are tricky.  The field being changed needed to be protected by either `Interlocked` or `lock/Monitor` and this led to more verbose code, with state-transitions that are more difficult to test.

Once we start needing to "protect" variables, the code starts getting more verbose and complicated.  It becomes difficult to reason about what is happening.  So in a multi-threaded environment, what do we need to protect?  Only variables that change (mutate) - things that are assigned.

## Constraints

Let's take an interlude and think about programming styles for a moment.
 
Cristina Lopes and Uncle Bob have similar things to say about programming style being about constraints.

* [Cristina Lopes discusses the idea of using constraints to define styles of programming and architecture](http://www.infoq.com/presentations/style-methodology).  I saw her speak at StrangeLoop and I loved the way she implemented a bunch of different programming styles (but all in Python).  Here's her book on the subject: [Excercises in Programming Style](http://www.amazon.com/Exercises-Programming-Style-Cristina-Videira/dp/1482227371).
* Uncle Bob describes the constraints that define [Three Paradigms](https://blog.8thlight.com/uncle-bob/2012/12/19/Three-Paradigms.html) of programming (Structured, Object-Oriented, Functional). For example, "All functional programs are dominated by one huge constraint. They don’t use assignment."

### Constraining Assignment
Since assignment makes things harder to reason about and harder to test, perhaps we should avoid it.  But without assignment, how can we change state?

So if we can't eliminate assignment, can we manage it by using a more predictable idiom?


## Actor Model (Idiomatic F#)
F# offers the Mailbox implementation of the Actor Model, where an actor process (or "thread of logic") is sent messages through the mailbox and processes them one at a time with consistent state transitions.  This message passing is an *asynchronous* object-oriented approach.  The "constraint" is that you can change state, but only by sending messages.  There is a one-to-one relationship between queues and actors.

A great opportunity in the actor model is that each type of state transition can be tested.  The new state (after processing the message) is a function of the old state and the message (`newState = F(previousState,message)`).

Constraints:

* Direct assignment isn't possible - the developer may send messages for the actor to process.

Goodness:

* Testable state transitions.
* All state transitions happen in one place in code and are triggered by messages.
* Future developers and maintainers are guided to safe methods of state transition - the compiler enforces this.

## Communicating Sequential Processes (Go, Clojure CLR)
Clojure's [core.async](http://clojure.org/news/2013/06/28/clojure-clore-async-channels) library implements Communicating Sequential Processes and channels.  (Go programming language also implements CSP).

Like the actor model, this is a message-passing model, though the CSP model is synchronous.  The "channels" double as a queue and a synchronization constraint.

I read that it has been shown that Actor-like behavior can be implemented with CSP, and vice versa, (though I am having trouble locating the source of this assertion.)

Constraints:

* Similar to actors

Goodness:

* Similar to actors, plus:
* Fewer OS threads are required
* The structure of CSP-based systems coordinates work, maintaining valid sequences.

[More on core.async](https://github.com/clojure/core.async#presentations)

## Atoms
Another way to think about state management is Atoms.  There are variants of AtomicReferences in [Java8](https://docs.oracle.com/javase/8/docs/api/?java/util/concurrent/atomic/AtomicReference.html), [Akka.net] (http://api.getakka.net/docs/stable/html/A8A879A7.htm), [Clojure-Clr](https://github.com/clojure/clojure-clr/blob/master/Clojure/Clojure/Lib/AtomicReference.cs), [C++11](http://en.cppreference.com/w/cpp/atomic), and many more.

I was first introduced to atoms through Clojure/ClojureScript.

In object-oriented languages, Atoms are objects that hold state and only change it through a specific set of operations.  For example, an atom usually supports these operations:

* Reset - ignore the value that was there before and set the atom to a new value.
  * In C#, `myAge.Update(36)`.
* Update - take the old value and use it in the calculation of a new value for the atom.
  * In C#, `myAge.Update(old => old + 1)`.
* GetValue - get the current value thread-safely
  * In C#, `Console.WriteLine(myAge.Value)`

Remember, "value" means something that doesn't change.

Constraints:

* Direct assignment isn't possible - the developer must explicitly consider the previous state (or consciously ignore it).
* State transition must be free of side-effects (pure)
* Previous state should not be mutated (which is made explicit by using immutable data types).

Goodness:

* Direct assignment isn't possible - the developer must explicitly consider the previous state (or consciously ignore it).
* Future developers and maintainers are guided to safe methods of state transition.  Some languages can even enforce this at compile-time.
* All state transitions are managed in the same way.  Combine with CSP/Actors for even more goodness.

More about [atoms](http://clojure.org/reference/atoms).

### Extra Atom Goodness

Some atom implementations support validators or guards that prevent the atom from violating an assertion.  Here is clojure-clr's implementation of Update() (or "`swap`" in [clojure-clr](https://github.com/clojure/clojure-clr/blob/5beaf1162ac853795f88bba977b13ba35c1416c5/Clojure/Clojure/Lib/Atom.cs#L80)).

This method takes in a (Clojure) function used to calculate the new value, validates the value, and then notifies any watchers of the new value.

```csharp
        public object swap(IFn f)
        {
            for (; ; )
            {
                object v = deref();
                object newv = f.invoke(v);
                Validate(newv);
                if (_state.CompareAndSet(v, newv))
                {
                    NotifyWatches(v,newv);
                    return newv;
                }
            }
        }
```

Since it uses compare and set, if another thread updated the value between `deref()` and `CompareAndSet`, it may have to loop and recalculate the replacement value as a function of the atom's "new" current value.

### Favorite Drink Survey with Atoms

```csharp
immutable class TeamMember
{
    public Drink FavoriteBeverage;
    public bool FavoriteBeverageIsStocked;
}

class SurveyProcessor
{
    Func<Drink,bool> drinkIsStocked;
    public void UpdateFavorites(Atom<TeamMember> teamMember, TeamSurvey survey)
    {
        teamMember.Update(tm => WithFavoriteDrink(tm, survey.FavoriteDrink));
    }

    internal static TeamMember WithFavoriteDrink(TeamMember teamMember, Drink favorite, Func<Drink,bool> drinkIsStocked) =>
        teamMember
            .WithFavoriteDrink(favorite)
            .WithFavoriteBeverageIsStocked(drinkIsStocked(favorite));
}
```

Wait!  What? C# supports Immutable classes?

Nope, sorry, C# isn't that cool [yet (see proposed Builder Pattern)](https://github.com/dotnet/roslyn/blob/master/docs/designNotes/2015-02-04%20C%23%20Design%20Meeting.md#builder-pattern).  I'm just trying to paint a picture.

### Immutability
Today, there isn't a great story for immutability in C#.  We have to [roll our own](https://github.com/jeremyrsellars/Valuable/blob/b4f04dbb2a4aefd5dc356dc9ebe30d66f06b61ba/ConsoleApplication1/MailingAddress.cs#L9) immutable "values," but I hope that will change soon.

State of immutability in .Net in 2016:

* [System.Collections.Immutable](https://msdn.microsoft.com/en-us/library/system.collections.immutable(v=vs.111).aspx)
* Structs with `MemberwiseClone`
  * In C#, structs implement [shallow structural equality](https://msdn.microsoft.com/en-us/library/2dts52z7(v=vs.110).aspx) (by default, but you can override the implementation), so you can compare them for equality based on their field values.  See next section for more on structural equality and why it can be a beautiful thing.
* [Roll your own](https://github.com/jeremyrsellars/Valuable/blob/b4f04dbb2a4aefd5dc356dc9ebe30d66f06b61ba/ConsoleApplication1/MailingAddress.cs#L9) immutable "values."

## Structural Equality
Structural equality means considering the structure of the things being compared.  For example, two different mathematical sets are equal if their contents are the same, regardless of the order they happen to be stored in.  For example `x = new HashSet{3,1,2}; Assert(x == x.ToSortedSet());`.  Also, theoretically, two different objects are structurally equal if they have the same fields, however this may be an expensive operation to compare, so .Net hasn't usually implemented structural equality for objects.

### Example
```csharp
var lookup = new Dictionary<string,SomethingAwesome>();
lookup["A"] = new SomethingAwesome();
lookup["ABC".Substring(0,1)] // yields that same instance because of value semantics - two different strings instances representing "A".

// But, most .Net things don't have value semantics or structural equality.
var lookup = new Dictionary<HashSet<string>,SomethingAwesome>();
lookup[new HashSet<string>{"A"}] = new SomethingAwesome();
lookup[new HashSet<string>{"ABC".Substring(0,1)}] // throws not found exception because the reference is different.
``` 

Clojure's collections implement structural equality, which is really nice.  This allows sets or dictionaries to be keys in other dictionaries.  Then, whether or not the key is the same reference or a different reference (perhaps from a database), the hash and equality is the same.

In .Net, the situation is not quite as nice, though it can be implemented with [System.Collections.StructuralComparisons](https://msdn.microsoft.com/en-us/library/system.collections.structuralcomparisons(v=vs.110).aspx).

## Putting it all together

Functional state transitions where "state" is an immutable "value" offer some nice wins:

* No fear of deadlocks as one might expect from multi-lock scenarios like reader/writer locks.
* The "types" guide future maintainers in appropriate ways of transitioning state.  They *must* follow the established actor/atom approach because otherwise it won't compile. 
* Reduce complexity
  * By constraining assignment, each state transition is testable - the new state is a function of the old state.
  * Only certain state transitions are allowed.  State transitions are constrained so that the new state is a function of the previous state. With imperative mutation, sequence of events is critical.
  * Any thread that is using an old copy of the state can continue safely, (no more "InvalidOperationException: An element in the collection has been modified").  When done, the thread can look up the new state value instantly.
* Performance win around critical sections.
  * As an alternative to reader/writer locks, Atoms allow consistent, non-blocking reads (super-fast).  It doesn't matter to readers if the writer takes a while to create a new immutable state value because readers are not blocked.

## Conclusion

Functional approaches to state management, combined with immutable values provide some big wins:

  * It eliminates whole classes of bugs and maintenance problems
  * Reduced complexity - actors and CSP make state transitions explicit.  Fewer things can go wrong with immutable values.
  * Easier maintenance - the state is harder to misuse when it is an immutable value stored in an atom.  The type of the state reference (atom, Mailbox, etc.) enforces proper usage.

2016-09-27  By Jeremy Sellars

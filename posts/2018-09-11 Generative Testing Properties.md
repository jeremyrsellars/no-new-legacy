---
title: "Generative Testing Part 2 – Properties and Oracles"
date: "2018-09-11"
revised: "2018-11-11"
description: "'Properties' are the facts checked by generative testing.  We assert that some properties are true of our programs, and the testing library checks it for us."
tags: ["testing", "generative testing", "clojure.spec", "nUnit"]
categories : ["Programming", "Craft"]
---

From the plain English root verbs of "generative testing", we can surmise there are at least two actions to explore: 'generate' and 'test'.  In a nutshell, the actions are

1. Generate some random test cases.
2. Check that the software works.

For this post, let's start with the more familiar part: _testing_.


### Testing
If you've been around programming for any length of time, it will come as no surprise that 'testing' means checking that the program works as expected.  Most generative testing software is part of a testing framework that does a lot of things for you, like selecting which tests to run, executing the tests, reporting the results of the tests, perhaps including a reason for the failure, etc..  One of the intents of this post is to connect with concepts you may already be familiar with, like unit testing.  Many of the foundational concepts of generative testing can be applied in testing frameworks like nUnit and jUnit.

Let's start with a simple test about the contents of a string: does a string look like sheep bleat?  For our purposes, let's define sheep bleat as any string that starts with a `b` and is followed by at least 2 `a`s, making a string like "baa" or "baaaaaaaaaaaaaaaaa".  Further, let's say that only `b` and `a`s are allowed in sheep bleat, so `baad` isn't valid bleat.  If you are familiar with the textual pattern language called Regular Expressions, `^baa+$` would match all valid sheep bleat as defined earlier.

So a function signature might look like `bool isSheepBleat(string s){return false;}` or `(defn sheep-bleat? [s] false)`.

<style id='sheep-bleat' class='before-alternating-table'></style>

|Text      |Sheep Bleat?|Reason        |
|----------|------------|--------------|
|`b`       |false       |Too short     |
|`ba`      |false       |Too short     |
|`baa`     |true        |2+ `a`s       |
|`baaa`    |true        |2+ `a`s       |
|`baaad`   |false       |invalid `d`   |
|`·baa`    |false       |leading space |
|`baa·`    |false       |trailing space|

#### Parameterized Tests

You can probably imagine these tests in your unit testing framework of choice.  Perhaps each of these tests would be represented as a function asserting the expected true/false value.  Some frameworks let you specify the test code once, and then the test data can be delivered as a parameter to the tests.  That increases the signal-to-noise ratio, and helps you keep your eye on the data rather than the redundant function execution code.  If you haven't yet taken the opportunity to try parameterized tests in your framework, I believe it would be well-worth your time.


The parameterized test might look like this, which executes an assertion function for each example:

```clojure
(defn sheep-bleat? [s] false) ; obvious bug here!

(defn assert-sheep-bleat
  [text expected-answer reason]
  (is (= expected-answer
         (sheep-bleat? text))
    reason))

(def examples
 [["b"         false    "Too short"]
  ["ba"        false    "Too short"]
  ["baa"       true     "2+ 'a' s"]
  ["baaa"      true     "2+ 'a' s"]
  ["baaad"     false    "invalid ' d'"]
  [" baa"      false    "leading space"]
  ["baa "      false    "trailing space"]])

(deftest Testing_sheep-bleat?
  ; Run the test for each of the examples
  (doseq [[text expected-answer reason] examples]
     (assert-sheep-bleat text expected-answer reason)))
```

Here is C# with nUnit.

```csharp
    [TestFixture]
    public class A_Parameterized_Test
    {
        [TestCaseSource(nameof(SheepishExamples))]
        public void SheepBleatDetection(SheepishTestCase testCase) =>
            Assert.AreEqual(
                testCase.IsSheepBleat,
                Sheepish.IsSheepBleat(testCase.Text), testCase.Reason);

        static IEnumerable<SheepishTestCase> SheepishExamples =>
            new[] {
                new SheepishTestCase("b",         false,    "Too short"),
                new SheepishTestCase("ba",        false,    "Too short"),
                new SheepishTestCase("baa",       true,     "2+ 'a' s"),
                new SheepishTestCase("baaa",      true,     "2+ 'a' s"),
                new SheepishTestCase("baaad",     false,    "invalid ' d'"),
                new SheepishTestCase(" baa",      false,    "leading space"),
                new SheepishTestCase("baa " ,     false,    "trailing space"),
            };

        public class SheepishTestCase
        {
            public SheepishTestCase(string text, bool isSheepBleat, string reason)
            {
                Text = text;
                IsSheepBleat = isSheepBleat;
                Reason = reason;
            }
            public string Text;
            public bool IsSheepBleat;
            public string Reason;
            // nUnit requires a unique ToString() for a name.
            public override string ToString() => Text;
        }
    }
```

#### Parameterized Test (with an oracle)

<a id='parameterized_test_oracle'></a>

With unit tests, we may wish to come up with specific examples and test that against a known-correct answer, but with generative testing we don't have the opportunity to think through the correct answer (examples are generated randomly, after all).  We will want a way to compute the expected answer.  This is sometimes called an "oracle" (think the deity that heroes from mythology consult to answer a question).  For this contrived example, we have a ready-made oracle that would provide the correct answer for any string: the regular expression `^baa+$`

```clojure
;; Same examples, but with the answers no-longer known beforehand.

(defn assert-sheep-bleat
  [text reason]
     ; ^ Remove the correct answer; we have to compute it somehow.
  (is (= (some? (re-find #"^baa+$" text))  ; true when a match is found, false when nil is returned.
         (sheep-bleat? text))
    reason))

(def examples
 [["b"         "Too short"]
  ["ba"        "Too short"]
  ["baa"       "2+ 'a' s"]
  ["baaa"      "2+ 'a' s"]
  ["baaad"     "invalid 'd'"]
  [" baa"      "leading space"]
  ["baa "      "trailing space"]])

(deftest Testing_sheep-bleat?_with_oracle
  ; Run the test for each of the examples
  (doseq [[text reason] examples]
    (assert-sheep-bleat text reason)))
```

```csharp
    [TestFixture]
    public class B_Parameterized_Test_With_Oracle
    {
        [TestCaseSource(nameof(SheepishExamples))]
        public void SheepBleatDetection(SheepishTestCase testCase) =>
            Assert.AreEqual(
                Regex.IsMatch(testCase.Text, @"^baa+$"),
                Sheepish.IsSheepBleat(testCase.Text), testCase.Reason);

        static IEnumerable<SheepishTestCase> SheepishExamples =>
            new[] {
                new SheepishTestCase("b",         "Too short"),
                new SheepishTestCase("ba",        "Too short"),
                new SheepishTestCase("baa",       "2+ 'a' s"),
                new SheepishTestCase("baaa",      "2+ 'a' s"),
                new SheepishTestCase("baaad",     "invalid ' d'"),
                new SheepishTestCase(" baa",      "leading space"),
                new SheepishTestCase("baa " ,     "trailing space"),
            };

        public class SheepishTestCase
        {
            public SheepishTestCase(string text, string reason)
            {
                Text = text;
                Reason = reason;
            }
            public string Text;
            public string Reason;
            // nUnit requires a unique ToString() for a name.
            public override string ToString() => Text;
        }
    }
```

#### Parameterized Test (with properties, without an oracle)

Let's imagine we don't have an oracle to give us the right answers, but we do know some things that should always be true.  We know sheepish always begins with `b`, for example, which is easy to test.  We know it needs at least 2 `a`s (and since it starts with `b`, the length must be at least 3).  Let's look at a few such examples from the Sheepish specification.

```clojure
(defn sheep-bleat?
  [s]
  true) ; obvious bug here!

(defn assert-sheep-bleat
  [text reason]
  (testing (str `("sheep-bleat?" ~text) ": " reason)
    (when (< (count text) 3)
      (is (false? (sheep-bleat? text))
        "Bleat is always at least 3 characters."))
    (when (sheep-bleat? text)
      (is (string/starts-with? text "b")
        "Bleat always starts with `b`")
      (is (string/ends-with? text "aa")
        "Bleat always ends with `aa`")
      (is (= #{\a \b} (into #{} text))
        "Bleat only contains `a` and `b` characters"))
    (when-not (= #{\a \b} (into #{} text))
      (is (false? (sheep-bleat? text))
        "Bleat only contains `a` and `b` characters"))))
```

These facts should hold true for any test case applied to the system being tested.  Some only apply in certain circumstances.

**Note:** Since we didn't use an oracle, we may not have covered all the test cases.  In fact, the above tests won't fail for the function that always returns `false`!  The tests fail for the `(constantly true)` function, but not the `(constantly false)` function.

This is one reason I recommend starting with example-based unit tests before moving on generating additional test cases.  But, just because you supply specific examples and results, that doesn't mean you can't benefit from the property tests above in the parameterized tests.  The properties should hold true for the specific examples and the new, generated examples.

#### Why using an oracle may not be the best choice

* In many cases if you already have an algorithm that can create the right answer for a test case, there wouldn't be a reason to write new code.
* Often we don't start out with an algorithm known to work, but stakeholders have identified some things they expected to be true of the system (nicely packaged in a specification document, if we're lucky).
* Sometimes a specification may be incomplete or not internally consistent.
    * A specification I was working with recently explicitly forbade the number 1, but 0, 2, 3, 4, etc. were fine.  The problem was that the number 1 could actually occur (because of a revision in a different part of the document), and then the behavior was undefined.
* Sometimes an algorithm believed to work doesn't actually behave the way it is believed to – the properties don't hold true in some corner cases.  Maybe the algorithm is right and the spec is wrong, or vice versa.  Either way, it is helpful to identify this, and the earlier, the better.
* The oracle may be "expensive" (measured computationally, dollar-cost, network, or some other resource) to use the oracle.
    * Is it a legacy system running on virtual hardware in the cloud?
    * Is it a third-party payment system that will only tell you that the request was issued correctly by issuing the request?

# Next time

This will set us up nicely for when the examples are generated by the generative testing library/framework.  [In part 3]({{urls.base_path}}posts/2018-11-11-generative-testing-simple-generators/), we'll get into the fun part: generating test data.

# Source code

If you want to follow along, the source code is here:

* Clojure: https://github.com/jeremyrsellars/no-new-legacy/src/sheepish
* C#: https://github.com/jeremyrsellars/no-new-legacy/src/Sheepish.net

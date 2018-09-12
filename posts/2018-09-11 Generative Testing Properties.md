---
title: "Generative Testing Part 2 – Properties"
date: "2018-09-11"
description: "'Properties' are the facts checked by generative testing.  We assert that some properties are true of our programs, and the testing library checks it for us."
tags: ["testing", "generative testing", "clojure.spec"]
categories : ["Programming", "Craft"]
---

# Outline

* What are some simple properties?
  * A string matches a regular expression like s/baa+/ (as an oracle)
* What are some composite properties?



From the plain English root verbs of "generative testing", we can surmise there are at least two actions to explore: 'generate' and 'test'.  In a nut shell, the actions are 1) generate some random test cases, and 2) check that the software works.  For this post, let's start with the more familiar part: _testing_.


### Testing
If you've been around programming for any length of time, it will come as no surprise that 'testing' means checking that the program works as expected.  Most generative testing software is part of a testing framework that does a lot of things for you, like selecting which tests to run, executing the tests, reporting the results of the tests, perhaps including a reason for the failure, etc..  One of the intents of this post is to connect with concepts you may already be familiar with, like unit testing.  Many of the foundational concepts of generative testing can be applied in testing frameworks like nUnit and jUnit.

Let's start with a simple test about the contents of a string: does a string look like sheep bleat?  For our purposes, let's define sheep bleat as any string that starts with a `b` and is followed by at least 2 `a`s, making a string like "baa" or "baaaaaaaaaaaaaaaaa".  Further, let's say that only `b` and `a`s are allowed in sheep bleat, so `baad` isn't valid bleat.  If you are familiar with the textual pattern language called Regular Expressions, `^baa+$` would match all valid sheep bleat as defined earlier.

So a function signature might look like `bool isSheepBleat(string s){return false;}` or `(defn sheep-bleat? [s] false)`.

|Text      |Sheep Bleat?|Reason        |
|----------|------------|--------------|
|`b`       |false       |Too short     |
|`ba`      |false       |Too short     |
|`baa`     |true        |2+ `a`s       |
|`baaa`    |true        |2+ `a`s       |
|`baaad`   |false       |invalid `d`   |
|` baa`    |false       |leading space |
|`baa `    |false       |trailing space|

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

#### Parameterized Test (with an oracle)

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

(deftest Testing_sheep-bleat-with-oracle?
  ; Run the test for each of the examples
  (doseq [[text reason] examples]
    (assert-sheep-bleat text reason)))
```

This will set us up nicely for when the examples are generated by the generative testing library/framework.  Next time we'll get into the fun part: generating test data.

# Source code

If you want to follow along, the source code is here: https://github.com/jeremyrsellars/no-new-legacy/src/sheepish

### Generating examples


# To-do

Add composite test, perhaps `{:text "baaa!", :alarm true}`.
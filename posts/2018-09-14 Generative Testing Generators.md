---
title: "Generative Testing Part 3 – Generators"
date: "2018-09-14"
type: "draft"
description: "'Generators' build data used for testing.  We assert that some properties are true of our programs, and the testing library checks those properties hold for many possible inputs."
tags: ["testing", "generative testing", "clojure.spec", "nUnit"]
categories : ["Programming", "Craft"]
---

[In part 2]({{urls.base_path}}/posts/2018-09-11-generative-testing-properties-and-oracles/) of this discussion of "generative testing", we discussed skipped the first word, "generative", and went straight to "testing."  This time, we'll move back to automatic test-case generation.  

Unlike example-based unit tests, we may not already know the "correct" answer for any given data, especially if the inputs is generated randomly.  If we have an oracle, we can compute the correct answer.  If we don't, we can only test facts (properties) that should be true in every case.

From the plain English root verbs of "generative testing", we can surmise there are at least two actions to explore: 'generate' and 'test'.  In a nut shell, the actions are


---
title: "Generative Testing Part 1"
date: "2018-09-03"
revised: "2018-09-07"
description: Generative testing can help, whether you wish you had 100 monkeys to drive your web application, 10 million example transactions for load testing, or you want to gain confidence in your software without writing hundreds of example scenarios.
tags: ["testing", "generative testing", "clojure.spec"]
categories : ["Programming", "Craft"]
---

It is rare that a topic is smack dab in the middle of the Venn diagram of so many of my favorite topics: software quality, functional programming, and extensibility.  The topic of generative testing hits all three!  I am excited to bring this to your attention and shed light on it from my point of view.

Software quality, from the [Manifesto for Software Craftsmanship](http://manifesto.softwarecraftsmanship.org/) emphasizes 4 values, and I see each of them touched on in these topics surrounding generative testing:

* "Well-crafted software"
    * Yes, software needs to function correctly, and testing can help demonstrate this, but well-crafted software is open to extension.  Consider the scenario that stakeholders ask for a new version of software with an additional data field to be displayed.  It probably shouldn't require a complete rewrite of the app, right?  As an ideal, software should be extendable.  Also, a team's prior investment in tests shouldn't prevent or delay adding this feature.  In other words, in well-crafted software, both the product and the tests need to be extensible.
* "Steadily adding value"
    * Similar to the previous point, if testing the updated feature (just some new fields, right?) requires a rewrite of the tests, that change to the tests will probably take longer than the feature itself.  (I don't know about you, but that mucks up my estimates).  Generative testing supplements traditional example-based unit tests by making its own examples; a new field and generator can be declared once and then the generator takes over to "fill in" the new field.
* "A community of professionals"
    * With your help, this topic meets this point, too.  You, the reader, are participating and are hopefully benefitting from this community of professionals.  I am excited to see if this content adds to your excitement about these topics and helps you accomplish your goals.  Feel free to let me know on twitter, etc..
* "Productive partnerships"
    * One of the great helps from all things "generative" is that generative models can build examples.  Sheep say "Baa!", right?  Or is it "Baaaaaa"?  Consider a regular expression that specifies what "sheep-bleat" looks like in text: `baa+`.  Does "baaaaa" match the sheep-bleet specification? yes.  Howabout "bowow"? nope.  This ability to evaluate content against a specification is useful (in a reductive sense), but what if you had a way to derive many mega-bleats (I mean megabytes) of example text?  That could help you talk with the stakeholders and show examples to make sure everyone is bleating the same language.  As the saying goes, a picture is worth a thousand words.  Think about software requirements/specification documents.  Rather than go back and forth with stakeholders about some line of text in a spec doc, it may be far more productive to "show" examples that are possible based on a translation of the document into a generative specification.

# Series

In this series, we'll discuss:

1. Brief introduction to combinators.  What is a spec? What do the generators create by default? How do we override the default generation to suit our needs?
2. Generative testing vs. DIY (example-based) unit tests.
    * It does this through synthesizing many examples, often exposing bugs, flaws, or problems in the underlying mental concept of a solution design. 
    * Spec-generated data in a data-oriented language like Clojure is especially powerful.
3. Specs and Combinators
4. Examples of attributes that can be verified

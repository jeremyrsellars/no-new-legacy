---
title: "Decisions are always in context"
date: "2019-02-14"
description: "Long before writing code, there are many decisions to make.  Each decision can have a profound and lasting effect, whether thoughtfully considered or not.  When teams share a common vocabulary for decisions and the context in which decisions are made, decisions can be made more effectively."
tags: ["decisions", "context"]
categories: ["Professionalism", "Team"]
---

Summary: Long before writing code, there are many decisions to make.  Each decision can have a profound and lasting effect (you might even say a legacy!), whether thoughtfully considered or not.  When teams share a common vocabulary for decisions and the context in which decisions are made, decisions can be made more effectively.

## Decisions

There are many kinds of decisions, which is obvious to most anyone over the age of 3.

<style>
.label_box {
  padding: 3px 6px;
  margin-left: 2em;
  margin-top: 3px;
  background: #eee;
  color:#555;
  border-radius: 3px;
  text-decoration:none;
  border:1px solid #ccc;
  display: inline-block;
}
</style>

* What should I eat today? <span class='label_box'>Personal</span>
* Should I use an `if` statement or a `switch` statement? <span class='label_box'>Programming</span>
* Should I use Rust or Python for this task?  <span class='label_box'>Tech</span>
* Should I use Lucene or roll my own search capabilities?  <span class='label_box'>Tech</span>
* How should I test that this part of the system works?  <span class='label_box'>Programming</span>
* Should I write this part of the system for Linux or AWS Lambdas?  <span class='label_box'>Tech</span>
* Should we offer our software as pay-per-version or use a subscription SaaS model?  <span class='label_box'>Business</span>

There are probably many ways of categorizing questions and here are a few categories:

* Technology choices – platform, language, libraries & frameworks
* Business model – pricing model, target business sector, project scope vs. budget vs. calendar time
* Personal decisions – ongoing education and edification, professional development, time spent with friends and family, dessert vs. the gym
* Programming/task decisions – coding style, what to do first, PowerShell vs. C#
* And more!

## Context

There are few choices that can be made universally without an understanding of the situation or place or time.

Let's take an example from everyday life.

**Should I have dessert or go work out?**

* Did your doctor tell you to avoid sugar at all cost?
    * Maybe go to the gym.
* Are you on a Valentine's Day date?
    * Maybe go have a romantic sweat together at the gym. (J/K ;-)
* Are you on vacation?
    * Maybe let your mouth have some fun, too.
* Is your mission in life getting out of credit card debt?
    * Maybe skip the restaurant and make dessert at home.
    * Or, go get a second job at the gym.

If healthy choices are always on your mind, you may naturally gravitate to the healthier options and not even consider other factors.  Similarly, if financial goals are at the front of your mind, it's easy to miss the other considerations.

## "Priming" the brain

This reminds me of a story I heard somewhere (probably on a podcast, but I can't remember the source).  Here is a recounting similar to what I remember hearing, but I haven't read the book or the original study.

> John Bargh and his colleagues carried out a marvelous experiment of this sort. The participants had a test of their language proficiency in which they had to make up sentences from lists of words. For example, they had to use four of these words to make a sentence: "be will sweat lonely they."
>
> Hence, they might respond: "They will be lonely."
>
> The participants made up a series of thirty sentences in this way. And that, for them, was the end of the experiment. The lists of words, however, contained many words that suggest the stereotype of elderly individuals, e.g., "lonely" the list above, and other words such as, "old" and "gray". As psychologists say, these words prime the stereotype, that is, make it more active even if the participants are not aware of its activity. After the experiment was over, the participants walked down the corridor to the elevator. Those who had received the primes to old age walked at a slower pace than those who had carried out a similar sentence-making test that did not prime old age. Yet, the slow walkers were neither aware of the primes to old age, which contained no words referring to slowness, nor of the effects of these primes on their behavior. And, as a subsequent study showed, the primes did not affect their emotions, at least not in a conscious way. You might say that they haven't made a proper inference, but an unconscious association.
>
> From _How We Reason_ by Philip Nicholas Johnson-Laird (pp. 61-62).

My point in referencing this obscure topic is that there are influences, both subjective and objective, both conscious and unconscious, that may affect our thoughts on some subject.

### Case Study

Let me try to illustrate how this could work in a real world situation I encountered recently:

#### Problem description

What we *think we know* and some of what we *know we don't yet know*:

* Users responsible for diagnosing problems in communication between 2 systems want to see the contents of messages passed between them. 
* Our product already offers similar functionality for a different data format.
* Because the domain of the new data is related to medical insurance and similar functionality is related to clinical data, it is less likely that there will be overlap in persons viewing the clinical data will also view the insurance data.
* The most natural place to put the new functionality is right next to the old functionality, even on the same screen.
* If you squint, some of the new data looks like the old data, but the new data may have far richer metadata.  We don't know if users would benefit from the richer metadata.

#### Possible influences

Putting on my "meta hat," I identified some of what might have influenced my thinking about how to proceed.  Here is what I came up with:

* Unconscious:
    * The existing implementation used some tech that I am not super familiar with and don't especially want to learn because it isn't likely to be very helpful in my future.
    * An associate recently gave a technical talk about a new technique that I've been wanting to try – a way allocate less memory.  (If you are curious, it's `ReadOnlyMemory<T>` from .Net's System.Memory Nuget package.)
    * At first glance, the problem description seems well suited to my favorite programming language (because almost everything is well-suited to my favorite programming language! Why do you think it's my favorite ;-)
* Conscious:
    * "Partially because the relevant screen is buried behind a login page and many clicks, the quickest way to explore the possibilities is *not* to jump into the code and add it to the product, instead it's to write a prototype."
    * "My prototype code is in ClojureScript, so I guess it would be quicker to polish that up and put it in with the C#."

Here are some contrived examples that didn't apply in my situation, but I can easily imagine:

* "I'm most fluent in this language."  <span class='label_box'>Personal</span>
* "In last week's meeting, the UX team reminded everyone of the goal for all click handlers to finish in 10ms or show a progress indicator, but the existing code was implemented before that goal took effect and is still slow." <span class='label_box'>Programming</span>
* "In the last formal review of my code, Readable Code Rule #18 was mentioned and I want to make sure to practice doing that right. The existing code also violates that, so if I rewrite it, it will look good in the review." <span class='label_box'>Team</span>
* "I'm in the mood to write something from scratch." <span class='label_box'>Personal</span>
* "I recently received feedback suggesting my work is slower than average. I think rewriting this in a cleaner way would be best, it will take longer and I don't want to be seen as slow." <span class='label_box'>Personal</span>
* "I have vacation next week and I want to finish before then, so I'll go with the quicker option." <span class='label_box'>Personal</span>
* "My team encourages cross-training, so if I use this other technology, I can work with someone on another team." <span class='label_box'>Team</span>
* "The annual patient privacy meeting reminded me of the fines associated with accidental leaks of Patient Health Information (PHI).  There is not development environment on the server with access to the data.  The C# compiler is on every modern Windows computer, so I can hop into notepad and start coding without copying PHI onto my less-secure development workstation." <span class='label_box'>Business</span>

I labeled these, but I can't peer into anyone's soul to identify any of the real motivations that might be at play.  For example, there could be any number of ways to complete the sentence:

* "I am most fluent in this language...
    * so my best work will be in that language. <span class='label_box'>Personal</span>
    * and I know it is a great fit for the solution. <span class='label_box'>Programming</span> <span class='label_box'>Tech</span>
    * so I will probably help people sooner and deliver value to the business/customer quicker. <span class='label_box'>Business</span>
    * so I can coast. <span class='label_box'>Personal</span>
    * and my team is depending on my getting done quickly. <span class='label_box'>Team</span>
    * so I want to get experience in something different. <span class='label_box'>Personal</span>

Surely there is justified time and place for each of these motivations.  For example, "Coasting" is great after a huge project, during an otherwise crazy season of life, family, or business, or when areas of life are more important.  Since many in our industry are young it can be easy to forget that motivations change as life goes on (children, grandkids, health, retirement).

### Digging deeper

As individuals, it can be difficult to look at a problem from all angles.  Teams help address this by bringing several perspectives to the discussion.

So how might we think about solving this problem?  Ask more questions, of course?

* What could the data look like?  Are there better ways of visualizing it?  Are there easier/quicker ways of visualizing it? <span class='label_box'>Business</span>
* How valuable is the functionality? Since there may be a different user base, would it be worth tailoring the view to that user base and pricing that new functionality separately? <span class='label_box'>Business</span>
* Does this project merit months of research to get questions answered or is it an option to get something out there quickly and iterate based on user feedback? <span class='label_box'>Business</span>
* How much code can be reused?  Do we like the code that can be reused?  Is now a good opportunity to improve the reused code?  <span class='label_box'>Programming</span>
* Is this an appropriate time to try out a novel approach/library/tool that I have wanted to try? <span class='label_box'>Personal</span>
* Is this most likely a "write once, extend never" feature or something to be built upon in the future?  Will future enhancements be done by my team and I, or by others?  <span class='label_box'>Team</span>

## More of the same?

One of the most pervasive unconscious influences is the "recent past"... I haven't found an adequate word yet, so let me describe this concept in 4 pieces!

* The "recent past" – The ideas that we've spent the last few weeks thinking about are readily available at the forefront of our minds.  Remember back in school when you had to take the final exam?  Weren't the recent topics often easier to remember than the ones from the start of class?
    * This can manifest in all sorts of areas, not just programming techniques and technology choices.  Consider the process of developing a business model for an additional product or service.  "We already have lawyer-approved contracts for subscription-based SaaS and are accustomed to that kind of pricing and support model, so let's just use that again.  It worked before!"
* "Momentum" – You are on a roll and maybe the whole team is running with you.  Don't forget to pause and think before doing "what we always do" simply because it worked before.
    * Maybe the team is familiar with REST-based APIs using WebApi which worked great for a desktop app, so that's the easy choice for the next round of mobile APIs (forgetting about the constraints of download speed, memory usage, slower round-trips, the need for bite-sized views because there is less room to display all that data, etc.).
    * Momentum can be good, but sometimes it can make one skip over a better solution.
* "Familiarity" – I usually go to the closest grocery store to my house, even though there are other stores with better selection.  It is more convenient and I know where most of the usual suspects are located.  It is both familiar and convenient.
    * In programming, your team's choice of languages has a profound impact on what is familiar.  The standard library is probably familiar.  The idioms are familiar (the things the language/framework make easy to express).  Team members and help available from StackOverflow, for example, are likely to already know about the big ideas in the language or tech stack.
    * Some of us remember when almost every dialog window had a series of "tabs" where you could select from rows of tabs and find what you were looking for.  These fell out of style but they were familiar.
    * Familiar doesn't mean good.
* "At hand" – The tools or ideas that are readily available are convenient to use.  They can be tempting even if a better-suited tool exists.
    * Maybe the data one wants to report on is on a server that has SQL Server and no other data stores.  It may be tempting to build a partial solution with SQL that can't easily go all the way because a more functional paradigm would fit the problem better.  Don't tell me you can build a good intranet search engine with SQL like this: `SELECT url, description FROM pages WHERE text LIKE @SearchString`.
    * Maybe I can turn some screws with the screw driver in the kitchen junk drawer, but if I'm hanging drywall or building a deck, I am going to the store and buying a power drill.
    * Sometimes the tools at hand well-suited to the job, but doing a lot of work with the wrong tool is frustrating, slow, error-prone, or generally ineffective.

## Trade-offs depend on context

A power drill is better. It is the preferred technology, right?  ;-)

Let compare merits of the screwdriver vs. power drill.

<style class="before-alternating-table"></style>

|Consideration|Hand screwdriver|Power drill|Winner|
|:------|:------|:------|:------|
|Speed|slow|variable|Power drill wins for driving many screws|
|Power|weak|strong|Power drill wins for driving long screws|
|Size|small|large|screwdriver wins for tight spaces and small drawers|
|Weight|small|large|screwdriver wins for backpacking/EDC|
|Cost|$|$$|screwdriver wins for cheap gift to nephew|

## Vocabulary

Here is a starting point for shared team understanding of concepts at play in decision making processes.

* **Conscious influences** – Ideas, priorities, motivations, people, and pressures that push us in one direction or another... that we are aware of and can reason about.
* **Unconscious influences** – Ideas, world view, motivations, and pressures that, until we are aware of them, push us in a direction without our knowledge.
* **Priming** – Preparing the mind by thinking about innovative, creative, exemplar, or otherwise inspirational ideas.
* **"Known Unknowns" and "Known Unknowns"** – Some things that we don't know (but wish we did) can be listed (how many customers could benefit from this proposed feature?).  Other things we don't even know we need to know, like that the author of an open source project was going to quit and no further progress on bugs or features.
* **Personal**, **Programming**, **Tech**, **Team**, and **Business** concerns or motivations.
* **Trade-off** – a compromise acknowledging the arguments for and against a choice.
* **Time box** – the time allocated or available to make the decision.
* **Problem statement** – A description of problem that includes the relevant details of the feature or issue to be addressed.  It brings clarity to know "What are we trying to solve?"
* **Scope of decision** – When, where, how long, etc. that the decision applies.  This may include a scheduled meeting to revisit the decision and evaluate newly acquired information.

## Conclusion

I hope this rambling has illustrated how every decision is made in context and should be treated as such.

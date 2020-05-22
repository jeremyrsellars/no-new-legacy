---
title: "Data formats and the Software System Bubble"
type: draft
date: "2020-05-22"
description: "Long before writing code, there are many decisions to make.  Each decision can have a profound and lasting effect, whether thoughtfully considered or not.  When teams share a common vocabulary for decisions and the context in which decisions are made, decisions can be made more effectively."
tags: ["decisions", "context", "bubble"]
categories: ["Professionalism", "System design"]
---

Summary: Picking a data format really starts to matter when communicating with software outside your application's bubble.  What is a bubble?  Why does it matter?  What can you do?

## Software bubble

You have probably noticed that part of the world you have fine-grained control over; and the rest, you don't.  This is particularly true in software.

The best definition I can think of for bubble is: The parts of the system that is deployed together at the same time.  Depending on your situation, you may be able to fudge parts of that definition, but I think that gets the point across.

* **The parts of the *system* ** -- multiple parts of software working together to accomplish a goal.
* **That are *deployed together* ** -- hopefully you have a one-click build that builds all the parts of the system into a single package or related packages.  Whether this is single uberjar, a huge Visual Studio solution, or a Jenkins build that pulls a bunch of micro-service packages and tests them together, the thing that matters here is oversight.  Many languages give you cool guarantees like "this value can never be out of range" or "that will never be null" and these are built into the language and execution environment.  These are in your control.
* **at the *same time* ** -- Perhaps in your situation this can be almost the same time, or "it's ok if it's eventually consistent," or "the users expect a downtime until they upgrade their client software when the server software is upgraded."


## Data formats and serialization 

I have come to like some things about the Transit Format (https://github.com/cognitect/transit-format) as an alternative/kinda-superset of JSON.  It's a bit tough to describe in a sentence, but it has all the things you love about JSON (plus real datetimes, URIs, and extensibility to adding new semantic types), and then 3 different ways of encoding the data, depending on server and client capabilities (binary as MessagePack, JSON (a smaller=faster encoding), and JSON-verbose a readable verbose encoding (that is a lot like what you would think of as JSON).  When I say "encode" I mean 3 different representations that are losslessly interchangeable, depending on the destination floating point support, etc..

The churn is really low which I see as a huge positive.  The spec hasn't changed in 5 years!  I used it in Up Dashboard for almost all the inter-process communication and would use it again, where appropriate.

Transit is not a serializer and it makes no attempt to preserve "identity" (as in ReferenceEquals), instead you serialize/deserialize things like native Dictionaries/Lists/strings/integers/DateTime/etc...  It supports custom handlers that can convert the raw data into your data classes, but you don't have to -- everything will still work.  Transit is good for cross-language interop and interop with things that aren't inside your Visual Studio solution or one-click build, like prototypes or 3rd party clients using an API.  This makes it more suitable than gRPC or protocol buffers to ride out versioning of APIs and formatters (here's looking at you, System.Text.Json), etc. without forcing clients to adapt the new idiosyncrasies.

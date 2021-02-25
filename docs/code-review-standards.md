# Code Review Standards
In this document we are going to explore the main things we look for in a code review, and what do they mean. If you do not have a lot of familiarity with code reviews please check out the following links before proceeding:

* https://www.ideamotive.co/blog/code-review-best-practices
* https://github.com/google/eng-practices

## What to look for?

### Ensure the general approach for the feature/fix/refactor is the proper one
The reviewer has to ensure that the general approach of the intended change is the proper one. 

Sometimes, the best solution isn’t viable because we have urgency or the solution is too expensive to implement right now. If this is the case we generally agree on creating an issue to address the change later as technical debt. 

### Keep our code consistent

#### Code and architecture style

Aside from using a linter, there are many details to look for in a pull request that can't be automated. The contributions are coming from many of us, but the code has to be felt as if it was written by a single person. 

This said, any contribution should be curated using the [coding guidelines]() document.

#### Approach consistency

In a large codebase it's usual to stumble upon a same problem twice. Be an algorithmic, API or architectural issue, we have to make sure two equivalent issues have the same solution applied. 

This has many benefits, including: 

* **Developer friendly codebase:** if any collaborator already looked at this solution elsewhere, she will already be familiar with it in a new place.

* **High productivity:** we can reach a toolset of well known solutions.
* **High quality solutions:** many collaborators are looking for consensus on the best solution possible for a particular common problem.   


### Prevent performance and bugs regressions
* Smoke testing.
* Performance tests (TBD).
* Proper usage of API.
* Unit testing coverage.



### Prevent unnecessary technical debt
* Simplificación de algoritmos
* Análisis de escalabilidad de sistemas
* Comentarlo y crear issues para atacarlo después


### Improve team’s knowledge of the code base
By requesting a code review, the knowledge of the changes are shared and the collaborators will stay up to date with the codebase. This is important for our productivity as this enables informed decision making and effective ownership. 

Always remember that the collective excellence reigns over the individual. 


### Special Considerations

### Big pull requests



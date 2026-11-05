# signal-to-opportunity (Claude Skill)

A Product Discovery Research Assistant. It helps a Product Manager turn manually collected customer signals into an evidence-backed, testable Discovery Brief.

> Automate the research groundwork. Amplify the Product Manager.
> AI organizes and challenges the evidence. The Product Manager makes the decision.

## What it does

Customer Signals → Theme Detection → Product Capability Validation (if documentation supplied) → Discovery Brief → Optional Internal Discovery Review → Hypothesis challenged or refined → Customer and Internal validation questions → Product Manager decision.

For each theme the brief contains: theme, supporting Signal IDs, facts, inferences (marked unvalidated), counter-evidence, opportunity hypothesis, What Must Be True, Riskiest Assumption, key unknowns (functional and, where relevant, non-functional), prioritized discovery questions, and optionally capability validation and internal review.

It keeps three evidence classes separate: Customer Evidence, Product Documentation Evidence, and Internal Expert Evidence. It does not recommend features or solutions, and P0/P1/P2 is *learning* priority, not product or roadmap priority.

## Inputs

**Required**
- Customer-signal Excel file with columns: Signal ID, Source, Customer, Product Area, Feedback (same structure as `CustomerSignals.xlsx`).

**Optional**
- Product documentation: uploaded PDF, uploaded text/Markdown, or an accessible link (only where Claude has permission and an approved connector). If a link is protected, the Skill says so and asks for an uploaded PDF or page-marked text.
- Internal expert feedback: Internal Feedback ID, Reviewer role, Feedback text, Related theme, optional reference/context. Reviewer names are not needed and are not inferred.

## How to invoke

Start Claude Code in this project folder (so the project Skill is found), attach or point to your files, and ask in plain language, or name the Skill, for example `/signal-to-opportunity`.

## Example prompts

- "Use signal-to-opportunity on CustomerSignals.xlsx and give me a Discovery Brief."
- "Analyze these customer signals, then check the themes against this product PDF."
- "Here is internal feedback (IE001-IE004). Run an internal discovery review on the Discovery Brief."
- "Turn the brief into an HTML Artifact. Show me the contents before publishing."

## Evidence and policy limitations

- The signals you provide are the only customer evidence. Anything beyond them is an inference or an unknown.
- "Not Found" in documentation means "not located in the provided documentation," never "the product lacks it."
- Internal expert feedback is an unverified assertion and is never counted as an additional customer vote.
- Do not use real customer or confidential data unless it is permitted under applicable Hyland AI and data-use requirements.
- The Skill will not bypass blocked tools, connectors, or permissions.
- Artifacts are only published after you have reviewed the contents.
- This is a research aid. It makes no claim of production readiness, measured time savings, or improved decision quality.
- The bundled reasoning and schema files are copies of the originals. The Skill's extra requirements (riskiest assumption, non-functional unknowns, P0/P1/P2) go beyond what the baseline schemas define.

## Skill vs. the local web application

| | Skill | Local web app (`SignalToOpportunity`) |
|---|---|---|
| Runs in | Claude Code / Claude | A local .NET application |
| Interaction | Conversational; can read PDFs and approved links | Fixed pipeline and screens |
| Output | Brief in chat, optional HTML Artifact | App screens |
| Relationship | Reuses the same reasoning files and schemas | Original, unchanged |

The Skill does not change, call, or depend on the web application.

## Contents

- `SKILL.md`: instructions Claude follows
- `references/`: copies of `DISCOVERY_LOGIC.md`, `CAPABILITY_CHECK_LOGIC.md`, `INTERNAL_REVIEW_LOGIC.md`, `discovery-brief-schema.json`, `capability-check-schema.json`, `internal-review-schema.json`

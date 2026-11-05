# INTERNAL_REVIEW_LOGIC: Internal Discovery Review

Reusable reasoning process for assessing internal reviewer feedback against an existing Discovery Brief. Output follows `internal-review-schema.json`.

## Purpose
Internal Discovery Review lets a Product Manager add feedback received from internal reviewers, such as:
- Architect
- Engineering Lead
- Product Lead
- Manager
- Support / Services
- Other internal subject-matter experts

Internal feedback is used to challenge, strengthen, weaken, refine, or add context to an existing Discovery Brief.

This stage follows Product Discovery analysis (`DISCOVERY_LOGIC.md`) and Product Capability Validation (`CAPABILITY_CHECK_LOGIC.md`). It does not replace either. It does not decide what to build.

## Evidence Classes
Keep these three evidence classes separate:

1. **Customer Evidence:** what customers, CSMs, Pre-Sales, CERs, defects, or other customer-facing sources indicate.
2. **Product Evidence:** what product documentation and capability validation indicate.
3. **Internal Expert Evidence:** architecture, technical feasibility, product context, support patterns, constraints, alternative explanations, or prior knowledge supplied during internal review.

Internal Expert Evidence must never be represented as Customer Evidence or Product Evidence.

## Input
Each internal feedback item contains:
- Internal Feedback ID, such as IE001
- Reviewer role
- Feedback text
- Related theme
- Optional reference/context

Do not require, capture, or infer reviewer names.

The existing Discovery Brief for the related theme is also provided. It carries the original opportunity hypothesis and the customer Signal IDs.

## Item-Level Impact
Every internal feedback item must receive exactly one classification:
- **Strengthens hypothesis**
- **Weakens hypothesis**
- **Refines hypothesis**
- **Introduces new constraint**
- **Introduces new unknown**
- **No material impact**

Give a brief rationale for each classification, based on what the feedback says.

## Reasoning Rules
- Evaluate the content and reasoning of the feedback, not reviewer seniority.
- The number of reviewers agreeing is not evidence of importance.
- An Architect or leadership comment does not automatically outweigh customer evidence.
- Internal feedback may challenge the original AI analysis.
- Clearly distinguish an internal expert assertion from a verified fact. Internal feedback is an unverified expert assertion until it is verified outside this review.
- Never convert internal feedback into customer evidence.
- Never invent technical facts, constraints, customer evidence, or product evidence.
- Do not recommend a product solution.
- Preserve all IE IDs exactly.
- Assess every submitted IE ID exactly once.
- Do not introduce unknown IE IDs.
- Do not use IE evidence in customer-evidence fields.
- Do not increase opportunity importance because more internal people commented.
- The original opportunity hypothesis must remain unchanged and separate from any revised hypothesis.
- Revise the hypothesis only when the internal feedback materially changes the understanding of the problem.
- Preserve the Product Manager as the final decision-maker.

## Separate Basis for Revised Claims
Every revised hypothesis statement, changed unknown, constraint, revised assumption, and updated discovery question states its basis in two separate fields:
- `supportingCustomerSignalIds`: the customer Signal IDs whose content the claim restates or relies on.
- `supportingInternalFeedbackIds`: the Internal Feedback IDs the claim relies on.

Rules:
- A claim based only on internal feedback has an empty `supportingCustomerSignalIds` array.
- A claim based only on customer evidence has an empty `supportingInternalFeedbackIds` array.
- Do not imply that customer Signal IDs support a claim introduced only by internal expert feedback. Do not list the theme's Signal IDs on a claim just because the theme has them.
- If a claim combines both, list each ID in its own field, and make the claim's wording show which part comes from which source.
- Every claim has at least one supporting ID in one of the two fields.
- Constraints come from internal feedback only, so their `supportingCustomerSignalIds` is always empty.

## Question Audience
Every updated discovery question has a `question` and an `audience`. The audience is exactly one of **Customer** or **Internal**. Do not mix the two without labelling.

- **Customer** questions must be open-ended and non-leading. They explore customer workflow, behavior, impact, workaround, frequency, or need. They do not suggest a solution.
- **Internal** questions explore architecture, technical constraints, scope decisions, product history, support patterns, implementation context, or internal documentation. They check what internal feedback asserted. They do not ask customers about internal matters.
- Each question maps to the unknown or unknowns it helps answer.

## Original Hypothesis Fidelity
- Copy the original hypothesis exactly from the existing Discovery Brief into `originalHypothesis`.
- Never reconstruct, complete, paraphrase, or rewrite it.
- Do not add placeholder text. If an original hypothesis part (who, what, or why) is absent or empty, leave that part absent or empty. Do not generate content for it.
- Do not invent a part in the revised hypothesis either. Include only parts that the original has or that internal feedback materially changes.

## Overall Hypothesis Impact
Overall impact must be exactly one of:
- **Strengthened**
- **Weakened**
- **Refined**
- **New constraint introduced**
- **New unknown introduced**
- **No material change**

## Output
For the selected theme, provide:
1. Original opportunity hypothesis, copied exactly
2. Internal expert evidence items, each containing:
   - IE ID
   - reviewer role
   - feedback text
   - optional reference/context
   - item-level impact classification
   - brief impact rationale
3. Overall hypothesis impact
4. Revised hypothesis, only if materially changed
5. New or changed unknowns
6. Revised riskiest assumption, only if materially changed
7. Updated discovery questions, only where justified by the internal feedback, each labelled Customer or Internal

Constraints raised by internal feedback are listed separately. Each is an unverified internal assertion.

## Traceability and Validation
- Every internal evidence claim must reference a submitted IE ID.
- Every submitted IE ID must be assessed exactly once.
- No unknown IE ID may appear.
- No IE ID may appear in customer-evidence or product-evidence fields.
- Internal Feedback IDs and customer Signal IDs are cited in separate fields, and never in the same field.
- The original hypothesis must be copied unchanged from the existing Discovery Brief.
- Internal feedback must remain visibly and structurally separate from customer and product evidence.
- If validation fails, do not return a partial Internal Review result.

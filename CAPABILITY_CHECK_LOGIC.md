# CAPABILITY_CHECK_LOGIC: Customer Need → Product Documentation

Reusable reasoning process for checking whether a customer need may already be covered by documented product capability. Output follows `capability-check-schema.json`.

## Scope
This is Product Capability Validation. It runs before, and separately from, Product Discovery analysis (`DISCOVERY_LOGIC.md`).
- It answers one question: what does the provided documentation say about the requested capability?
- It does not group signals into themes, form opportunity hypotheses, write discovery questions, or recommend solutions. Those belong to Product Discovery.
- It does not judge whether the opportunity is worthwhile.

## Evidence Boundary
Two kinds of evidence exist, and they are never mixed:
- **Customer-signal evidence:** what the customer signals state (cited by Signal ID).
- **MRG evidence:** exact text from the provided product documentation (cited by page or section).

Anything else is an interpretation or an unknown, and must be labeled as such.

## Process

### 1. Start from the customer need
- Begin with the requested capability or underlying need, as the customer signals state it.
- Cite the customer Signal IDs. Do not add detail the signals do not contain.

### 2. Search only the provided documentation
- Look for evidence only in the documentation text provided.
- Do not use outside knowledge of the product, other versions, or general industry norms.

### 3. Prefer explicit evidence
- Explicit text that directly describes the capability outranks related or implied text.
- Mark each piece of evidence as explicit, related, or implied.

### 4. Never invent
- No made-up documentation text, page numbers, sections, or capabilities.
- If the documentation does not say it, it is not evidence.

### 5. Assess using only these values
Compare the customer's intent and outcome with what the documentation explicitly says. Matching words or terminology is not enough.

- **Supported:** use only when the documentation explicitly describes the same capability, the same user action, and the relevant outcome the customer requested.
- **Partially Supported:** use only when the documentation explicitly covers a meaningful subset of the requested capability or outcome. A merely related feature is not enough.
- **Unclear:** use only when the documentation addresses the same kind of user task or goal as the customer need, but differs in scope, object, workflow, or outcome.
  - Example: searching inside an open document versus finding a document across a repository. This is Unclear, because both are search or find tasks that differ in object and scope. Unclear unless stronger evidence exists.
  - Example: extracting one PDF page versus splitting a multi-page PDF into several documents. This is Partially Supported, not Unclear, because extraction explicitly covers a meaningful subset of the requested page-separation task.
- **Not Found:** use when the documentation does not address the same user task or goal as the customer need. No relevant evidence for the requested capability is located in the provided documentation. Not Found never means Unsupported (see rule 6).
  - A shared product surface, screen, viewer, module, file type, or keyword is not enough to establish related evidence.
  - Example: zoom, pan, or rotate in a document viewer provides no relevant evidence for comparing two documents side by side. This is Not Found.

Decision rules:
- Do not upgrade Unclear to Partially Supported merely because keywords overlap.
- Ask four questions of the documented behavior: is it the same capability, the same object, the same scope, and the same outcome as the customer need? Only "yes" to all four, for the whole request, allows Supported. A "yes" for a meaningful part of it allows Partially Supported. Related-area text with a different scope, workflow, object, or outcome is Unclear. Nothing relevant is Not Found.
- When uncertain between two assessments, choose the more conservative one, meaning the one that claims less support (Supported, then Partially Supported, then Unclear).
- Related evidence requires a meaningful match in user intent or task, not merely a shared product area.
- When deciding between Unclear and Not Found, ask: "Does the documented behavior help accomplish substantially the same user task?" If no, choose Not Found. If yes, but the scope, object, workflow, or outcome differs, choose Unclear.
- Conservatism never hides evidence. If documentation for substantially the same task exists, the answer is Unclear, not Not Found. Cite it as related evidence and say what it covers and why it does not match. Text that fails the question above is not evidence, so it is not quoted.
- Assessment and Evidence Strength are separate concepts. Assessment says how much of the customer need the documentation covers. Evidence Strength says how explicit and direct the cited text is. Decide each on its own. For example, a Supported result rests on explicit text and so is usually High, a Partially Supported result can be High for the part it covers while the assessment stays partial, and Unclear and Not Found always carry Low strength because there is no direct evidence to rate.

### 6. Not Found is not Unsupported
- "Not Found" means only "not located in the provided documentation."
- Absence from the documentation is not proof of absence from the product. The documentation may be incomplete, out of date, scoped to a different product or version, or worded differently.
- Never state or imply that the product lacks the capability.

### 7. Rate evidence strength
- **High:** explicit documentation directly describes the requested capability.
- **Medium:** the documentation is relevant but only partially or indirectly matches.
- **Low:** the evidence is weak, ambiguous, or implied.
- Explain the rating in plain language. Do not use numerical confidence or percentages.

### 8. Keep evidence separate
- Customer-signal evidence and MRG evidence appear in separate fields and are never merged.
- Do not paraphrase documentation text as if it were customer evidence, or the reverse.

### 9. Consider what an apparent match may mean
Even when the capability looks documented, consider whether the customer's request may instead indicate:
- **Discoverability or awareness:** the capability exists but the customer does not know about it or cannot find it.
- **Configuration:** it exists but must be enabled or set up.
- **Permissions:** it exists but is restricted for this customer's role.
- **Workflow or usability:** it exists but does not fit how the customer works, or is hard to use.
- **Incomplete capability:** it exists but covers only part of the need.

State these as unvalidated possibilities for the PM, not conclusions. Where the schema offers no matching value, record the possibility in the explanation. Map permissions to "Possible configuration issue" and incomplete capability to "Possible missing capability", with the nuance stated in the explanation.

### 10. Do not assume the problem is solved
- Never conclude that a documented feature fully solves the customer's underlying problem. That needs validation with customers and product data.
- A documented capability shows only that the documentation describes it. It does not show that it works for this customer or meets their need.

### 11. Require exact evidence
- Quote documentation text exactly as written. No paraphrase, trimming that changes meaning, or merged quotes.
- Give the page number or section heading exactly as provided. If it cannot be identified, say "Not identifiable from the provided text." Never guess.

### 12. When in doubt, do not guess
- If the evidence is insufficient, choose "Unclear" or "Not Found" instead of stretching weak text into support.
- Not Found and Unclear carry no MRG evidence, Low strength, and the interpretation "Needs further investigation".

## Output: Capability Check
For each customer theme, per `capability-check-schema.json`:
1. Theme and customer Signal IDs
2. Requested capability
3. Assessment
4. MRG evidence (exact text, page or section, directness)
5. Evidence strength and reason
6. PM interpretation (unvalidated)
7. Explanation

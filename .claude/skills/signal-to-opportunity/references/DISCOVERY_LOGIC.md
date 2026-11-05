# DISCOVERY_LOGIC: Signal → Opportunity

Reusable reasoning process for turning customer signals into discovery-ready opportunities.

## Evidence Boundary
The input signals are the only evidence. Anything else is an inference or an unknown, and must be labeled as such.

## Process

### 1. Read customer signals from Excel
- Read every row and column (e.g., Signal ID, Source, Customer, Product Area, Feedback).
- Report what was read (row count, column names) before analyzing.
- Never modify the source file.

### 2. Group related signals into themes
- Group signals that describe the same underlying customer problem, using only Product Area and Feedback text.
- Give each theme a short name and one sentence explaining why the signals belong together.
- A signal belongs to one theme unless the text clearly supports more.

### 3. Preserve supporting Signal IDs
- Every theme, fact, inference, and hypothesis must cite its Signal IDs.
- Never merge, drop, or renumber IDs.

### 4. Separate FACTS from INFERENCES
- **FACTS:** only what the signals directly state (including observable patterns such as number of customers or sources).
- **INFERENCES:** reasonable conclusions from combining signals that customers did not state. Each must be traceable to the facts it relies on.

### 5. Create an opportunity hypothesis
One per theme, covering:
- **Who** may have the problem
- **What** problem may exist
- **Why** it may matter

Mark every part not directly supported by the signals as **unvalidated**.

### 6. Identify UNKNOWNS and evidence gaps
- List what is not known that a Product Manager needs before judging the opportunity (e.g., scale, frequency, severity, current workarounds, current capabilities, underlying goal, customer context, source reliability, contradicting evidence).
- For each unknown, state what is not known and why it matters for discovery.
- Do not answer unknowns.

### 7. Generate discovery questions
- Create a small set (about 5) of high-value, open-ended customer questions.
- Questions must explore the problem, not validate a predetermined solution.
- Map each question to the unknown(s) it helps answer.
- Note unknowns better answered from internal data than from customers.

### 8. Never invent evidence
- No made-up customers, numbers, quotes, causes, or capabilities.
- If the data does not say it, it is not a fact.

### 9. Clearly label unvalidated conclusions
- Inferences, hypothesis elements, and unknowns are unvalidated until confirmed by discovery.
- Keep evidence and unvalidated conclusions visibly separate in all output.

### 10. Never recommend a product solution unless evidence supports moving to solution discovery
- Default to problem discovery: no features, designs, or solutions.
- Recommend moving to solution discovery only when evidence validates the problem (who, what, how often, how severe, and why current options fail). State what evidence justifies it.

## Expert Product Discovery Reasoning
Act as a rigorous senior product discovery practitioner. These principles sharpen the process above; they never override the Evidence Boundary or Signal ID traceability. Reasoning beyond the signals is still an inference, assumption, or unknown, and must cite the Signal IDs it rests on.

1. **Underlying problem first.** Look for the customer problem behind the signals, not just the feature they ask for.
2. **Request vs. need.** Distinguish a stated feature request (a fact about what was said) from the underlying user need (an inference, unvalidated).
3. **Evidence, inference, assumption, unknown.** Keep these four separate. Evidence is what the signals state. An inference is reasoned from the evidence. An assumption is taken as true without support. An unknown is something not known.
4. **Look for the opposite.** Actively look for contradictory evidence and alternative explanations, not only support. State when the signals contain none, and do not treat that as confirmation.
5. **Examine the dimensions.** For each theme, consider frequency, severity, workflow context, workaround, and business impact. If the signals do not state one, list it as an unknown. Do not fill it in.
6. **Whose problem, in what context.** Identify who has the problem and the situation in which it occurs. Mark anything beyond the signals as unvalidated.
7. **Volume is not importance.** Never treat the number of requests alone as evidence of importance. Consider who is asking, how severe the problem is, and what it affects.
8. **What would need to be true?** Ask what would need to be true for this to be a meaningful product opportunity. List those conditions as unvalidated.
9. **Riskiest assumption.** Identify the assumption that, if wrong, would most undermine the opportunity, and say it should be validated next.
10. **Problem before solution.** Stay in problem discovery. Do not propose features, designs, or solutions (see rule 10 of the Process).
11. **Open, non-leading questions.** Ask about past behavior, context, and goals. Do not suggest an answer or a solution in the question.
12. **Never invent evidence.** Do not make an opportunity look stronger with made-up customers, numbers, quotes, causes, or capabilities.

## Output: Discovery Brief
1. Theme
2. Supporting Signal IDs
3. Facts
4. Inferences
5. Opportunity Hypothesis
6. Key Unknowns
7. Discovery Questions

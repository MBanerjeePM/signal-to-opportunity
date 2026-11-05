# AB_COMPARISON: Generic Prompt vs. Signal → Opportunity

> **This is a single synthetic A/B run and not a benchmark.**

All data in this comparison is synthetic hackathon test data (`CustomerSignals.xlsx`, `SampleMRG.txt`). It is not real customer data and not Hyland product documentation.

> **Historical note:** this comparison was completed before Internal Discovery Review was built. Internal Discovery Review was not part of it, and the 8/10 result below does not cover that capability.

## 1. Test purpose
To show whether Signal → Opportunity provides value beyond a generic LLM prompt, using the same five synthetic signals.

The purpose is to demonstrate design differences, not to benchmark model intelligence.

## 2. Test setup
**Baseline A: generic Claude prompt**
- One Claude Code headless run with tools disabled.
- The only instruction was: *"Review these customer feedback signals. Identify the main product opportunities and recommend what the Product Manager should investigate."*
- Input: the same five synthetic signals (Signal ID, Source, Customer, Product Area, Feedback).
- The comparison intentionally withheld documentation and structured discovery instructions from the generic baseline. It was **not** given `DISCOVERY_LOGIC.md`, `CAPABILITY_CHECK_LOGIC.md`, `SampleMRG.txt` or either JSON schema, and it received no instructions beyond the generic prompt.

**B: Signal → Opportunity**
- The most recent successful, validated end-to-end regression output from the application. It was not re-run for this comparison.
- It is produced by two Claude Code headless stages with tools disabled, driven by the logic files and schemas above.
- **Signal → Opportunity's documentation check used synthetic `SampleMRG.txt`**, not the uploaded PDF.

## 3. The 10 comparison dimensions
1. Traceability to Signal IDs
2. Facts separated from inference
3. Counter-evidence considered
4. Existing product capability checked
5. Unsupported capability claims prevented
6. Unknowns explicitly identified
7. Riskiest assumption identified
8. Non-leading discovery questions produced
9. Structured output validation
10. Human PM decision preserved

## 4. What Baseline A actually produced
The baseline **produced a useful analysis.** It returned a readable report with two opportunities:
- **PDF page manipulation (S001, S002, S003):** evidence summary (3 signals, 3 customers, 3 sources), an interpretation, and a list of things to investigate.
- **Faster document findability (S004, S005):** evidence summary, an interpretation naming possible root causes (relevance or ranking, filtering UX, metadata quality, indexing gaps, training or configuration), and investigation items. These included internal search analytics and follow-up sessions watching real search tasks.
- **Recommended priorities:** validate PDF manipulation first, citing more signals, more source diversity and a more concrete need, and run Search discovery in parallel.
- **Caveats:** five signals is a small sample, there is no data on customer size, revenue, severity or frequency, and the two themes might be linked.
- **Observed weaknesses:**
  - The PDF heading and interpretation placed all three signals "in the Web Client", although only S003 names it.
  - It stated that users "can't edit PDF structure in the Web Client and probably leave the product" as the likely gap, which no signal states.
  - It ranked priorities by signal count and source diversity.
  - Signal IDs were cited per opportunity, not per claim.
  - It produced no structured, checkable output.

## 5. What Signal → Opportunity B actually produced
Two themes, each with a capability assessment and a Discovery Brief:
- **PDF page manipulation (S001, S002, S003):** Partially Supported, High evidence strength, "Possible discoverability/awareness issue". Two explicit quotes, from PDF Page 42 (rotate and save) and PDF Page 57 (extract a single page). It stated that no Web Client claim could be made because the documentation does not say which client it applies to.
- **Finding the right document (Search) (S004, S005):** Unclear, Low strength, "Needs further investigation". One related quote from PDF Page 63 (search inside an open document), treated as related evidence only and not as partial support.
- **Each brief had:**
  - itemised facts and inferences, with Signal IDs on each and inferences tagged unvalidated;
  - counter-evidence, including alternative explanations and weakening evidence, with a note that no contradictory signals is not confirmation;
  - one riskiest assumption;
  - five labelled unknowns, each tagged customers, internal data or both;
  - five open-ended discovery questions mapped to those unknowns.
- **Validation:** the output passed the schema check and the Signal ID and verbatim-quote checks. The app fails closed.
- It made no prioritization and no solution recommendation.

## 6. Result for each dimension

| # | Dimension | Baseline A | Signal → Opportunity B | Result |
|---|---|---|---|---|
| 1 | Signal ID traceability | IDs cited at opportunity level and some inline. Individual claims not cited. Nothing checked. | IDs on every fact, inference and counter-evidence item. The app confirmed every ID exists and every signal is in a theme. | **Demonstrated** (granularity and verification; Baseline A did cite IDs) |
| 2 | Facts separated from inference | "Evidence" and "Interpretation" paragraphs with hedging. Heading placed all three PDF signals "in the Web Client". | Itemised facts versus inferences, each inference tagged unvalidated. Noted that "only S003 names" the Web Client. | **Demonstrated** |
| 3 | Counter-evidence considered | Considered alternative causes for Search, a small sample, and a possible link between themes. | Per theme: alternative explanation, weakening evidence, and a note that absence of contradiction is not confirmation. | **Not Demonstrated** (comparable in substance in this run) |
| 4 | Existing capability checked | Listed "what can the Web Client do with PDFs today?" as a question. Checked nothing, since it had no documentation. | Checked against synthetic `SampleMRG.txt`: Partially Supported and Unclear, with exact quotes and pages. | **Demonstrated** (baseline intentionally given no documentation) |
| 5 | Unsupported capability claims prevented | Asserted a capability gap and a Web Client attribution that no signal states. | Made no Web Client claim, used Unclear and not "unsupported", and every quote was verified. | **Demonstrated** |
| 6 | Unknowns explicitly identified | Investigation lists and caveats covered frequency, workaround, severity, root cause and product versus configuration. | Five labelled unknowns per theme, each tagged with where it is best answered. | **Not Demonstrated** (similar substance; B more structured) |
| 7 | Riskiest assumption identified | None. Gave a priority order instead. | One per theme, marked unvalidated, with why it is risky. | **Demonstrated** |
| 8 | Non-leading discovery questions | Mostly PM-facing items, some feature-leaning (e.g., asking whether reorder, delete, merge and extract were also requested). | Five open, past-behavior questions per theme, each mapped to an unknown. | **Demonstrated** |
| 9 | Structured output validation | None (free text). | Schema, Signal ID, verbatim-quote and theme-match validation passed. The app fails closed. | **Demonstrated** (follows from the design) |
| 10 | Human PM decision preserved | Ranked priorities by signal count and source diversity, while noting it is "not a ranking by impact". | No prioritization and no solutions. Interpretations tagged unvalidated. The app has no way to record a PM decision. | **Demonstrated** (narrowly; neither captures a PM decision) |

## 7. Final result
- **8 dimensions with demonstrated differentiation:** 1, 2, 4, 5, 7, 8, 9, 10
- **2 dimensions with no demonstrated differentiation:** 3 (counter-evidence) and 6 (unknowns)

**Counter-evidence and unknown identification were not demonstrated differentiators in this run.**

## 8. Three most important observed differences
1. **Unsupported claims:** Baseline A stated a capability gap and attributed all three PDF signals to the Web Client without evidence. B refused to make a Web Client claim and flagged that only S003 names it.
2. **Capability check and validation:** B checked the need against documentation, and its output was machine-validated. Baseline A did neither. Its documentation check was only a to-do item, because it was given no documentation.
3. **Riskiest assumption and prioritization:** B named the riskiest assumption per theme and made no ranking. Baseline A ranked priorities by signal count and source diversity.

## 9. Conclusion
In this single run, a generic prompt produced a useful analysis. It had sensible themes, caveats about sample size and plausible root-cause questions. It matched B on counter-evidence and unknowns in substance, and it noticed a possible link between the two themes that B did not. The structured approach differed most where discipline matters. Baseline A asserted an unsupported capability gap, attributed all three PDF signals to the Web Client, ranked by volume, and offered no checkable evidence, riskiest assumption or customer-ready questions. B adds validated traceability and a documentation check.

## 10. Limitations
- **This is a single synthetic A/B run and not a benchmark.**
- It used five synthetic signals and one run per side. Model output varies between runs, and the baseline's result could differ on another run.
- **No claim is made about time saved, decision quality, statistical significance or real-user value.** None was measured. Baseline A's latency was not measured.
- Signal → Opportunity's documentation check used synthetic `SampleMRG.txt`.
- The comparison intentionally withheld documentation and structured discovery instructions from the generic baseline. The dimensions are also the ones Signal → Opportunity was designed to satisfy, so the comparison favors it by design.
- The comparison shows design differences. It does not benchmark model intelligence.

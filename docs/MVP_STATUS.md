# MVP_STATUS

## Project
**Signal → Opportunity: AI-powered Product Discovery Intelligence**

A Product Discovery Research Assistant designed to reduce the manual research and preparation burden around product discovery.

> Automate the research groundwork. Amplify the Product Manager.
> AI organizes and challenges the evidence. The Product Manager makes the decision.

## MVP status
**Hackathon-ready.** This implementation is frozen as the hackathon MVP. All data is synthetic hackathon test data.

## Tested lifecycle
Customer Signals
→ Theme Detection
→ Product Capability Validation
→ Discovery Brief
→ Internal Discovery Review
→ Hypothesis challenged or refined
→ Customer and Internal validation questions
→ Product Manager decision

## Working capabilities

**Customer signals and discovery**
- Excel customer-signal upload
- Structured signal parsing
- AI theme detection
- Evidence traceability through Signal IDs
- Facts vs inferences separation
- Counter-evidence and alternative explanations
- Opportunity hypothesis
- What Must Be True
- Riskiest assumption
- Key unknowns
- Open-ended discovery questions

**Product capability validation**
- Product capability validation
- Supported / Partially Supported / Unclear / Not Found classification
- MRG evidence quotation and page traceability

**Internal Discovery Review (tested MVP capability)**
- Manual Internal Feedback entry, with automatic IE IDs (IE001, IE002, …)
- Reviewer role without reviewer name
- Internal Expert Evidence as a separate evidence class, kept apart from Customer Evidence and Product Evidence
- Item-level impact classification (Strengthens, Weakens, Refines, Introduces new constraint, Introduces new unknown, No material impact)
- Overall hypothesis impact
- Original hypothesis preserved exactly
- Revised hypothesis only when materially changed
- New or changed unknowns
- Constraints
- Revised riskiest assumption where justified
- Customer vs Internal discovery questions, each labelled with its audience
- Separate traceability: customer Signal IDs and Internal Feedback IDs are cited in separate fields
- Headcount or opinion-count feedback does not increase importance
- Internal assertions explicitly labelled unverified
- No solution recommendation
- The Product Manager remains the final decision-maker

**Trust and execution**
- Validation of AI-returned Signal IDs, Internal Feedback IDs and documentation quotes
- Fail-closed behavior if validation fails
- Hyland Enterprise Claude Code headless execution with tools disabled

## Intended benefits (not yet validated)
The tool is designed to help prepare discovery by connecting scattered signals, researching and structuring evidence, checking documented product capabilities, separating facts, inference and unknowns, surfacing counter-evidence, identifying the riskiest assumption, preparing customer and internal validation questions, and incorporating internal expert review without confusing it with customer evidence.

These are **intended or potential benefits until validated with real PM users.** No hours saved, productivity improvement, decision-quality improvement or business outcome has been measured or is claimed.

## Current prototype limitations
- `CustomerSignals.xlsx` and `SampleMRG.txt` are synthetic hackathon test data. `SampleMRG.txt` is not Hyland product documentation.
- The real uploaded MRG PDF is not yet parsed. The PDF can be selected in the UI, but it is neither read nor sent anywhere. The UI shows the banner: "Prototype mode: Capability validation uses synthetic SampleMRG.txt, not the uploaded PDF."
- No real customer data and no real internal feedback has been used. Real internal feedback would require appropriate AI and data-use review before it is entered.
- Internal Feedback is kept in browser session memory only. Refreshing the page or running a new signal analysis clears it.
- A theme may contain multiple capabilities, so future versions should consider per-capability validation.
- AI output can vary between runs. Deterministic rules and schema validation reduce this but do not eliminate it.
- Current analysis latency is approximately 60–70 seconds for the synthetic five-signal demo. An Internal Review run took about 40–60 seconds in testing.
- Not production-ready. No measured time savings or business outcome is claimed.
- No integrations with Salesforce, Gainsight, Power BI, Jira, Confluence or Microsoft 365 have been built.

## Quality checks completed
These are functional checks on synthetic data. They are not statistical evidence.
- 4/4 controlled capability classification tests passed.
- Signal-analysis end-to-end regression passed, and it remained PASS after Internal Discovery Review was added.
- Both synthetic demo themes returned, with no Signal ID or MRG quote validation errors.
- Internal Review reasoning regression: PASS.
- Internal Review application/API test: PASS.
- Internal Review browser UI test: PASS.
- Internal Review checks that passed: evidence-class separation, original hypothesis preservation, the headcount rule, and Customer/Internal question-audience separation.
- A single synthetic A/B run showed differentiation on 8 of 10 evaluated dimensions against a generic prompt (see `AB_COMPARISON.md`). It is not a benchmark, and it predates Internal Discovery Review.

## Project files
- `CustomerSignals.xlsx`: synthetic customer signals
- `SampleMRG.txt`: synthetic product documentation
- `DISCOVERY_LOGIC.md` and `discovery-brief-schema.json`: discovery analysis rules and output schema
- `CAPABILITY_CHECK_LOGIC.md` and `capability-check-schema.json`: capability validation rules and output schema
- `INTERNAL_REVIEW_LOGIC.md` and `internal-review-schema.json`: Internal Discovery Review rules and output schema
- `SignalToOpportunity/`: the web application
- `SignalToOpportunity_WORKING_MVP/`: backup copy of the application source, taken before Internal Discovery Review was added
- `HACKATHON_SUBMISSION_DRAFT.md`, `HACKATHON_JUDGE_SUMMARY.md`, `AB_COMPARISON.md`, `DEMO_SCRIPT.md`, `SLIDE_PLAN.md`: submission materials

# Signal → Opportunity: Skill MVP Status

## 1. Purpose
Signal → Opportunity is a Product Discovery Research Assistant.

## 2. Skill location
`.claude/skills/signal-to-opportunity/`

## 3. Tested inputs
All inputs were synthetic.
- `CustomerSignals.xlsx` (synthetic)
- `SampleMRG.txt` (synthetic)
- Internal Feedback IE001–IE003 (synthetic)

## 4. Tested workflow
Customer Signals
→ Theme Detection
→ Product Capability Validation
→ Discovery Brief
→ Internal Discovery Review
→ Revised Hypothesis / Riskiest Assumption
→ Functional and relevant Non-Functional Unknowns
→ P0/P1/P2 Customer and Internal Discovery Questions
→ PM Decision Point

## 5. Successfully tested capabilities
- Skill discovery and invocation
- Excel signal analysis
- Theme detection
- Evidence traceability
- Facts vs Inferences
- Counter-evidence
- Product Capability Validation
- Exact synthetic documentation quotation and page references
- Supported / Partially Supported / Unclear classification
- Riskiest Assumption
- Functional Unknowns
- Relevant Non-Functional Unknowns
- P0/P1/P2 discovery-learning priority
- Customer vs Internal question audience
- Internal Discovery Review
- IE evidence separation
- Revised hypothesis
- Revised Riskiest Assumption
- Claude Artifact preview
- Private Artifact publication

## 6. Important rules
- P0/P1/P2 is discovery-learning priority, not product or roadmap priority.
- Internal Expert Evidence is not Customer Evidence.
- Product Documentation Evidence remains separate from Customer Evidence.
- AI does not recommend what to build.
- The Product Manager remains the decision-maker.

## 7. Limitations
- Tests use synthetic data.
- Real MRG PDF/link ingestion has not been tested in the Skill.
- Protected documentation links depend on approved connector access and permissions.
- Real customer/internal data has not been used.
- No measured time savings or business outcome.
- AI output may vary between runs.
- No production-readiness claim.

## 8. Delivery model
One discovery method, two experiences:

**A. Local .NET web application**
- Visual guided workspace
- Richer interactive experience

**B. Claude Skill + Artifact**
- Reusable, lightweight adoption path
- No custom hosted application required for the Skill experience

The tests recorded above cover experience B only. The .NET application was not part of these tests.

## 9. Core messages
> Automate the research groundwork. Amplify the Product Manager.

> AI organizes and challenges the evidence. The Product Manager makes the decision.

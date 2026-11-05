# Signal → Opportunity

**AI-powered Product Discovery Research Assistant**

- **Participant:** Mousumi Banerjee
- **Project:** Solo Hackathon Project
- **Category:** Discovery Excellence

## 1. Elevator Pitch

"Signal → Opportunity turns scattered customer signals into a structured learning plan for Product Managers. It helps identify what is known, what may be wrong, what is still unknown, and what should be learned next."

## 2. Problem

Product Managers manually:

- connect feedback from different sources
- research existing capabilities
- separate facts from assumptions
- identify evidence gaps
- prepare customer and internal discovery questions

## 3. Solution

```
Customer Signals
→ Theme Detection
→ Product Capability Validation
→ Discovery Brief
→ Internal Discovery Review
→ Prioritized Validation
→ Product Manager Decision
```

## 4. Core Discovery Concepts

**Riskiest Assumption**
"The unproven belief that could most seriously weaken or change the opportunity if it turns out to be wrong."

**Functional and Non-Functional Unknowns**
Functional unknowns concern what users need the product to do. Non-functional unknowns concern qualities such as performance, security, compliance, usability, reliability or scalability.

**P0/P1/P2 Discovery Questions**
Discovery questions are ranked by how much learning they provide and how urgently they should be answered.

> P0/P1/P2 is discovery-learning priority, not product or roadmap priority.

## 5. One Method, Two Experiences

### A. Local .NET Web Workspace (`SignalToOpportunity/`)

- Visual guided prototype
- Excel signal upload
- Opportunity themes
- Product capability validation
- Discovery Brief
- Internal Discovery Review

### B. Claude Skill + Artifact (`.claude/skills/signal-to-opportunity/`)

- Reusable Claude workflow
- Accepts customer signals and product documentation
- Supports optional internal feedback
- Produces structured Discovery output
- Includes the newer tested non-functional and P0/P1/P2 discovery model

**The two experiences are intentionally not completely feature-identical.** Both were tested as separate experiences, and no attempt was made to make them match.

### Schema difference (intentional)

Two versions of the discovery schema and reasoning are kept on purpose:

| | Location | Notes |
|---|---|---|
| Web application | Repo root: `DISCOVERY_LOGIC.md`, `discovery-brief-schema.json` (plus the capability-check and internal-review files) | Preserved exactly as required by the tested web runtime. |
| Claude Skill | `.claude/skills/signal-to-opportunity/references/` | Its own tested reference files, with the newer discovery improvements below. |

The **Skill version** adds:

- Functional vs Non-Functional Unknowns, with `nonFunctionalCategory`
- P0/P1/P2 discovery-learning priority, with `priorityReason`
- Customer/Internal audience
- Functional/Non-Functional question type
- `unknownsAddressed`
- `riskiestAssumptionAddressed`

The web application does not yet include these improvements.

## 6. Running the Web Application

Prerequisite: a .NET SDK compatible with the project (`SignalToOpportunity.csproj` targets `net10.0`).

```
cd SignalToOpportunity
dotnet run
```

Then open: http://localhost:5080

The Claude analysis path also requires an appropriately authenticated Claude Code environment.

## 7. Using the Claude Skill

Invoke the Skill conceptually as:

```
/signal-to-opportunity CustomerSignals.xlsx SampleMRG.txt
```

- Protected documentation links require appropriate permissions/connectors.
- Real or confidential information must follow applicable Hyland AI/data-use requirements.

## 8. Synthetic Demo Data

**CustomerSignals.xlsx**
- Synthetic hackathon data
- Customer A–E labels
- No real customer information

**SampleMRG.txt**
- Synthetic hackathon product-documentation fixture
- Not Hyland product documentation
- Invented page references for testing

## 9. Testing

Tested facts only:

- 4/4 controlled capability classification tests passed
- End-to-end signal-analysis regression passed
- Internal Discovery Review reasoning/API/browser tests passed
- Claude Skill tested with synthetic Excel, synthetic documentation, and synthetic internal feedback
- Private Claude Artifact successfully produced from the Skill workflow

These results are not statistical proof.

## 10. Limitations

- Synthetic data only
- Real MRG PDF ingestion is not implemented end-to-end in the web app
- Protected/real MRG ingestion is not tested in the Skill
- No real customer data used
- No measured time savings
- No proven business outcome
- AI outputs may vary
- Not production-ready

## 11. Future Direction

- Real approved MRG ingestion
- Per-capability validation
- Validation with real Product Managers
- Approved evidence enrichment from systems such as Salesforce, Gainsight, Power BI, Jira, Confluence and Microsoft 365
- Continuous opportunity evolution as new evidence arrives

**These integrations are not built today.**

## 12. Core Principles

**Automate the research groundwork. Amplify the Product Manager.**

**AI organizes and challenges the evidence. The Product Manager makes the decision.**

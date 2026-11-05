# Signal → Opportunity
*AI-powered Product Discovery Intelligence · Category: Discovery Excellence*

## 1. Problem
Product discovery starts with research-heavy groundwork. Product Managers connect scattered feedback, check whether the capability already exists, and prepare evidence, and two traps follow:
- A requested feature is mistaken for the underlying need.
- Request volume is mistaken for importance.

## 2. Solution
Signal → Opportunity is a Product Discovery Research Assistant:

Customer signals → theme → capability validation → Discovery Brief → **Internal Discovery Review** → customer and internal validation questions → **PM decision**

The AI prepares an evidence-backed brief. Internal experts then challenge or refine it, as a separate evidence class and never as customer evidence. It recommends no solutions.

> Automate the research groundwork. Amplify the Product Manager.

## 3. Why It Is Different
A generic LLM can generate a plausible analysis. Signal → Opportunity creates a **traceable, challengeable and validated** Discovery Brief.

In one synthetic A/B run, differentiation was demonstrated on 8 of 10 evaluated dimensions. **This is a single synthetic A/B run and not a benchmark.** Demonstrated differences:
- Prevents unsupported capability-gap claims
- Checks existing capabilities against documentation
- Validates Signal IDs and documentation evidence
- Identifies the riskiest assumption
- Does not equate volume with priority, including internal headcount
- Keeps the Product Manager in control

## 4. What We Built & Validated
A working prototype covering signals, capability validation, the Discovery Brief and Internal Discovery Review. Internal feedback gets automatic IDs, impact classification, a preserved original hypothesis, and Customer versus Internal questions.

- 4/4 controlled capability-classification tests passed.
- The signal-analysis regression passed.
- Internal Review reasoning, API and browser UI tests passed.

These are functional checks, not statistical evidence. All data is synthetic hackathon data.

## 5. Limitations & Future Vision
- The real uploaded MRG PDF is not analyzed, and no real customer or internal feedback was used.
- AI output varies between runs, and internal feedback lives in browser memory only.
- We claim no production readiness, measured time savings or business outcomes. Benefits are intended until validated with real PMs.

**Future vision:** real MRG ingestion, then approved enterprise evidence from Salesforce, Gainsight, Power BI, Jira, Confluence and Microsoft 365. None is built.

*AI organizes and challenges the evidence. The Product Manager makes the decision.*

using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

sealed record InternalFeedbackItem(string? Id, string? ReviewerRole, string? Text, string? Reference);

sealed record ReviewRequest(JsonObject? Discovery, JsonObject? Capability, List<InternalFeedbackItem>? Feedback);

// Internal Discovery Review stage: assesses manually entered internal feedback against one theme's
// existing Discovery Brief (INTERNAL_REVIEW_LOGIC.md + internal-review-schema.json), run through local
// Claude Code in headless mode with tools disabled. Fail-closed: any problem throws, so no partial results.
static class InternalReview
{
    private static readonly string[] Roles =
        ["Architect", "Engineering Lead", "Product Lead", "Manager", "Support / Services", "Other"];

    private const int MaxItems = 20;
    private const int MaxTextChars = 2000;
    private const int MaxReferenceChars = 500;

    private static readonly Regex IdPattern = new(@"^IE[0-9]+$");

    // Best-effort detection of headcount / opinion-count feedback ("three other PMs also think this matters").
    private static readonly Regex[] OpinionCount =
    [
        new(@"\b(\d+|one|two|three|four|five|six|seven|eight|nine|ten|several|many|multiple|most|all)\b.{0,50}" +
            @"\b(managers?|people|reviewers?|leads?|stakeholders?|colleagues?|teams?|pms?|engineers?|architects?|experts?)\b.{0,60}" +
            @"\b(think|thinks|believe|believes|agree|agrees|feel|feels|support|supports|want|wants|consider|considers)\b",
            RegexOptions.IgnoreCase),
        new(@"\b(everyone|everybody|we all|all of us)\b.{0,40}\b(agree|agrees|think|thinks|believe|believes|support|supports)\b",
            RegexOptions.IgnoreCase)
    ];

    private sealed record Item(string Id, string Role, string Text, string? Reference);

    public static async Task<JsonObject> RunAsync(ReviewRequest req, string contentRoot, CancellationToken ct)
    {
        if (!await Pipeline.Gate.WaitAsync(0, ct))
            throw new PipelineException("Another analysis is already running. Please try again in a moment.");
        try
        {
            return await RunCoreAsync(req, contentRoot, ct);
        }
        catch (Exception ex) when (ex is Win32Exception or JsonException or IOException or InvalidOperationException)
        {
            throw new PipelineException("The internal review could not be completed: " + ex.Message);
        }
        finally
        {
            Pipeline.Gate.Release();
        }
    }

    private static async Task<JsonObject> RunCoreAsync(ReviewRequest req, string contentRoot, CancellationToken ct)
    {
        if (req.Discovery is null || req.Capability is null)
            throw new PipelineException("The selected theme's Discovery Brief and capability check are required.");

        var root = Pipeline.FindProjectRoot(contentRoot);
        string Path_(string name) => Path.Combine(root, name);
        var logic = File.ReadAllText(Path_("INTERNAL_REVIEW_LOGIC.md"));
        var reviewSchemaPath = Path_("internal-review-schema.json");

        // --- The submitted theme data must itself be a valid brief and capability check ---
        var themeErrs = SchemaValidator.Validate(
            new JsonObject { ["themes"] = new JsonArray(req.Discovery.DeepClone()) },
            Pipeline.LoadSchema(Path_("discovery-brief-schema.json")));
        themeErrs.AddRange(SchemaValidator.Validate(
            new JsonObject { ["capabilityChecks"] = new JsonArray(req.Capability.DeepClone()) },
            Pipeline.LoadSchema(Path_("capability-check-schema.json"))));
        if (themeErrs.Count > 0)
            throw new PipelineException("The selected theme data is not valid: " + string.Join("; ", themeErrs.Take(3)));

        var items = ValidateFeedback(req.Feedback);
        var theme = req.Discovery["theme"]!.GetValue<string>();
        var customerIds = new HashSet<string>(
            req.Discovery["supportingSignalIds"]!.AsArray().Select(i => i!.GetValue<string>()), StringComparer.Ordinal);
        var originalHypothesis = req.Discovery["opportunityHypothesis"]!.AsObject();

        // --- Prompt ---
        var unknownsText = string.Join("\n", req.Discovery["unknowns"]!.AsArray().Select(u =>
            $"  {u!["id"]!.GetValue<string>()}: {u["notKnown"]!.GetValue<string>()}"));
        var capText =
            $"  Assessment: {req.Capability["assessment"]!.GetValue<string>()}\n" +
            $"  Evidence strength: {req.Capability["evidenceStrength"]!.GetValue<string>()}\n" +
            $"  PM interpretation (unvalidated): {req.Capability["pmInterpretation"]!.GetValue<string>()}\n" +
            $"  Explanation: {req.Capability["explanation"]!.GetValue<string>()}";
        var feedbackText = string.Join("\n\n", items.Select(i =>
            $"{i.Id}\n  Reviewer role: {i.Role}\n  Feedback: \"{i.Text}\"\n  Related theme: {theme}" +
            (i.Reference is null ? "" : $"\n  Reference/context: {i.Reference}")));
        var prompt =
            $"Follow these analysis instructions exactly:\n\n{logic}\n\n" +
            "EXISTING DISCOVERY BRIEF, for the selected theme:\n" +
            $"Theme: {theme}\n" +
            $"Customer Signal IDs behind this theme: {string.Join(", ", customerIds)}\n" +
            "Original opportunity hypothesis, as JSON. Copy it exactly and unchanged into originalHypothesis:\n" +
            $"{originalHypothesis.ToJsonString()}\n" +
            $"Original riskiest assumption: {req.Discovery["riskiestAssumption"]!["assumption"]!.GetValue<string>()}\n" +
            $"Original unknowns:\n{unknownsText}\n\n" +
            "PRODUCT EVIDENCE for this theme (capability validation; context only, not customer evidence):\n" +
            $"{capText}\n\n" +
            $"SUBMITTED INTERNAL FEEDBACK (Internal Expert Evidence):\n\n{feedbackText}\n\n" +
            "Produce the Internal Review for this theme as JSON matching the provided schema.";

        var review = await Pipeline.RunClaudeAsync(prompt, reviewSchemaPath, "Internal review", ct) as JsonObject
            ?? throw new PipelineException("Internal review returned an unreadable response.");

        // --- Validation (any failure: no results) ---
        var errs = SchemaValidator.Validate(review, Pipeline.LoadSchema(reviewSchemaPath));
        if (errs.Count == 0) CrossCheck(review, items, originalHypothesis, customerIds, errs);
        if (errs.Count > 0) throw Pipeline.Invalid("Internal review", errs);

        // Show the submitted values (verified identical above), not the model's copies.
        review["originalHypothesis"] = originalHypothesis.DeepClone();
        foreach (var a in review["feedbackAssessments"]!.AsArray().Select(a => a!.AsObject()))
        {
            var item = items.First(i => i.Id == a["internalFeedbackId"]!.GetValue<string>());
            a["reviewerRole"] = item.Role;
            a["feedbackText"] = item.Text;
            a.Remove("referenceContext");
            if (item.Reference is not null) a["referenceContext"] = item.Reference;
        }
        return new JsonObject { ["review"] = review.DeepClone() };   // clone: the node still belongs to the CLI envelope
    }

    private static List<Item> ValidateFeedback(List<InternalFeedbackItem>? feedback)
    {
        if (feedback is null || feedback.Count == 0)
            throw new PipelineException("Add at least one internal feedback item first.");
        if (feedback.Count > MaxItems)
            throw new PipelineException($"Prototype limit: up to {MaxItems} feedback items per review.");

        var items = new List<Item>();
        foreach (var f in feedback)
        {
            var id = f.Id?.Trim() ?? "";
            var role = f.ReviewerRole?.Trim() ?? "";
            var text = f.Text?.Trim() ?? "";
            var reference = string.IsNullOrWhiteSpace(f.Reference) ? null : f.Reference.Trim();
            if (!IdPattern.IsMatch(id)) throw new PipelineException("Each feedback item needs an ID like IE001.");
            if (!Roles.Contains(role)) throw new PipelineException($"{id}: choose a reviewer role from the list.");
            if (text.Length == 0) throw new PipelineException($"{id}: feedback text is required.");
            if (text.Length > MaxTextChars) throw new PipelineException($"{id}: feedback is too long (limit {MaxTextChars} characters).");
            if (reference is { Length: > MaxReferenceChars })
                throw new PipelineException($"{id}: reference is too long (limit {MaxReferenceChars} characters).");
            items.Add(new Item(id, role, text, reference));
        }
        if (items.Select(i => i.Id).Distinct(StringComparer.Ordinal).Count() != items.Count)
            throw new PipelineException("Internal Feedback IDs must be unique.");
        return items;
    }

    private static void CrossCheck(JsonObject review, List<Item> items, JsonObject originalHypothesis,
                                   HashSet<string> customerIds, List<string> errs)
    {
        var submitted = items.ToDictionary(i => i.Id, StringComparer.Ordinal);
        var assessments = review["feedbackAssessments"]!.AsArray().Select(a => a!.AsObject()).ToList();

        // Every submitted IE ID assessed exactly once; no unknown IE IDs.
        foreach (var id in submitted.Keys)
            if (assessments.Count(a => a["internalFeedbackId"]!.GetValue<string>() == id) != 1)
                errs.Add($"{id} was not assessed exactly once");
        foreach (var a in assessments)
        {
            var id = a["internalFeedbackId"]!.GetValue<string>();
            if (!submitted.ContainsKey(id)) errs.Add($"unknown Internal Feedback ID {id}");
        }

        // IE IDs anywhere in the claims must be submitted ones; customer IDs must belong to this theme.
        var ieRefs = new List<string>();
        var custRefs = new List<string>();
        Collect(review, "supportingInternalFeedbackIds", ieRefs);
        Collect(review, "supportingCustomerSignalIds", custRefs);
        foreach (var id in ieRefs.Distinct().Where(id => !submitted.ContainsKey(id)))
            errs.Add($"unknown Internal Feedback ID {id} cited");
        foreach (var id in custRefs.Distinct())
        {
            if (IdPattern.IsMatch(id)) errs.Add($"Internal Feedback ID {id} appears as customer evidence");
            else if (!customerIds.Contains(id)) errs.Add($"customer Signal ID '{id}' is not part of this theme");
        }

        // Original hypothesis must match the submitted Discovery Brief exactly.
        if (Canon(review["originalHypothesis"]) != Canon(originalHypothesis))
            errs.Add("the original hypothesis does not match the submitted Discovery Brief");

        // Reviewer role and feedback text echoed by the AI must match what was submitted.
        foreach (var a in assessments)
        {
            var id = a["internalFeedbackId"]!.GetValue<string>();
            if (!submitted.TryGetValue(id, out var item)) continue;
            if (a["reviewerRole"]!.GetValue<string>() != item.Role) errs.Add($"{id}: reviewer role does not match what was submitted");
            if (a["feedbackText"]!.GetValue<string>() != item.Text) errs.Add($"{id}: feedback text does not match what was submitted");
            var echoedRef = (string?)a["referenceContext"];
            if (!string.IsNullOrEmpty(echoedRef) && echoedRef != item.Reference)
                errs.Add($"{id}: reference does not match what was submitted");
        }

        // Opinion-count feedback must not move the hypothesis or the opportunity's importance.
        foreach (var a in assessments)
        {
            var id = a["internalFeedbackId"]!.GetValue<string>();
            var impact = a["impact"]!.GetValue<string>();
            if (!submitted.TryGetValue(id, out var item)) continue;
            if (OpinionCount.Any(r => r.IsMatch(item.Text)) && impact != "No material impact")
                errs.Add($"{id}: feedback that only reports how many people agree must be 'No material impact'");
            if (impact == "No material impact" && ieRefs.Contains(id))
                errs.Add($"{id}: feedback with no material impact must not be cited as support for a claim");
        }
        if (review["overallHypothesisImpact"]!.GetValue<string>() == "Strengthened"
            && !assessments.Any(a => a["impact"]!.GetValue<string>() == "Strengthens hypothesis"))
            errs.Add("the hypothesis is reported as strengthened, but no feedback item was assessed as strengthening it");
    }

    private static void Collect(JsonNode? node, string key, List<string> into)
    {
        switch (node)
        {
            case JsonObject o:
                foreach (var kv in o)
                {
                    if (kv.Key == key && kv.Value is JsonArray arr)
                    {
                        foreach (var x in arr)
                            if (x is JsonValue v && v.TryGetValue<string>(out var s)) into.Add(s);
                    }
                    else Collect(kv.Value, key, into);
                }
                break;
            case JsonArray a:
                foreach (var x in a) Collect(x, key, into);
                break;
        }
    }

    // Order-independent exact form of an opportunity hypothesis (who / what / why).
    private static string Canon(JsonNode? hypothesis)
    {
        var sb = new StringBuilder();
        if (hypothesis is JsonObject o)
            foreach (var part in new[] { "who", "what", "why" })
                if (o[part] is JsonObject p)
                {
                    var ids = (p["supportingSignalIds"] as JsonArray)?.Select(x => (string?)x) ?? [];
                    sb.Append(part).Append('|').Append((string?)p["text"]).Append('|')
                      .Append(string.Join(",", ids)).Append('|').Append((bool?)p["unvalidated"]).Append('\n');
                }
        return sb.ToString();
    }
}

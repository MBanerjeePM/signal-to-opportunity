using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

class PipelineException(string message) : Exception(message);

sealed record MrgPage(string Label, string Section, string Text);

// Two-stage AI pipeline, run through local Claude Code in headless mode with tools disabled:
//   1. Discovery analysis   (DISCOVERY_LOGIC.md + discovery-brief-schema.json)
//   2. Capability validation (CAPABILITY_CHECK_LOGIC.md + SampleMRG.txt + capability-check-schema.json)
// Both outputs are validated before anything is returned; any failure throws, so no partial results.
static class Pipeline
{
    public const string PrototypeNotice =
        "Prototype mode: Capability validation uses synthetic SampleMRG.txt, not the uploaded PDF.";

    private const int MaxSignals = 200;
    private static readonly TimeSpan CallTimeout = TimeSpan.FromMinutes(5);
    internal static readonly SemaphoreSlim Gate = new(1, 1);

    public static async Task<JsonObject> RunAsync(
        List<string> columns, List<List<string>> rows, string contentRoot, CancellationToken ct)
    {
        if (!await Gate.WaitAsync(0, ct))
            throw new PipelineException("Another analysis is already running. Please try again in a moment.");
        try
        {
            return await RunCoreAsync(columns, rows, contentRoot, ct);
        }
        catch (Exception ex) when (ex is Win32Exception or JsonException or IOException or InvalidOperationException)
        {
            throw new PipelineException("The AI analysis could not be completed: " + ex.Message);
        }
        finally
        {
            Gate.Release();
        }
    }

    private static async Task<JsonObject> RunCoreAsync(
        List<string> columns, List<List<string>> rows, string contentRoot, CancellationToken ct)
    {
        // --- Signals from the uploaded Excel file ---
        var idCol = columns.FindIndex(c => c.Trim().Equals("Signal ID", StringComparison.OrdinalIgnoreCase));
        if (idCol < 0)
            throw new PipelineException("The Excel file needs a 'Signal ID' column.");
        if (rows.Count == 0)
            throw new PipelineException("The Excel file has no signal rows.");
        if (rows.Count > MaxSignals)
            throw new PipelineException($"Prototype limit: up to {MaxSignals} signals per analysis.");

        var signalIds = rows.Select(r => r[idCol].Trim()).ToList();
        if (signalIds.Any(string.IsNullOrEmpty))
            throw new PipelineException("Every signal needs a Signal ID.");
        if (signalIds.Distinct(StringComparer.Ordinal).Count() != signalIds.Count)
            throw new PipelineException("Signal IDs must be unique.");
        var validIds = new HashSet<string>(signalIds, StringComparer.Ordinal);

        string SignalLine(List<string> row) => string.Join(" | ",
            columns.Select((name, i) => (name, value: row[i].Trim()))
                   .Where(c => c.value.Length > 0)
                   .OrderBy(c => c.name.Trim().Equals("Signal ID", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                   .Select(c => c.name.Trim().Equals("Signal ID", StringComparison.OrdinalIgnoreCase)
                       ? c.value : $"{c.name}: {c.value}"));
        var signalById = rows.ToDictionary(r => r[idCol].Trim(), SignalLine, StringComparer.Ordinal);

        // --- Reference files (read, never modified) ---
        var root = FindProjectRoot(contentRoot);
        string Path_(string name) => Path.Combine(root, name);
        var discoveryLogic = File.ReadAllText(Path_("DISCOVERY_LOGIC.md"));
        var capabilityLogic = File.ReadAllText(Path_("CAPABILITY_CHECK_LOGIC.md"));
        var mrgRaw = File.ReadAllText(Path_("SampleMRG.txt"));
        var briefSchemaPath = Path_("discovery-brief-schema.json");
        var capSchemaPath = Path_("capability-check-schema.json");
        var mrgPages = ParseMrg(mrgRaw);
        if (mrgPages.Count == 0)
            throw new PipelineException("SampleMRG.txt has no page-marked sections.");

        // --- Stage 1: discovery ---
        var signalText = string.Join("\n", rows.Select(SignalLine));
        var prompt1 =
            $"Follow these analysis instructions exactly:\n\n{discoveryLogic}\n\n" +
            $"CUSTOMER SIGNALS (the only evidence):\n{signalText}\n\n" +
            "Produce the Discovery Brief as JSON matching the provided schema.";
        var brief = await RunClaudeAsync(prompt1, briefSchemaPath, "Discovery analysis", ct);

        var errs = SchemaValidator.Validate(brief, LoadSchema(briefSchemaPath));
        CheckSignalIds(brief, validIds, errs);
        if (errs.Count == 0)
        {
            var covered = new HashSet<string>(StringComparer.Ordinal);
            foreach (var t in brief["themes"]!.AsArray())
                foreach (var id in t!["supportingSignalIds"]!.AsArray())
                    covered.Add(id!.GetValue<string>());
            foreach (var id in validIds.Where(id => !covered.Contains(id)))
                errs.Add($"signal {id} was not placed in any theme");
        }
        if (errs.Count > 0) throw Invalid("Discovery analysis", errs);
        var themes = brief["themes"]!.AsArray().Select(t => t!.AsObject()).ToList();

        // --- Stage 2: capability validation ---
        var themeText = string.Join("\n\n", themes.Select(t =>
        {
            var ids = t["supportingSignalIds"]!.AsArray().Select(i => i!.GetValue<string>()).ToList();
            var lines = string.Join("\n", ids.Select(i => $"  {signalById[i]}"));
            return $"THEME: {t["theme"]!.GetValue<string>()}\nCustomer Signal IDs: {string.Join(", ", ids)}\nCustomer signals:\n{lines}";
        }));
        var prompt2 =
            $"Follow these analysis instructions exactly:\n\n{capabilityLogic}\n\n" +
            $"CUSTOMER THEMES (customer-signal evidence only):\n{themeText}\n\n" +
            "PRODUCT DOCUMENTATION (the only MRG evidence; synthetic test data). " +
            "Page and section come from the '=== ... ===' marker lines:\n" +
            $"{mrgRaw}\n\n" +
            "Produce one capability check per theme as JSON matching the provided schema.";
        var capability = await RunClaudeAsync(prompt2, capSchemaPath, "Capability validation", ct);

        errs = SchemaValidator.Validate(capability, LoadSchema(capSchemaPath));
        CheckSignalIds(capability, validIds, errs);
        if (errs.Count == 0) CheckMrgQuotes(capability, mrgPages, errs);

        var checks = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase);
        if (errs.Count == 0)
        {
            foreach (var c in capability["capabilityChecks"]!.AsArray())
            {
                var name = c!["theme"]!.GetValue<string>().Trim();
                if (!checks.TryAdd(name, c.AsObject()))
                    errs.Add($"more than one capability check for theme '{name}'");
            }
            foreach (var t in themes)
                if (!checks.ContainsKey(t["theme"]!.GetValue<string>().Trim()))
                    errs.Add($"no capability check for theme '{t["theme"]!.GetValue<string>()}'");
            if (checks.Count != themes.Count)
                errs.Add("capability checks do not match the discovery themes");
        }
        if (errs.Count > 0) throw Invalid("Capability validation", errs);

        // --- Combine (deep clones: a node can only have one parent) ---
        var results = new JsonArray();
        foreach (var t in themes)
        {
            var name = t["theme"]!.GetValue<string>();
            results.Add(new JsonObject
            {
                ["theme"] = name,
                ["discovery"] = t.DeepClone(),
                ["capability"] = checks[name.Trim()].DeepClone()
            });
        }
        return new JsonObject { ["prototypeNotice"] = PrototypeNotice, ["results"] = results };
    }

    // ---------- Claude Code (headless, tools disabled, existing Enterprise login) ----------

    internal static async Task<JsonNode> RunClaudeAsync(string prompt, string schemaPath, string stage, CancellationToken ct)
    {
        var psi = new ProcessStartInfo(ClaudePath())
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetTempPath(),   // keep project files out of Claude's working context
            StandardInputEncoding = new UTF8Encoding(false),
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        foreach (var arg in new[] { "-p", "--tools", "", "--output-format", "json",
                                    "--json-schema", LoadSchema(schemaPath).ToJsonString() })
            psi.ArgumentList.Add(arg);
        // Always use the signed-in Enterprise account, never an API key.
        psi.Environment.Remove("ANTHROPIC_API_KEY");
        psi.Environment.Remove("ANTHROPIC_AUTH_TOKEN");

        using var proc = Process.Start(psi)
            ?? throw new PipelineException($"{stage} could not start Claude Code.");
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(CallTimeout);

        string output;
        try
        {
            var stdout = proc.StandardOutput.ReadToEndAsync(cts.Token);
            var stderr = proc.StandardError.ReadToEndAsync(cts.Token);
            await proc.StandardInput.WriteAsync(prompt.AsMemory(), cts.Token);
            proc.StandardInput.Close();
            await proc.WaitForExitAsync(cts.Token);
            output = await stdout;
            await stderr;   // drained so the process never blocks; warnings are ignored
        }
        catch (OperationCanceledException)
        {
            try { proc.Kill(true); } catch { /* already gone */ }
            if (ct.IsCancellationRequested) throw;
            throw new PipelineException($"{stage} timed out after {CallTimeout.TotalMinutes:0} minutes.");
        }

        if (proc.ExitCode != 0)
            throw new PipelineException(
                $"{stage} failed: Claude Code exited with code {proc.ExitCode}. Check that you are signed in.");

        var envelope = JsonNode.Parse(output) as JsonObject
            ?? throw new PipelineException($"{stage} returned an unreadable response.");
        if (envelope["is_error"] is JsonValue e && e.TryGetValue<bool>(out var isError) && isError)
            throw new PipelineException($"{stage} failed: Claude Code reported an error.");
        return envelope["structured_output"]
            ?? throw new PipelineException($"{stage} returned no structured output.");
    }

    private static string ClaudePath()
    {
        var local = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "bin", "claude.exe");
        return File.Exists(local) ? local : "claude";
    }

    // ---------- Reference files ----------

    internal static string FindProjectRoot(string start)
    {
        var dir = new DirectoryInfo(start);
        for (var i = 0; i < 6 && dir is not null; i++, dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "DISCOVERY_LOGIC.md"))) return dir.FullName;
        throw new PipelineException("Could not find DISCOVERY_LOGIC.md and the other project reference files.");
    }

    internal static JsonObject LoadSchema(string path) => JsonNode.Parse(File.ReadAllText(path))!.AsObject();

    // "=== PDF Page 42 | Section: PDF Viewer ===" starts a page; following lines are its text.
    private static List<MrgPage> ParseMrg(string raw)
    {
        var header = new Regex(@"^===\s*(?<label>.+?)\s*\|\s*Section:\s*(?<section>.+?)\s*===\s*$");
        var pages = new List<MrgPage>();
        string? label = null, section = null;
        var text = new StringBuilder();

        void Flush()
        {
            if (label is not null) pages.Add(new MrgPage(label, section!, text.ToString().Trim()));
            text.Clear();
        }

        foreach (var rawLine in raw.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');
            var m = header.Match(line);
            if (m.Success)
            {
                Flush();
                label = m.Groups["label"].Value;
                section = m.Groups["section"].Value;
            }
            else if (label is not null) text.AppendLine(line);
        }
        Flush();
        return pages;
    }

    // ---------- Validation ----------

    private static void CheckSignalIds(JsonNode? output, HashSet<string> validIds, List<string> errs)
    {
        var ids = new List<string>();
        CollectIds(output, ids);
        foreach (var id in ids.Distinct().Where(id => !validIds.Contains(id)))
            errs.Add($"unknown Signal ID '{id}' (not in the uploaded Excel)");
    }

    private static void CollectIds(JsonNode? node, List<string> ids)
    {
        switch (node)
        {
            case JsonObject o:
                foreach (var kv in o)
                {
                    if ((kv.Key == "supportingSignalIds" || kv.Key == "customerSignalIds") && kv.Value is JsonArray arr)
                    {
                        foreach (var x in arr)
                            if (x is JsonValue v && v.TryGetValue<string>(out var s)) ids.Add(s);
                    }
                    else CollectIds(kv.Value, ids);
                }
                break;
            case JsonArray a:
                foreach (var x in a) CollectIds(x, ids);
                break;
        }
    }

    // Every quote must appear verbatim (whitespace-normalised) on the page it cites.
    private static void CheckMrgQuotes(JsonNode capability, List<MrgPage> pages, List<string> errs)
    {
        static string Norm(string s) => Regex.Replace(s, @"\s+", " ").Trim();

        foreach (var c in capability["capabilityChecks"]!.AsArray())
        {
            var theme = c!["theme"]!.GetValue<string>();
            foreach (var ev in c["mrgEvidence"]!.AsArray())
            {
                var quote = Norm(ev!["exactEvidenceText"]!.GetValue<string>());
                var cited = ev["pageOrSection"]!.GetValue<string>();
                var ok = pages.Where(p => Cites(cited, p)).Any(p => Norm(p.Text).Contains(quote, StringComparison.Ordinal));
                if (!ok)
                    errs.Add($"MRG quote for '{theme}' does not appear verbatim on the cited page ('{cited}')");
            }
        }
    }

    // A citation matches a page when it names that page's number (or, if the label has no number, the label).
    private static bool Cites(string cited, MrgPage page)
    {
        var num = Regex.Match(page.Label, @"\d+");
        return num.Success
            ? Regex.IsMatch(cited, $@"(?<!\d){num.Value}(?!\d)")
            : cited.Contains(page.Label, StringComparison.OrdinalIgnoreCase);
    }

    internal static PipelineException Invalid(string stage, List<string> errs) =>
        new($"{stage} returned output that failed validation, so no results are shown: " +
            string.Join("; ", errs.Take(5)) + (errs.Count > 5 ? $"; and {errs.Count - 5} more" : ""));
}

// Checks data against the subset of JSON Schema (draft-07) used by our schema files:
// $ref, type, enum, const, minLength, required, properties, additionalProperties:false,
// items, minItems, maxItems, contains, and allOf with if/then.
static class SchemaValidator
{
    public static List<string> Validate(JsonNode? data, JsonObject rootSchema)
    {
        var errs = new List<string>();
        Check(data, rootSchema, rootSchema, "$", errs);
        return errs;
    }

    private static void Check(JsonNode? n, JsonObject s, JsonObject root, string path, List<string> errs)
    {
        if ((string?)s["$ref"] is { } reference)
        {
            Check(n, Resolve(root, reference), root, path, errs);
            return;
        }
        if ((string?)s["type"] is { } type && !TypeOk(n, type))
        {
            errs.Add($"{path}: expected {type}");
            return;
        }
        if (s["enum"] is JsonArray allowed && !allowed.Any(x => x?.ToJsonString() == n?.ToJsonString()))
            errs.Add($"{path}: value is not one of the allowed values");
        if (s.ContainsKey("const") && s["const"]?.ToJsonString() != n?.ToJsonString())
            errs.Add($"{path}: value does not match the required constant");

        if (n is JsonValue sv && sv.TryGetValue<string>(out var str)
            && (int?)s["minLength"] is { } minLen && str.Length < minLen)
            errs.Add($"{path}: text is empty");
        if (n is JsonValue pv && pv.TryGetValue<string>(out var patStr)
            && (string?)s["pattern"] is { } pattern && !Regex.IsMatch(patStr, pattern))
            errs.Add($"{path}: '{patStr}' does not match the required format");

        if (n is JsonObject obj)
        {
            if (s["required"] is JsonArray required)
                foreach (var key in required)
                    if (!obj.ContainsKey(key!.GetValue<string>()))
                        errs.Add($"{path}: missing '{key.GetValue<string>()}'");

            var props = s["properties"] as JsonObject;
            foreach (var kv in obj)
            {
                if (props is not null && props.TryGetPropertyValue(kv.Key, out var sub) && sub is JsonObject subSchema)
                    Check(kv.Value, subSchema, root, $"{path}.{kv.Key}", errs);
                else if (s["additionalProperties"] is JsonValue ap && ap.TryGetValue<bool>(out var extraOk) && !extraOk)
                    errs.Add($"{path}: unexpected field '{kv.Key}'");
            }
        }

        if (n is JsonArray arr)
        {
            if ((int?)s["minItems"] is { } min && arr.Count < min) errs.Add($"{path}: needs at least {min} item(s)");
            if ((int?)s["maxItems"] is { } max && arr.Count > max) errs.Add($"{path}: allows at most {max} item(s)");
            if (s["items"] is JsonObject itemSchema)
                for (var i = 0; i < arr.Count; i++)
                    Check(arr[i], itemSchema, root, $"{path}[{i}]", errs);
            if (s["contains"] is JsonObject containsSchema && !arr.Any(x =>
                {
                    var trial = new List<string>();
                    Check(x, containsSchema, root, path, trial);
                    return trial.Count == 0;
                }))
                errs.Add($"{path}: no item satisfies the required condition");
        }

        if (s["allOf"] is JsonArray allOf)
            foreach (var part in allOf)
                if (part is JsonObject rule && rule["if"] is JsonObject cond)
                {
                    var trial = new List<string>();
                    Check(n, cond, root, path, trial);
                    if (trial.Count == 0 && rule["then"] is JsonObject then)
                        Check(n, then, root, path, errs);
                }
    }

    private static bool TypeOk(JsonNode? n, string type) => type switch
    {
        "object" => n is JsonObject,
        "array" => n is JsonArray,
        "string" => n is JsonValue v && v.TryGetValue<string>(out _),
        "boolean" => n is JsonValue v && v.TryGetValue<bool>(out _),
        _ => true
    };

    // Resolves "#/definitions/name" within the root schema.
    private static JsonObject Resolve(JsonObject root, string reference)
    {
        JsonNode? node = root;
        foreach (var part in reference.TrimStart('#', '/').Split('/'))
            node = node?[part];
        return node as JsonObject ?? throw new InvalidOperationException($"Unresolved schema reference {reference}");
    }
}

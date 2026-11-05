using System.IO.Compression;
using System.Text;
using System.Text.Json.Nodes;
using System.Xml.Linq;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5080");

var app = builder.Build();

// Serve wwwroot/index.html as the single page.
app.UseDefaultFiles();
app.UseStaticFiles();

// Receives an .xlsx upload, reads it in memory only (nothing is saved to disk),
// runs the AI pipeline (see Pipeline.cs), and returns the column names, row count, rows
// and the combined, validated analysis. The product MRG PDF is not read or sent.
app.MapPost("/api/analyze", async (IFormFile? file, IWebHostEnvironment env, CancellationToken ct) =>
{
    const long MaxUploadBytes = 10 * 1024 * 1024;

    if (file is null || file.Length == 0)
        return Results.BadRequest(new { error = "No file was received." });
    if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest(new { error = "Please upload an .xlsx file." });
    if (file.Length > MaxUploadBytes)
        return Results.BadRequest(new { error = "File is too large (limit 10 MB)." });

    try
    {
        using var memory = new MemoryStream();
        await file.CopyToAsync(memory);
        memory.Position = 0;

        var table = ExcelReader.ReadFirstSheet(memory);
        if (table.Count == 0)
            return Results.BadRequest(new { error = "The first sheet is empty." });

        var columns = table[0];
        var rows = table.Skip(1).ToList();

        JsonObject analysis;
        try
        {
            analysis = await Pipeline.RunAsync(columns, rows, env.ContentRootPath, ct);
        }
        catch (PipelineException ex)
        {
            return Results.Json(new { error = ex.Message }, statusCode: 502);
        }

        return Results.Ok(new
        {
            fileName = file.FileName,
            columns,
            rowCount = rows.Count,
            rows,
            analysis
        });
    }
    catch (Exception ex) when (ex is InvalidDataException or InvalidOperationException
                                   or System.Xml.XmlException or KeyNotFoundException)
    {
        return Results.BadRequest(new { error = "Could not read this file as an Excel workbook." });
    }
}).DisableAntiforgery(); // local-only app; no cookies or login are involved

// Internal Discovery Review for one theme: assesses manually entered internal feedback against that
// theme's existing Discovery Brief (see Review.cs). Nothing is stored; results exist only in the response.
app.MapPost("/api/review", async (ReviewRequest req, IWebHostEnvironment env, CancellationToken ct) =>
{
    try
    {
        return Results.Ok(await InternalReview.RunAsync(req, env.ContentRootPath, ct));
    }
    catch (PipelineException ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: 502);
    }
});

app.Run();

// Minimal .xlsx reader using only built-in .NET (an .xlsx is a zip of XML files).
static class ExcelReader
{
    private const long MaxEntryBytes = 50 * 1024 * 1024;

    private static readonly XNamespace Main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace RelNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace PkgRel = "http://schemas.openxmlformats.org/package/2006/relationships";

    public static List<List<string>> ReadFirstSheet(Stream xlsx)
    {
        using var zip = new ZipArchive(xlsx, ZipArchiveMode.Read);

        var sharedStrings = ReadSharedStrings(zip);
        var sheetPath = FindFirstSheetPath(zip);
        var sheet = LoadXml(zip, sheetPath) ?? throw new InvalidDataException("Sheet not found.");

        var result = new List<List<string>>();
        var width = 0;

        foreach (var row in sheet.Descendants(Main + "row"))
        {
            var cells = new Dictionary<int, string>();
            foreach (var c in row.Elements(Main + "c"))
            {
                var col = ColumnIndex((string?)c.Attribute("r") ?? "");
                if (col < 0) col = cells.Count == 0 ? 0 : cells.Keys.Max() + 1;
                cells[col] = CellText(c, sharedStrings);
            }
            if (cells.Count == 0 || cells.Values.All(string.IsNullOrWhiteSpace)) continue;

            var list = new List<string>();
            for (var i = 0; i <= cells.Keys.Max(); i++)
                list.Add(cells.TryGetValue(i, out var v) ? v : "");
            width = Math.Max(width, list.Count);
            result.Add(list);
        }

        // Make every row the same width as the widest row.
        foreach (var r in result)
            while (r.Count < width) r.Add("");

        return result;
    }

    private static XDocument? LoadXml(ZipArchive zip, string path)
    {
        var entry = zip.GetEntry(path);
        if (entry is null) return null;
        if (entry.Length > MaxEntryBytes) throw new InvalidDataException("Entry too large.");
        using var s = entry.Open();
        return XDocument.Load(s);
    }

    private static string FindFirstSheetPath(ZipArchive zip)
    {
        var wb = LoadXml(zip, "xl/workbook.xml") ?? throw new InvalidDataException("Not a workbook.");
        var firstSheet = wb.Descendants(Main + "sheet").FirstOrDefault()
            ?? throw new InvalidDataException("No sheets.");
        var relId = (string?)firstSheet.Attribute(RelNs + "id");

        var rels = LoadXml(zip, "xl/_rels/workbook.xml.rels");
        var target = rels?.Descendants(PkgRel + "Relationship")
            .FirstOrDefault(r => (string?)r.Attribute("Id") == relId)?
            .Attribute("Target")?.Value;

        if (string.IsNullOrEmpty(target)) return "xl/worksheets/sheet1.xml";
        return target.StartsWith('/') ? target.TrimStart('/') : "xl/" + target;
    }

    private static List<string> ReadSharedStrings(ZipArchive zip)
    {
        var doc = LoadXml(zip, "xl/sharedStrings.xml");
        if (doc is null) return new List<string>();
        // Each <si> holds plain text in <t>, or rich text split across several <r><t> runs.
        return doc.Descendants(Main + "si")
            .Select(si => string.Concat(si.Descendants(Main + "t").Select(t => t.Value)))
            .ToList();
    }

    private static string CellText(XElement c, List<string> sharedStrings)
    {
        var type = (string?)c.Attribute("t");
        if (type == "inlineStr")
            return string.Concat(c.Descendants(Main + "t").Select(t => t.Value));

        var v = c.Element(Main + "v")?.Value ?? "";
        if (type == "s" && int.TryParse(v, out var idx) && idx >= 0 && idx < sharedStrings.Count)
            return sharedStrings[idx];
        if (type == "b") return v == "1" ? "TRUE" : "FALSE";
        return v;
    }

    // "C12" -> 2 (zero-based column index); returns -1 if no letters found.
    private static int ColumnIndex(string cellRef)
    {
        var n = 0;
        var any = false;
        foreach (var ch in cellRef)
        {
            if (!char.IsLetter(ch)) break;
            n = n * 26 + (char.ToUpperInvariant(ch) - 'A' + 1);
            any = true;
        }
        return any ? n - 1 : -1;
    }
}

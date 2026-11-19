using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using MeuGarcom.Application.Common.Interfaces;

namespace MeuGarcom.Infrastructure.Ai;

/// <summary>
/// Turns spreadsheet attachments (XLSX, CSV) into plain text, since the providers only read PDFs
/// and images natively. PDFs and images are sent as they are.
/// </summary>
internal static class AttachmentText
{
    private const int MaxChars = 200_000;
    private const string XlsxMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static bool IsImage(AiAttachment attachment) => attachment.MediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    public static bool IsPdf(AiAttachment attachment) => attachment.MediaType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase);

    /// <summary>Text version of a spreadsheet, or null when the attachment is a PDF or image.</summary>
    public static string? ToText(AiAttachment attachment)
    {
        if (attachment.MediaType.Equals(XlsxMediaType, StringComparison.OrdinalIgnoreCase))
            return Clip(ReadXlsx(attachment.Content));
        if (attachment.MediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
            return Clip(Encoding.UTF8.GetString(attachment.Content));
        return null;
    }

    private static string Clip(string text) => text.Length > MaxChars ? text[..MaxChars] : text;

    private static string ReadXlsx(byte[] content)
    {
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        using var zip = new ZipArchive(new MemoryStream(content), ZipArchiveMode.Read);

        var shared = new List<string>();
        var sharedEntry = zip.GetEntry("xl/sharedStrings.xml");
        if (sharedEntry is not null)
        {
            using var stream = sharedEntry.Open();
            shared = XDocument.Load(stream).Descendants(ns + "si")
                .Select(si => string.Concat(si.Descendants(ns + "t").Select(t => t.Value)))
                .ToList();
        }

        var builder = new StringBuilder();
        var sheets = zip.Entries
            .Where(e => e.FullName.StartsWith("xl/worksheets/sheet", StringComparison.OrdinalIgnoreCase) && e.FullName.EndsWith(".xml"))
            .OrderBy(e => e.FullName);

        foreach (var sheet in sheets)
        {
            using var stream = sheet.Open();
            foreach (var row in XDocument.Load(stream).Descendants(ns + "row"))
            {
                var cells = row.Elements(ns + "c").Select(c =>
                {
                    var type = (string?)c.Attribute("t");
                    var value = c.Element(ns + "v")?.Value;
                    return type switch
                    {
                        "s" when int.TryParse(value, out var index) && index < shared.Count => shared[index],
                        "inlineStr" => string.Concat(c.Descendants(ns + "t").Select(t => t.Value)),
                        _ => value ?? "",
                    };
                });
                var line = string.Join('\t', cells).Trim();
                if (line.Length > 0)
                    builder.AppendLine(line);
                if (builder.Length > MaxChars)
                    return builder.ToString();
            }
        }

        return builder.ToString();
    }
}

using System.IO.Compression;
using System.Text;
using System.Xml;

namespace QLDA.Gen.Generators;

/// <summary>
/// ClosedXML 0.102.3 emits non-standard OOXML packages:
///   1. <c>[Content_Types].xml</c> sets the <c>xml</c> default content type to
///      <c>application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml</c>
///      (workbook MIME) instead of <c>application/xml</c>. This breaks strict
///      parsers such as SheetJS / exceljs used by VSCode Excel viewer extensions —
///      they bail out when an arbitrary XML part is fetched with the workbook MIME.
///   2. Core properties are written to
///      <c>package/services/metadata/core-properties/{guid}.psmdcp</c> instead of
///      the standard <c>docProps/core.xml</c>. Most parsers expect the latter.
///   3. <c>xl/*.xml</c> parts use the <c>xmlns:x="…"</c> namespace-prefix form
///      (e.g. <c>&lt;x:workbook&gt;</c>) instead of the default-namespace form
///      (<c>xmlns="…"</c> with <c>&lt;workbook&gt;</c>). Wijmo's xlsx library
///      (used by gc-excelviewer) requires the default-namespace form.
///   4. Relationship <c>Target</c> paths are absolute
///      (<c>Target="/xl/worksheets/sheet1.xml"</c>) instead of relative
///      (<c>Target="worksheets/sheet1.xml"</c>). Excel and Wijmo both prefer
///      relative paths within <c>xl/_rels/workbook.xml.rels</c>.
/// This post-processor rewrites the .xlsx in place to the standard structure.
/// Idempotent: files that are already in the standard structure are left unchanged.
/// </summary>
public static class OoxmlStructureNormalizer
{
    private const string ContentTypesPath = "[Content_Types].xml";
    private const string PackageRelsPath = "_rels/.rels";
    private const string CorePropsPsmdcpPrefix = "package/services/metadata/core-properties/";
    private const string CorePropsStandardPath = "docProps/core.xml";
    private const string MainNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const string MainNsPrefixDecl = "xmlns:x=\"" + MainNs + "\"";

    // The xl/_rels/workbook.xml.rels targets live under the xl/ folder, so leading
    // "/xl/" must be stripped; the package _rels/.rels targets live at the package
    // root, so leading "/" must be stripped but the "xl/" prefix kept.
    private static readonly string[] XlPartPaths =
    {
        "xl/workbook.xml",
        "xl/styles.xml",
        "xl/sharedStrings.xml",
        "xl/worksheets/sheet1.xml",
        "xl/theme/theme1.xml",
    };

    public static void Normalize(string xlsxPath)
    {
        if (!File.Exists(xlsxPath))
        {
            throw new FileNotFoundException("xlsx not found", xlsxPath);
        }

        // Read all entries into memory (xlsx files are small, ~7KB).
        var entries = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        using (var read = ZipFile.OpenRead(xlsxPath))
        {
            foreach (var entry in read.Entries)
            {
                using var ms = new MemoryStream();
                using var es = entry.Open();
                es.CopyTo(ms);
                entries[entry.FullName] = ms.ToArray();
            }
        }

        // Idempotency: nothing to do if already standard.
        if (entries.ContainsKey(CorePropsStandardPath) && !HasAnyPsmdcp(entries))
        {
            if (IsContentTypesStandard(entries[ContentTypesPath]))
            {
                return;
            }
        }

        // --- 1. Normalize [Content_Types].xml ---
        entries[ContentTypesPath] = NormalizeContentTypes(entries[ContentTypesPath]);

        // --- 2. Move .psmdcp (if any) to docProps/core.xml ---
        var psmdcpKeys = entries.Keys
            .Where(k => k.StartsWith(CorePropsPsmdcpPrefix, StringComparison.OrdinalIgnoreCase) &&
                        k.EndsWith(".psmdcp", StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (psmdcpKeys.Count > 0)
        {
            entries[CorePropsStandardPath] = ReSerializeXml(entries[psmdcpKeys[0]]);
            foreach (var key in psmdcpKeys)
            {
                entries.Remove(key);
            }
        }
        else if (!entries.ContainsKey(CorePropsStandardPath))
        {
            entries[CorePropsStandardPath] = BuildMinimalCoreProperties();
        }

        // --- 3. Update _rels/.rels to reference docProps/core.xml ---
        entries[PackageRelsPath] = NormalizePackageRels(entries[PackageRelsPath]);

        // --- 4. Convert x: namespace prefix to default namespace on xl/*.xml parts ---
        foreach (var part in XlPartPaths)
        {
            if (entries.TryGetValue(part, out var bytes))
            {
                entries[part] = NormalizeXlNamespace(bytes);
            }
        }

        // --- 5. Normalize target paths in workbook rels (drop leading "/xl/") ---
        if (entries.TryGetValue("xl/_rels/workbook.xml.rels", out var wbRels))
        {
            entries["xl/_rels/workbook.xml.rels"] = NormalizeXlRelsTargetPaths(wbRels);
        }

        // --- Write the new zip atomically ---
        var tempPath = xlsxPath + ".tmp";
        using (var write = ZipFile.Open(tempPath, ZipArchiveMode.Create))
        {
            foreach (var (name, bytes) in entries)
            {
                var entry = write.CreateEntry(name, CompressionLevel.Optimal);
                using var es = entry.Open();
                es.Write(bytes, 0, bytes.Length);
            }
        }
        File.Move(tempPath, xlsxPath, overwrite: true);
    }

    private static bool HasAnyPsmdcp(Dictionary<string, byte[]> entries) =>
        entries.Keys.Any(k => k.EndsWith(".psmdcp", StringComparison.OrdinalIgnoreCase));

    private static bool IsContentTypesStandard(byte[] bytes)
    {
        var xml = Encoding.UTF8.GetString(bytes);
        return xml.Contains("Extension=\"xml\" ContentType=\"application/xml\"", StringComparison.Ordinal)
            && !xml.Contains("Extension=\"psmdcp\"", StringComparison.OrdinalIgnoreCase);
    }

    private static byte[] NormalizeContentTypes(byte[] original)
    {
        var doc = new XmlDocument();
        using (var sr = new StreamReader(new MemoryStream(original), detectEncodingFromByteOrderMarks: true))
        using (var xr = XmlReader.Create(sr))
        {
            doc.Load(xr);
        }

        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("ct", "http://schemas.openxmlformats.org/package/2006/content-types");

        var defaults = doc.SelectNodes("//ct:Default[@Extension='xml' or @Extension='psmdcp']", nsmgr);
        if (defaults != null)
        {
            for (int i = defaults.Count - 1; i >= 0; i--)
            {
                var node = defaults[i]!;
                if (node.Attributes?["Extension"]?.Value.Equals("xml", StringComparison.OrdinalIgnoreCase) == true)
                {
                    node.Attributes!["ContentType"]!.Value = "application/xml";
                }
                else
                {
                    node.ParentNode!.RemoveChild(node);
                }
            }
        }

        var existing = doc.SelectSingleNode("//ct:Default[@Extension='xml']", nsmgr);
        if (existing == null)
        {
            var newDefault = doc.CreateElement("Default", nsmgr.LookupNamespace("ct")!);
            newDefault.SetAttribute("Extension", "xml");
            newDefault.SetAttribute("ContentType", "application/xml");
            doc.DocumentElement!.PrependChild(newDefault);
        }

        var coreOverride = doc.SelectSingleNode("//ct:Override[@PartName='/docProps/core.xml']", nsmgr);
        if (coreOverride == null)
        {
            var newOverride = doc.CreateElement("Override", nsmgr.LookupNamespace("ct")!);
            newOverride.SetAttribute("PartName", "/docProps/core.xml");
            newOverride.SetAttribute("ContentType", "application/vnd.openxmlformats-package.core-properties+xml");
            doc.DocumentElement!.AppendChild(newOverride);
        }

        var workbookOverride = doc.SelectSingleNode("//ct:Override[@PartName='/xl/workbook.xml']", nsmgr);
        if (workbookOverride == null)
        {
            var newWb = doc.CreateElement("Override", nsmgr.LookupNamespace("ct")!);
            newWb.SetAttribute("PartName", "/xl/workbook.xml");
            newWb.SetAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml");
            doc.DocumentElement!.AppendChild(newWb);
        }

        return BeautifyXml(doc);
    }

    private static byte[] NormalizePackageRels(byte[] original)
    {
        var doc = new XmlDocument();
        using (var sr = new StreamReader(new MemoryStream(original), detectEncodingFromByteOrderMarks: true))
        using (var xr = XmlReader.Create(sr))
        {
            doc.Load(xr);
        }

        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("r", "http://schemas.openxmlformats.org/package/2006/relationships");

        const string corePropsType = "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties";
        const string corePropsTarget = "docProps/core.xml";

        var existing = doc.SelectNodes("//r:Relationship[@Type='" + corePropsType + "']", nsmgr);
        if (existing != null)
        {
            for (int i = existing.Count - 1; i >= 0; i--)
            {
                existing[i]!.ParentNode!.RemoveChild(existing[i]!);
            }
        }

        var rel = doc.CreateElement("Relationship", nsmgr.LookupNamespace("r")!);
        rel.SetAttribute("Id", "rIdCoreProps");
        rel.SetAttribute("Type", corePropsType);
        rel.SetAttribute("Target", corePropsTarget);
        doc.DocumentElement!.AppendChild(rel);

        return BeautifyXml(doc);
    }

    /// <summary>
    /// Convert ClosedXML's <c>xmlns:x="…"</c> namespace-prefix form to the default
    /// namespace form. After this, <c>&lt;x:workbook&gt;</c> becomes <c>&lt;workbook&gt;</c>
    /// and <c>&lt;x:tag&gt;</c> becomes <c>&lt;tag&gt;</c> throughout the document.
    /// </summary>
    private static byte[] NormalizeXlNamespace(byte[] original)
    {
        var xml = Encoding.UTF8.GetString(original);
        if (!xml.Contains("xmlns:x=\"", StringComparison.Ordinal))
        {
            return original;
        }

        var doc = new XmlDocument();
        using (var ms = new MemoryStream(original))
        using (var sr = new StreamReader(ms, detectEncodingFromByteOrderMarks: true))
        using (var xr = XmlReader.Create(sr))
        {
            doc.Load(xr);
        }

        var xdoc = System.Xml.Linq.XDocument.Parse(doc.OuterXml);
        var mainNs = System.Xml.Linq.XNamespace.Get(MainNs);

        var elements = xdoc.Descendants().ToList();
        foreach (var el in elements)
        {
            if (el.Name.NamespaceName == MainNs)
            {
                el.Name = mainNs.GetName(el.Name.LocalName);
            }
        }
        var root = xdoc.Root;
        if (root != null)
        {
            root.Attribute(System.Xml.Linq.XNamespace.Xmlns + "x")?.Remove();
            if (root.Name.Namespace == mainNs && root.Attribute("xmlns") == null)
            {
                root.SetAttributeValue("xmlns", mainNs.NamespaceName);
            }
        }

        foreach (var el in xdoc.Descendants())
        {
            var attrs = el.Attributes().ToList();
            foreach (var attr in attrs)
            {
                if (attr.Name.NamespaceName == MainNs)
                {
                    var newAttr = new System.Xml.Linq.XAttribute(mainNs.GetName(attr.Name.LocalName), attr.Value);
                    attr.Remove();
                    el.Add(newAttr);
                }
            }
        }

        return Encoding.UTF8.GetBytes(xdoc.Declaration + xdoc.ToString());
    }

    /// <summary>
    /// Strip leading <c>/xl/</c> from relationship Target paths inside
    /// <c>xl/_rels/workbook.xml.rels</c>.
    /// </summary>
    private static byte[] NormalizeXlRelsTargetPaths(byte[] original)
    {
        var doc = new XmlDocument();
        using (var ms = new MemoryStream(original))
        using (var sr = new StreamReader(ms, detectEncodingFromByteOrderMarks: true))
        using (var xr = XmlReader.Create(sr))
        {
            doc.Load(xr);
        }

        var nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("r", "http://schemas.openxmlformats.org/package/2006/relationships");

        var rels = doc.SelectNodes("//r:Relationship", nsmgr);
        if (rels != null)
        {
            foreach (XmlNode rel in rels)
            {
                var target = rel.Attributes?["Target"]?.Value;
                if (target != null && target.StartsWith("/xl/", StringComparison.Ordinal))
                {
                    rel.Attributes!["Target"]!.Value = target.Substring(4);
                }
                else if (target != null && target.StartsWith("/", StringComparison.Ordinal))
                {
                    rel.Attributes!["Target"]!.Value = target.Substring(1);
                }
            }
        }

        return BeautifyXml(doc);
    }

    private static byte[] ReSerializeXml(byte[] original)
    {
        var doc = new XmlDocument();
        using (var ms = new MemoryStream(original))
        using (var sr = new StreamReader(ms, detectEncodingFromByteOrderMarks: true))
        using (var xr = XmlReader.Create(sr))
        {
            doc.Load(xr);
        }
        var ms2 = new MemoryStream();
        var settings = new XmlWriterSettings
        {
            Indent = false,
            OmitXmlDeclaration = false,
            Encoding = new UTF8Encoding(false),
        };
        using (var writer = XmlWriter.Create(ms2, settings))
        {
            doc.Save(writer);
        }
        return ms2.ToArray();
    }

    private static byte[] BuildMinimalCoreProperties() =>
        Encoding.UTF8.GetBytes(
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
            "<cp:coreProperties xmlns:cp=\"http://schemas.openxmlformats.org/package/2006/metadata/core-properties\" " +
            "xmlns:dc=\"http://purl.org/dc/elements/1.1/\" " +
            "xmlns:dcterms=\"http://purl.org/dc/terms/\" " +
            "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
            "<dc:creator>QLDA.Gen</dc:creator>" +
            "<dcterms:created xsi:type=\"dcterms:W3CDTF\">2026-01-01T00:00:00Z</dcterms:created>" +
            "<dcterms:modified xsi:type=\"dcterms:W3CDTF\">2026-01-01T00:00:00Z</dcterms:modified>" +
            "</cp:coreProperties>");

    private static byte[] BeautifyXml(XmlDocument doc)
    {
        var ms = new MemoryStream();
        var settings = new XmlWriterSettings
        {
            Indent = false,
            OmitXmlDeclaration = false,
            Encoding = new UTF8Encoding(false),
        };
        using (var writer = XmlWriter.Create(ms, settings))
        {
            doc.Save(writer);
        }
        return ms.ToArray();
    }
}

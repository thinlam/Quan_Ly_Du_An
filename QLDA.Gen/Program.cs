using QLDA.Gen.Descriptors;
using QLDA.Gen.Generators;

namespace QLDA.Gen;

class Program
{
    static int Main(string[] args)
    {
        var allFlag = args.Contains("--all");
        var force = args.Contains("--force");
        var positional = args
            .Where(a => a != "--all" && a != "--force")
            .ToArray();

        // Optional first positional arg may be an output directory.
        string? customPath = positional.FirstOrDefault(IsPath);
        if (customPath != null)
        {
            positional = positional.Where(a => a != customPath).ToArray();
        }

        var basePath = customPath ?? Path.Combine(
            Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName ?? "",
            "QLDA.WebApi", "ExportTemplates");

        var targets = BuildRegistry(basePath);
        var availableSlugs = targets.Select(t => t.Slug).ToList();

        // Selective mode requires explicit slugs.
        if (!allFlag && positional.Length == 0)
        {
            PrintUsage(basePath, availableSlugs);
            return 0;
        }

        // Validate unknown slugs.
        var unknown = positional.Except(availableSlugs, StringComparer.OrdinalIgnoreCase).ToList();
        if (unknown.Count > 0)
        {
            Console.Error.WriteLine($"Unknown template slug(s): {string.Join(", ", unknown)}");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Available slugs:");
            foreach (var s in availableSlugs) Console.Error.WriteLine($"  {s}");
            return 1;
        }

        var selected = allFlag
            ? targets
            : targets.Where(t => positional.Contains(t.Slug, StringComparer.OrdinalIgnoreCase)).ToList();

        Console.WriteLine($"Output path: {basePath}");
        Console.WriteLine(force
            ? "Mode: overwrite (--force set)"
            : "Mode: skip existing (pass --force to overwrite)");
        Console.WriteLine();

        var generator = new TemplateGenerator(basePath, force);
        foreach (var t in selected)
        {
            t.Run(generator);
        }

        Console.WriteLine();
        Console.WriteLine($"Done. {selected.Count} template(s) processed.");
        return 0;
    }

    private record TemplateEntry(string Slug, Action<TemplateGenerator> Run);

    private static List<TemplateEntry> BuildRegistry(string basePath) =>
    [
        new("danh-sach-du-an",
            g => g.GenerateTemplate(CreateDescriptor<DanhSachDuAnExportDescriptor>(basePath))),
        new("danh-sach-tong-hop-van-ban-quyet-dinh",
            g => g.GenerateTemplate(CreateDescriptor<DanhSachTongHopVanBanQuyetDinhExportDescriptor>(basePath))),
        new("tong-hop-nhu-cau-kinh-phi-nam",
            g => g.GenerateTemplate(CreateDescriptor<TongHopNhuCauKinhPhiNamExportDescriptor>(basePath))),
        new("de-xuat-nhu-cau-kinh-phi-chu-truong",
            g => g.GenerateTemplate(CreateDescriptor<DeXuatNhuCauKinhPhiChuTruongExportDescriptor>(basePath))),
        new("ke-hoach-trien-khai-hang-muc",
            g => g.GenerateTemplate(CreateDescriptor<KeHoachTrienKhaiHangMucExportDescriptor>(basePath))),
        new("tinh-hinh-thuc-hien-dau-thau",
            g => g.GenerateTemplate(CreateDescriptor<TinhHinhThucHienDauThauExportDescriptor>(basePath))),
        new("danh-sach-ban-giao-ho-so",
            g => g.GenerateTemplate(CreateDescriptor<DanhSachBanGiaoHoSoExportDescriptor>(basePath))),
        new("danh-sach-file-ban-giao-ho-so",
            g => g.GenerateTemplate(CreateDescriptor<DanhSachFileBanGiaoHoSoExportDescriptor>(basePath))),
        new("danh-sach-noi-dung-da-ky",
            g => g.GenerateTemplate(CreateDescriptor<DanhSachNoiDungDaKyExportDescriptor>(basePath))),
        new("import-phan-khai-kinh-phi",
            g => g.GenerateImportTemplate(CreateImportDescriptor<PhanKhaiKinhPhiImportDescriptor>(basePath))),
        new("import-ke-hoach-trien-khai-hang-muc",
            g => g.GenerateImportTemplate(CreateImportDescriptor<KeHoachTrienKhaiHangMucImportDescriptor>(basePath))),
    ];

    private static T CreateImportDescriptor<T>(string basePath) where T : IImportDescriptor, new()
    {
        var descriptor = new T();
        descriptor.OutputPath = Path.Combine(basePath, descriptor.TemplateFileName);
        return descriptor;
    }

    private static T CreateDescriptor<T>(string basePath) where T : IExportDescriptor, new()
    {
        var descriptor = new T();
        descriptor.OutputPath = Path.Combine(basePath, descriptor.TemplateFileName);
        return descriptor;
    }

    private static bool IsPath(string arg) =>
        arg.StartsWith('/') || arg.StartsWith('\\') || arg.StartsWith('~')
        || (arg.Length >= 2 && char.IsLetter(arg[0]) && arg[1] == ':')   // Windows drive
        || arg.StartsWith("./", StringComparison.Ordinal)
        || arg.StartsWith("../", StringComparison.Ordinal);

    private static void PrintUsage(string basePath, List<string> slugs)
    {
        Console.WriteLine("QLDA.Gen — Excel template generator");
        Console.WriteLine($"Output path (default): {basePath}");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run -- <slug1> [<slug2> ...] [--force] [<output-dir>]");
        Console.WriteLine("  dotnet run -- --all [--force]");
        Console.WriteLine();
        Console.WriteLine("Modes (combine flags as needed):");
        Console.WriteLine("  <slug(s)>    Generate only the specified template(s).");
        Console.WriteLine("  --all        Generate all registered templates.");
        Console.WriteLine("  --force      Overwrite existing files. WITHOUT --force, existing files are skipped.");
        Console.WriteLine("               This protects hand-fixed templates from being clobbered by accident.");
        Console.WriteLine("  --all --force  Generate all + overwrite (highest-power mode).");
        Console.WriteLine("  <output-dir> Optional first positional arg. Use as base path instead of default.");
        Console.WriteLine();
        Console.WriteLine("Available template slugs:");
        foreach (var s in slugs) Console.WriteLine($"  {s}");
    }
}

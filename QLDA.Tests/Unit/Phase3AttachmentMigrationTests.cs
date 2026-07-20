using System.Reflection;
using System.Text.Json;
using BuildingBlocks.Application.Attachments.Commands;
using BuildingBlocks.Application.Attachments.Common;
using BuildingBlocks.Application.Attachments.DTOs;
using BuildingBlocks.Application.Attachments.Queries;
using BuildingBlocks.Domain.Entities;
using FluentAssertions;
using QLDA.Application.TepDinhKems.DTOs;
using Xunit;

namespace QLDA.Tests.Unit;

/// <summary>
/// Phase 3 regression: DTO compatibility + mapping + migration static checks.
/// </summary>
public class Phase3AttachmentMigrationTests
{
    [Fact]
    public void TepDinhKemDto_InheritsAttachmentDto_AndKeepsQlDaFields()
    {
        typeof(AttachmentDto).IsAssignableFrom(typeof(TepDinhKemDto)).Should().BeTrue();

        var dto = new TepDinhKemDto
        {
            Id = Guid.NewGuid(),
            GroupId = "g",
            GroupType = "BanGiaoHoSo",
            FileName = "a.pdf",
            OriginalName = "a.pdf",
            Path = "/x",
            Size = 1,
            TenNguoiTao = "Nguyen Van A",
            CreatedBy = "u1",
        };

        dto.TenNguoiTao.Should().Be("Nguyen Van A");
        dto.FileName.Should().Be("a.pdf");
        dto.GetId().Should().Be(dto.Id!.Value);
    }

    [Fact]
    public void TepDinhKemDto_JsonSerialize_KeepsContractNames_AndHidesAudit()
    {
        var dto = new TepDinhKemDto
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            GroupId = "g1",
            GroupType = "HopDong",
            FileName = "f.pdf",
            OriginalName = "orig.pdf",
            Path = "/p",
            Size = 10,
            TenNguoiTao = "A",
            CreatedBy = "secret-user",
            UpdatedBy = "secret-user-2",
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        json.Should().Contain("\"fileName\"");
        json.Should().Contain("\"groupType\"");
        json.Should().Contain("\"tenNguoiTao\"");
        json.Should().Contain("\"id\"");
        json.Should().NotContain("secret-user");
        json.Should().NotContain("createdBy");
        json.Should().NotContain("updatedBy");
    }

    [Fact]
    public void TepDinhKemDto_ToEntities_UsesSignedGroupTypeHelper()
    {
        var groupId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var dtos = new List<TepDinhKemDto>
        {
            new() { FileName = "goc.pdf", OriginalName = "goc.pdf", Size = 1 },
            new() { FileName = "ky.pdf", OriginalName = "ky.pdf", Size = 2, ParentId = parentId },
        };

        var entities = dtos.ToEntities(groupId, "KhoKhanVuongMac").ToList();

        entities.Should().HaveCount(2);
        entities[0].GroupType.Should().Be("KhoKhanVuongMac");
        entities[1].GroupType.Should().Be("KySo_KhoKhanVuongMac");
        entities[0].GroupId.Should().Be(groupId.ToString());
    }

    [Fact]
    public void Attachment_ToTepDinhKemDto_MapsCoreFields()
    {
        var entity = new Attachment
        {
            Id = Guid.NewGuid(),
            GroupId = "g",
            GroupType = "VanBanPhapLy",
            FileName = "vb.pdf",
            OriginalName = "vb-orig.pdf",
            Path = "/vb",
            Size = 42,
            Type = "pdf",
            ParentId = null,
        };

        var dto = TepDinhKemMappingConfiguration.ToDto(entity);

        dto.Should().BeOfType<TepDinhKemDto>();
        dto.FileName.Should().Be("vb.pdf");
        dto.GroupType.Should().Be("VanBanPhapLy");
        dto.Size.Should().Be(42);
    }

    [Fact]
    public void AttachmentBulkInsertOrUpdateCommand_RequiresGroupTypes_AndDefaultAutoDeleteFalse()
    {
        var cmd = new AttachmentBulkInsertOrUpdateCommand
        {
            GroupId = "g",
            GroupTypes = ["BanGiaoHoSo"],
            Entities = [],
        };

        cmd.AutoDeleteMissing.Should().BeFalse();
        cmd.GroupTypes.Should().NotBeEmpty();
    }

    [Fact]
    public void GetAttachmentsQuery_Record_ExistsWithIncludeSignedDefaultTrue()
    {
        var q = new GetAttachmentsQuery(["id-1"]);
        q.GroupIds.Should().ContainSingle("id-1");
        q.IncludeSigned.Should().BeTrue();
        q.BaseGroupTypes.Should().BeNull();
    }

    [Fact]
    public void WebApiControllers_DoNotCall_TepDinhKemBulkInsertOrUpdateCommand()
    {
        var webApiDir = FindWebApiControllersDir();
        var offenders = Directory.GetFiles(webApiDir, "*Controller.cs")
            .Select(f => (File: f, Text: File.ReadAllText(f)))
            .Where(x => x.Text.Contains("TepDinhKemBulkInsertOrUpdateCommand", StringComparison.Ordinal)
                        && !IsOnlyInComment(x.Text, "TepDinhKemBulkInsertOrUpdateCommand"))
            .Select(x => Path.GetFileName(x.File))
            .ToList();

        offenders.Should().BeEmpty(
            "Phase 3: controllers must use AttachmentBulkInsertOrUpdateCommand. Offenders: {0}",
            string.Join(", ", offenders));
    }

    [Fact]
    public void WebApiControllers_AttachmentBulkInsert_AlwaysPassGroupTypes()
    {
        var webApiDir = FindWebApiControllersDir();
        var offenders = new List<string>();

        foreach (var file in Directory.GetFiles(webApiDir, "*Controller.cs"))
        {
            var text = StripComments(File.ReadAllText(file));
            if (!text.Contains("new AttachmentBulkInsertOrUpdateCommand", StringComparison.Ordinal))
                continue;

            var idx = 0;
            while ((idx = text.IndexOf("new AttachmentBulkInsertOrUpdateCommand", idx, StringComparison.Ordinal)) >= 0)
            {
                var sliceEnd = Math.Min(text.Length, idx + 500);
                var slice = text[idx..sliceEnd];
                if (!slice.Contains("GroupTypes", StringComparison.Ordinal))
                    offenders.Add($"{Path.GetFileName(file)}@{idx}");
                idx += 1;
            }
        }

        offenders.Should().BeEmpty(
            "Every AttachmentBulkInsertOrUpdateCommand must set GroupTypes. Offenders: {0}",
            string.Join(", ", offenders));
    }

    private static string StripComments(string text)
    {
        // Remove /* */ then // line comments so dead commented code does not fail static checks.
        text = System.Text.RegularExpressions.Regex.Replace(
            text, @"/\*.*?\*/", " ", System.Text.RegularExpressions.RegexOptions.Singleline);
        text = System.Text.RegularExpressions.Regex.Replace(
            text, @"//.*?$", " ", System.Text.RegularExpressions.RegexOptions.Multiline);
        return text;
    }

    [Fact]
    public void TepDinhKemDto_HasNoDuplicateCoreProperties_VsAttachmentDto()
    {
        var baseProps = typeof(AttachmentDto)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        var declaredOnChild = typeof(TepDinhKemDto)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(p => p.Name)
            .ToList();

        var duplicates = declaredOnChild.Where(baseProps.Contains).ToList();
        duplicates.Should().BeEmpty(
            "TepDinhKemDto must not redeclare AttachmentDto fields: {0}",
            string.Join(", ", duplicates));
    }

    private static string FindWebApiControllersDir()
    {
        // QLDA.Tests runs from bin/... → walk up to repo then QLDA.WebApi/Controllers
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "QLDA.WebApi", "Controllers");
            if (Directory.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("QLDA.WebApi/Controllers not found from test base directory.");
    }

    private static bool IsOnlyInComment(string text, string token)
    {
        // Rough: if every occurrence is inside /* */ block, treat as comment-only.
        var idx = 0;
        while ((idx = text.IndexOf(token, idx, StringComparison.Ordinal)) >= 0)
        {
            var before = text[..idx];
            var lastOpen = before.LastIndexOf("/*", StringComparison.Ordinal);
            var lastClose = before.LastIndexOf("*/", StringComparison.Ordinal);
            var inBlock = lastOpen > lastClose;
            if (!inBlock)
                return false;
            idx += token.Length;
        }

        return true;
    }
}

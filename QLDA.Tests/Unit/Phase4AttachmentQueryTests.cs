using BuildingBlocks.Application.Attachments.Common;
using BuildingBlocks.Application.Attachments.Queries;
using FluentAssertions;
using QLDA.Domain.Enums;
using Xunit;

namespace QLDA.Tests.Unit;

/// <summary>
/// Phase 4: multi-GroupType expand + GetAttachmentsQuery validation surface.
/// </summary>
public class Phase4AttachmentQueryTests
{
    [Fact]
    public void ExpandGroupTypes_BanGiaoHoSo_MatchesLegacySignedHelperBehavior()
    {
        var types = AttachmentSubquery.ExpandGroupTypes(
            includeSigned: true, nameof(EGroupType.BanGiaoHoSo));

        types.Should().BeEquivalentTo(
            nameof(EGroupType.BanGiaoHoSo),
            SignedGroupTypeHelper.Prefix + nameof(EGroupType.BanGiaoHoSo));
    }

    [Fact]
    public void ExpandGroupTypes_KhoKhanNested_TwoScopesIndependent()
    {
        var parent = AttachmentSubquery.ExpandGroupTypes(
            includeSigned: true, nameof(EGroupType.KhoKhanVuongMac));
        var nested = AttachmentSubquery.ExpandGroupTypes(
            includeSigned: true, nameof(EGroupType.KetQuaXuLyKhoKhanVuongMac));

        parent.Should().NotContain(nameof(EGroupType.KetQuaXuLyKhoKhanVuongMac));
        nested.Should().NotContain(nameof(EGroupType.KhoKhanVuongMac));
        nested.Should().Contain(SignedGroupTypeHelper.Prefix + nameof(EGroupType.KetQuaXuLyKhoKhanVuongMac));
    }

    [Fact]
    public void ExpandGroupTypes_ThanhLyMulti_ThreeBases()
    {
        var types = AttachmentSubquery.ExpandGroupTypes(
            includeSigned: true,
            nameof(EGroupType.ThanhLyHopDong_BienBanNghiemThu),
            nameof(EGroupType.ThanhLyHopDong),
            nameof(EGroupType.ThanhLyHopDong_Khac));

        types.Should().HaveCount(6);
        types.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void GetAttachmentsQuery_Record_DefaultsIncludeSignedTrue()
    {
        var q = new GetAttachmentsQuery(GroupIds: ["g1"], BaseGroupTypes: ["HopDong"]);
        q.IncludeSigned.Should().BeTrue();
        q.GroupIds.Should().ContainSingle("g1");
    }

    [Fact]
    public void Controllers_NoLongerUseSignedHelperPrefixInApplicationQueries()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "QLDA.Application"));
        Directory.Exists(root).Should().BeTrue($"expected QLDA.Application at {root}");

        var hits = Directory.EnumerateFiles(root, "*Query*.cs", SearchOption.AllDirectories)
            .SelectMany(f => File.ReadAllLines(f).Select((line, i) => (f, i: i + 1, line)))
            .Where(x => x.line.Contains("SignedHelper.Prefix", StringComparison.Ordinal))
            .ToList();

        hits.Should().BeEmpty(
            "Phase 4 query handlers must use AttachmentSubquery.ExpandGroupTypes, not SignedHelper.Prefix. Hits: "
            + string.Join("; ", hits.Select(h => $"{Path.GetFileName(h.f)}:{h.i}")));
    }
}

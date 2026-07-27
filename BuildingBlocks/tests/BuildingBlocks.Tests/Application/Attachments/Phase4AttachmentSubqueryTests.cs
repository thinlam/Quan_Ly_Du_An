using BuildingBlocks.Application.Attachments.Common;
using BuildingBlocks.Domain.Entities;
using Xunit;

namespace BuildingBlocks.Tests.Application.Attachments;

public class Phase4AttachmentSubqueryTests
{
    private static IQueryable<Attachment> Seed()
    {
        var a = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var b = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var list = new List<Attachment>
        {
            new() { Id = Guid.NewGuid(), GroupId = "X", GroupType = "HopDong", FileName = "1.pdf", OriginalName = "1.pdf", Path = "/", Size = 1 },
            new() { Id = Guid.NewGuid(), GroupId = "X", GroupType = "KySo_HopDong", FileName = "1s.pdf", OriginalName = "1s.pdf", Path = "/", Size = 1 },
            new() { Id = Guid.NewGuid(), GroupId = "X", GroupType = "BienBan", FileName = "2.pdf", OriginalName = "2.pdf", Path = "/", Size = 1 },
            new() { Id = Guid.NewGuid(), GroupId = "A", GroupType = "HopDong", FileName = "a.pdf", OriginalName = "a.pdf", Path = "/", Size = 1 },
            new() { Id = Guid.NewGuid(), GroupId = "B", GroupType = "HopDong", FileName = "b.pdf", OriginalName = "b.pdf", Path = "/", Size = 1 },
            new() { Id = a, GroupId = "X", GroupType = "KySo_HopDong", FileName = "dup.pdf", OriginalName = "dup.pdf", Path = "/", Size = 1, ParentId = b },
        };
        return list.AsQueryable();
    }

    [Fact]
    public void ExpandGroupTypes_OneBase_NoSigned()
    {
        var types = AttachmentSubquery.ExpandGroupTypes(includeSigned: false, "HopDong");
        Assert.Equal(["HopDong"], types);
    }

    [Fact]
    public void ExpandGroupTypes_OneBase_WithSigned()
    {
        var types = AttachmentSubquery.ExpandGroupTypes(includeSigned: true, "HopDong");
        Assert.Equal(2, types.Count);
        Assert.Contains("HopDong", types);
        Assert.Contains("KySo_HopDong", types);
    }

    [Fact]
    public void ExpandGroupTypes_Multi_AndDedupes()
    {
        var types = AttachmentSubquery.ExpandGroupTypes(
            includeSigned: true,
            "HopDong",
            "PhuLucHopDong",
            "HopDong",
            "  ");
        Assert.Equal(4, types.Count);
        Assert.Contains("HopDong", types);
        Assert.Contains("KySo_HopDong", types);
        Assert.Contains("PhuLucHopDong", types);
        Assert.Contains("KySo_PhuLucHopDong", types);
    }

    [Fact]
    public void ExpandGroupTypes_AlreadyPrefixed_NoDoublePrefix()
    {
        var types = AttachmentSubquery.ExpandGroupTypes(includeSigned: true, "KySo_HopDong");
        Assert.Equal(["KySo_HopDong"], types);
        Assert.DoesNotContain("KySo_KySo_HopDong", types);
    }

    [Fact]
    public void ExpandGroupTypes_EmptyOrWhitespace_ReturnsEmpty()
    {
        Assert.Empty(AttachmentSubquery.ExpandGroupTypes(includeSigned: true, "  ", ""));
        Assert.Empty(AttachmentSubquery.ExpandGroupTypes(null, includeSigned: true));
    }

    [Fact]
    public void ForGroupTypes_SameGroupId_ExcludesOutOfScopeType()
    {
        var result = Seed().ForGroupTypes("X", includeSigned: true, "HopDong").ToList();
        Assert.Equal(3, result.Count); // HopDong + 2 KySo_HopDong
        Assert.All(result, a => Assert.Equal("X", a.GroupId));
        Assert.DoesNotContain(result, a => a.GroupType == "BienBan");
    }

    [Fact]
    public void ForGroupTypes_IncludeSignedFalse_OriginalOnly()
    {
        var result = Seed().ForGroupTypes("X", includeSigned: false, "HopDong").ToList();
        Assert.Single(result);
        Assert.Equal("HopDong", result[0].GroupType);
    }

    [Fact]
    public void SignedOnly_ReturnsOnlySigned()
    {
        var result = Seed().SignedOnly("X", "HopDong").ToList();
        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.Equal("KySo_HopDong", a.GroupType));
    }

    [Fact]
    public void OriginalOnly_ReturnsOnlyBase()
    {
        var result = Seed().OriginalOnly("X", "HopDong").ToList();
        Assert.Single(result);
        Assert.Equal("HopDong", result[0].GroupType);
    }

    [Fact]
    public void OriginalOnly_AlreadySigned_Throws()
    {
        Assert.Throws<ArgumentException>(() => Seed().OriginalOnly("X", "KySo_HopDong").ToList());
    }

    [Fact]
    public void ForExactGroupType_DoesNotNormalize()
    {
        var result = Seed().ForExactGroupType("X", "KySo_HopDong").ToList();
        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.Equal("KySo_HopDong", a.GroupType));
    }

    [Fact]
    public void ForGroupTypes_DoesNotReturnOtherGroupId()
    {
        var result = Seed().ForGroupTypes("A", includeSigned: true, "HopDong").ToList();
        Assert.Single(result);
        Assert.Equal("A", result[0].GroupId);
        Assert.DoesNotContain(result, a => a.GroupId == "B");
    }

    [Fact]
    public void SignedOnly_AlreadyPrefixedBase_NoDoublePrefix()
    {
        var result = Seed().SignedOnly("X", "KySo_HopDong").ToList();
        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.Equal("KySo_HopDong", a.GroupType));
    }
}

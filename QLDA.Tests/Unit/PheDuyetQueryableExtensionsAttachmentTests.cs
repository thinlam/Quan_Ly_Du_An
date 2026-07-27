using BuildingBlocks.Domain.Entities;
using FluentAssertions;
using QLDA.Application.QuanLyPheDuyet.DTOs;
using QLDA.Application.QuanLyPheDuyet.Queries;
using Xunit;

namespace QLDA.Tests.Unit;

public class PheDuyetQueryableExtensionsAttachmentTests
{
    [Fact]
    public void AssignAttachments_MatchesGuidEntityId_ViaToString()
    {
        var entityId = Guid.Parse("08ded4c9-2194-2ddf-b3bf-2f1810035a0a");
        var items = new List<PheDuyetListItemDto>
        {
            new() { EntityId = entityId.ToString() },
        };
        var files = new List<Attachment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                GroupId = "08ded4c9-2194-2ddf-b3bf-2f1810035a0a",
                FileName = "a.pdf",
            },
        };

        PheDuyetQueryableExtensions.AssignAttachments(items, files);

        items[0].DanhSachTepDinhKem.Should().NotBeNull();
        items[0].DanhSachTepDinhKem.Should().HaveCount(1);
        items[0].DanhSachTepDinhKem[0].GroupId.Should().Be(entityId.ToString());
    }

    [Fact]
    public void AssignAttachments_MatchesLongEntityId_ViaToString()
    {
        // GroupId luôn là string; EntityId (Guid/long) chuẩn hóa bằng ToString().
        const long entityId = 123L;
        var items = new List<PheDuyetListItemDto>
        {
            new() { EntityId = entityId.ToString() },
        };
        var files = new List<Attachment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                GroupId = "123",
                FileName = "b.pdf",
            },
        };

        PheDuyetQueryableExtensions.AssignAttachments(items, files);

        items[0].DanhSachTepDinhKem.Should().NotBeNull();
        items[0].DanhSachTepDinhKem.Should().HaveCount(1);
        items[0].DanhSachTepDinhKem[0].GroupId.Should().Be("123");
    }

    [Fact]
    public void AssignAttachments_NoMatchingFile_ReturnsEmptyListNotNull()
    {
        var items = new List<PheDuyetListItemDto>
        {
            new() { EntityId = Guid.NewGuid().ToString() },
        };

        PheDuyetQueryableExtensions.AssignAttachments(items, []);

        items[0].DanhSachTepDinhKem.Should().NotBeNull();
        items[0].DanhSachTepDinhKem.Should().BeEmpty();
    }

    [Fact]
    public void AssignAttachments_CaseInsensitiveGroupId_StillMatches()
    {
        var entityId = Guid.Parse("08ded4c9-2194-2ddf-b3bf-2f1810035a0a");
        var items = new List<PheDuyetListItemDto>
        {
            new() { EntityId = entityId.ToString() }, // lowercase
        };
        var files = new List<Attachment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                GroupId = entityId.ToString().ToUpperInvariant(),
                FileName = "c.pdf",
            },
        };

        PheDuyetQueryableExtensions.AssignAttachments(items, files);

        items[0].DanhSachTepDinhKem.Should().HaveCount(1);
    }

    [Fact]
    public void PheDuyetListItemDto_DanhSachTepDinhKem_DefaultsToEmptyNotNull()
    {
        var dto = new PheDuyetListItemDto();

        dto.DanhSachTepDinhKem.Should().NotBeNull();
        dto.DanhSachTepDinhKem.Should().BeEmpty();
    }

    [Fact]
    public void IncludeAttachments_DefaultsToTrue_OnQuery()
    {
        var query = new PheDuyetGetDanhSachQuery();

        query.IncludeAttachments.Should().BeTrue();
    }

    [Fact]
    public void IncludeSigned_DefaultsToTrue_OnQuery()
    {
        var query = new PheDuyetGetDanhSachQuery();

        query.IncludeSigned.Should().BeTrue();
    }

    [Fact]
    public void AssignAttachments_IncludesSignedFiles_WithParentId()
    {
        var entityId = Guid.Parse("08dee930-5abe-25bd-687a-7b4630000f5a");
        var parentId = Guid.NewGuid();
        var items = new List<PheDuyetListItemDto>
        {
            new()
            {
                EntityId = entityId.ToString(),
                EntityName = "HoSoMoiThauDienTu",
            },
        };
        var files = new List<Attachment>
        {
            new()
            {
                Id = parentId,
                GroupId = entityId.ToString(),
                GroupType = "HoSoMoiThauDienTu",
                FileName = "goc.pdf",
                ParentId = null,
            },
            new()
            {
                Id = Guid.NewGuid(),
                GroupId = entityId.ToString(),
                GroupType = "KySo_HoSoMoiThauDienTu",
                FileName = "goc.signed.pdf",
                ParentId = parentId,
            },
            new()
            {
                Id = Guid.NewGuid(),
                GroupId = entityId.ToString(),
                GroupType = "HoSoMoiThauDienTu",
                FileName = "goc2.pdf",
                ParentId = null,
            },
        };

        PheDuyetQueryableExtensions.AssignAttachments(items, files);

        items[0].DanhSachTepDinhKem.Should().HaveCount(3);
        items[0].DanhSachTepDinhKem.Should().Contain(f => f.GroupType == "KySo_HoSoMoiThauDienTu");
        items[0].DanhSachTepDinhKem.Should().Contain(f => f.ParentId == parentId);
    }

    [Fact]
    public void AssignAttachments_DeduplicatesById()
    {
        var entityId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var items = new List<PheDuyetListItemDto>
        {
            new() { EntityId = entityId.ToString() },
        };
        var dup = new Attachment
        {
            Id = fileId,
            GroupId = entityId.ToString(),
            GroupType = "HoSoMoiThauDienTu",
            FileName = "a.pdf",
        };

        PheDuyetQueryableExtensions.AssignAttachments(items, [dup, dup]);

        items[0].DanhSachTepDinhKem.Should().HaveCount(1);
    }
}


using BuildingBlocks.Application.Attachments.Common;
using BuildingBlocks.Application.Attachments.DTOs;
using BuildingBlocks.Domain.Entities;
using Xunit;

namespace BuildingBlocks.Tests.Application.Attachments;

public class SignedGroupTypeHelperTests
{
    [Fact]
    public void ResolveSignedGroupType_ParentNull_ReturnsBase()
    {
        var result = "KeHoachTrienKhaiHangMuc".ResolveSignedGroupType(isChild: false);
        Assert.Equal("KeHoachTrienKhaiHangMuc", result);
    }

    [Fact]
    public void ResolveSignedGroupType_Child_AddsPrefix()
    {
        var result = "KeHoachTrienKhaiHangMuc".ResolveSignedGroupType(isChild: true);
        Assert.Equal("KySo_KeHoachTrienKhaiHangMuc", result);
    }

    [Fact]
    public void ResolveSignedGroupType_AlreadyPrefixed_DoesNotDoublePrefix()
    {
        var result = "KySo_KeHoachTrienKhaiHangMuc".ResolveSignedGroupType(isChild: true);
        Assert.Equal("KySo_KeHoachTrienKhaiHangMuc", result);
    }

    [Fact]
    public void ToBaseGroupType_StripsPrefix()
    {
        Assert.Equal("BanGiaoHoSo", "KySo_BanGiaoHoSo".ToBaseGroupType());
        Assert.Equal("BanGiaoHoSo", "BanGiaoHoSo".ToBaseGroupType());
    }

    [Fact]
    public void ExpandWithSignedVariant_ReturnsBaseAndSigned()
    {
        var expanded = SignedGroupTypeHelper.ExpandWithSignedVariant("KhoKhanVuongMac");
        Assert.Equal(2, expanded.Length);
        Assert.Contains("KhoKhanVuongMac", expanded);
        Assert.Contains("KySo_KhoKhanVuongMac", expanded);
    }

    [Fact]
    public void WithSignedVariant_AlreadyPrefixed_NoDouble()
    {
        Assert.Equal("KySo_QLHD", SignedGroupTypeHelper.WithSignedVariant("KySo_QLHD"));
    }
}

public class AttachmentCollectionExtensionsTests
{
    [Fact]
    public void ToEntities_FromInsertOrUpdate_ResolvesSignedByParentId()
    {
        var parentId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var dtos = new List<AttachmentInsertOrUpdateModel>
        {
            new() { FileName = "goc.pdf", OriginalName = "goc.pdf", Size = 10 },
            new() { FileName = "ky.pdf", OriginalName = "ky.pdf", Size = 11, ParentId = parentId },
        };

        var entities = dtos.ToEntities(groupId, "BanGiaoHoSo");

        Assert.Equal(2, entities.Count);
        Assert.All(entities, e => Assert.Equal(groupId.ToString(), e.GroupId));
        Assert.Equal("BanGiaoHoSo", entities[0].GroupType);
        Assert.Equal("KySo_BanGiaoHoSo", entities[1].GroupType);
        Assert.Null(entities[0].ParentId);
        Assert.Equal(parentId, entities[1].ParentId);
    }

    [Fact]
    public void ToEntities_EmptyBaseGroupType_Throws()
    {
        var dtos = new List<AttachmentInsertOrUpdateModel> { new() { FileName = "a.pdf" } };
        Assert.Throws<ArgumentException>(() => dtos.ToEntities(Guid.NewGuid(), "  "));
    }

    [Fact]
    public void ToAttachmentEntities_MapsDtoFields()
    {
        var id = Guid.NewGuid();
        var dtos = new List<AttachmentDto>
        {
            new()
            {
                Id = id,
                GroupId = "g1",
                GroupType = "HopDong",
                FileName = "f.pdf",
                OriginalName = "orig.pdf",
                Path = "/p",
                Size = 99,
                Type = "pdf",
            }
        };

        var entities = dtos.ToAttachmentEntities();

        Assert.Single(entities);
        Assert.Equal(id, entities[0].Id);
        Assert.Equal("g1", entities[0].GroupId);
        Assert.Equal("HopDong", entities[0].GroupType);
        Assert.Equal("f.pdf", entities[0].FileName);
        Assert.Equal(99, entities[0].Size);
    }

    [Fact]
    public void ToEntities_FromIAttachmentDto_WorksForAttachmentDto()
    {
        IEnumerable<IAttachmentDto> dtos =
        [
            new AttachmentDto { FileName = "a.pdf", OriginalName = "a.pdf", Size = 1 },
            new AttachmentDto { FileName = "b.pdf", OriginalName = "b.pdf", Size = 2, ParentId = Guid.NewGuid() },
        ];

        var entities = dtos.ToEntities(Guid.NewGuid(), "ThanhToan");

        Assert.Equal("ThanhToan", entities[0].GroupType);
        Assert.Equal("KySo_ThanhToan", entities[1].GroupType);
    }
}

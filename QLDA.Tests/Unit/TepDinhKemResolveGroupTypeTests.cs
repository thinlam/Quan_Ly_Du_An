using BuildingBlocks.Domain.Entities;
using FluentAssertions;
using QLDA.Domain.Enums;
using QLDA.WebApi.Models.TepDinhKems;
using Xunit;

namespace QLDA.Tests.Unit;

/// <summary>
/// Form insert/update phải resolve GroupType từ base handler, bỏ qua FE
/// (tránh ParentId + GroupType="" → "KySo_").
/// </summary>
public class TepDinhKemResolveGroupTypeTests
{
    [Fact]
    public void ToEntities_FormBase_IgnoresEmptyFeGroupType_SignedChild()
    {
        var groupId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var models = new List<TepDinhKemModel>
        {
            new()
            {
                FileName = "goc.pdf",
                OriginalName = "goc.pdf",
                Path = "/goc.pdf",
                Size = 1,
                GroupType = "",
            },
            new()
            {
                FileName = "ky.pdf",
                OriginalName = "ky.pdf",
                Path = "/ky.pdf",
                Size = 2,
                ParentId = parentId,
                GroupType = "",
            },
        };

        var entities = models.ToEntities(groupId, EGroupType.DeXuatChuyenTiep).ToList();

        entities.Should().HaveCount(2);
        entities[0].GroupType.Should().Be(nameof(EGroupType.DeXuatChuyenTiep));
        entities[1].GroupType.Should().Be("KySo_DeXuatChuyenTiep");
        entities[1].ParentId.Should().Be(parentId);
    }

    [Fact]
    public void ToEntities_FormBase_IgnoresFeKySoPrefixOnly()
    {
        var groupId = Guid.NewGuid();
        var models = new List<TepDinhKemModel>
        {
            new()
            {
                FileName = "ky.pdf",
                OriginalName = "ky.pdf",
                Path = "/ky.pdf",
                Size = 2,
                ParentId = Guid.NewGuid(),
                GroupType = "KySo_",
            },
        };

        var entities = models.ToEntities(groupId, EGroupType.DeXuatChuyenTiep).ToList();

        entities.Should().ContainSingle()
            .Which.GroupType.Should().Be("KySo_DeXuatChuyenTiep");
    }

    [Fact]
    public void ToEntities_FormBase_OverwritesWrongFeGroupType()
    {
        var groupId = Guid.NewGuid();
        var models = new List<TepDinhKemModel>
        {
            new()
            {
                FileName = "a.pdf",
                OriginalName = "a.pdf",
                Path = "/a.pdf",
                Size = 1,
                GroupType = "WrongType",
            },
        };

        var entities = models.ToEntities(groupId, EGroupType.DeXuatChuyenTiep).ToList();

        entities.Should().ContainSingle()
            .Which.GroupType.Should().Be(nameof(EGroupType.DeXuatChuyenTiep));
    }

    [Fact]
    public void ToEntities_NoneBase_FallsBackToFeGroupType_ForDirectSign()
    {
        var groupId = Guid.NewGuid();
        var models = new List<TepDinhKemModel>
        {
            new()
            {
                FileName = "ky.pdf",
                OriginalName = "ky.pdf",
                Path = "/ky.pdf",
                Size = 1,
                GroupType = nameof(EGroupType.BanGiaoHoSo),
            },
        };

        var entities = models.ToEntities(groupId, EGroupType.None).ToList();

        entities.Should().ContainSingle()
            .Which.GroupType.Should().Be(nameof(EGroupType.BanGiaoHoSo));
    }
}

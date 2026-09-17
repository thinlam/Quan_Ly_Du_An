using FluentAssertions;
using QLDA.Application.Authorization;
using QLDA.Application.Dashboard.Queries;
using QLDA.Application.ThanhToans;
using QLDA.Application.ThanhToans.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;
using Xunit;

namespace QLDA.Tests.Unit;

public class DashboardGiaiNganTests {
    [Fact]
    public void ThanhToanInsertDto_ToEntity_ShouldMapNguonVonId() {
        // Arrange
        var dto = new ThanhToanInsertDto {
            DuAnId = Guid.NewGuid(),
            NghiemThuId = Guid.NewGuid(),
            NguonVonId = 23,
            GiaTri = 500_000_000,
            SoHoaDon = "HD-001"
        };

        // Act
        var entity = dto.ToEntity();

        // Assert
        entity.NguonVonId.Should().Be(23);
        entity.GiaTri.Should().Be(500_000_000);
        entity.SoHoaDon.Should().Be("HD-001");
    }

    [Fact]
    public void ThanhToanUpdateDto_ToEntity_ShouldMapNguonVonId() {
        // Arrange
        var dto = new ThanhToanUpdateDto {
            Id = Guid.NewGuid(),
            NghiemThuId = Guid.NewGuid(),
            NguonVonId = 25,
            GiaTri = 200_000_000,
            SoHoaDon = "HD-002"
        };

        // Act
        var entity = dto.ToEntity();

        // Assert
        entity.NguonVonId.Should().Be(25);
        entity.GiaTri.Should().Be(200_000_000);
    }

    [Fact]
    public void ThanhToan_ToDto_ShouldMapNguonVonIdAndTenNguonVon() {
        // Arrange
        var entity = new ThanhToan {
            Id = Guid.NewGuid(),
            DuAnId = Guid.NewGuid(),
            NghiemThuId = Guid.NewGuid(),
            NguonVonId = 23,
            NguonVon = new DanhMucNguonVon { Id = 23, Ten = "Vốn ngân sách" },
            GiaTri = 300_000_000
        };

        // Act
        var dto = entity.ToDto();

        // Assert
        dto.NguonVonId.Should().Be(23);
        dto.TenNguonVon.Should().Be("Vốn ngân sách");
        dto.GiaTri.Should().Be(300_000_000);
    }

    [Fact]
    public void ThanhToan_Update_ShouldUpdateNguonVonIdIfProvided() {
        // Arrange
        var entity = new ThanhToan {
            Id = Guid.NewGuid(),
            NguonVonId = 10,
            GiaTri = 100_000_000
        };

        var updateDto = new ThanhToanUpdateDto {
            Id = entity.Id,
            NghiemThuId = Guid.NewGuid(),
            NguonVonId = 20,
            GiaTri = 150_000_000
        };

        // Act
        entity.Update(updateDto);

        // Assert
        entity.NguonVonId.Should().Be(20);
        entity.GiaTri.Should().Be(150_000_000);
    }
}

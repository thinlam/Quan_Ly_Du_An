using System.ComponentModel;
using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.TepDinhKems;
using SequentialGuid;

namespace QLDA.WebApi.Models.CanBoTrienKhaiHangMucs;

public class CanBoTrienKhaiHangMucModel
{
    public long CanBoId { get; set; }
    public string TenCanBo { get; set; }
}
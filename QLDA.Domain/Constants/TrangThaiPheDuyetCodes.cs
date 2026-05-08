namespace QLDA.Domain.Constants;

/// <summary>
/// Mã trạng thái phê duyệt — Loai đồng nhất với PheDuyetEntityNames
/// </summary>
public static class TrangThaiPheDuyetCodes {
    public static class Loai {
        public const string DungChung = "DungChung";
        // Phải khớp PheDuyetEntityNames constants
        public const string PheDuyetDuToan = PheDuyetEntityNames.PheDuyetDuToan;
    }

    public static class DungChung {
        public const string DuThao = "DT";
        public const string DaTrinh = "ĐTr";
        public const string DaDuyet = "ĐD";
        public const string TraLai = "TL";
        public const string TuChoi = "TC";
        public const string Legacy = "LEG";
    }

    public static class DuToan {
        public const string DuThao = "DT";
        public const string DaTrinh = "ĐTr";
        public const string DaDuyet = "ĐD";
        public const string TraLai = "TL";
        public const string Legacy = "LEG";
    }
}

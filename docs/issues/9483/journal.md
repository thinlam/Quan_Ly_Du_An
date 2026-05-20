# Journal: Issue #9483 - Điều chỉnh QĐ phê duyệt dự toán/dự án

## Date: 2026-05-18

## Feature Summary
Bổ sung UC Điều chỉnh QĐ phê duyệt dự toán/dự án/kế hoạch thuê dịch vụ công nghệ thông tin theo yêu cầu riêng (các lần nếu có).

## Spec Analysis
1. **Actors**: CB.PCT, LĐ.PCT, GĐ/PGĐ, CB.PKH-TC, LD.PKH-TC, P.HC-TH, HĐTĐ
2. **Workflow**: PCT lập → Tư vấn thẩm tra → PKH-TC/HĐTĐ thẩm định → GĐ/PGĐ phê duyệt
3. **Loại điều chỉnh**: Mục tiêu/quy mô, TMĐT, Tiến độ, Chủ đầu tư, Tạm dừng, Nguồn vốn, Cơ cấu TMĐT
4. **UI Rule**: Cập nhật TMĐT chỉ hiển thị khi Loại điều chỉnh = "Điều chỉnh tổng mức đầu tư"
5. **Status Rule**: Chờ thẩm định/Chờ duyệt → không cho chỉnh sửa, xóa

## Existing Codebase
- `QuyetDinhDieuChinh` entities/commands/queries đã tồn tại (commit `f990bf8`)
- `ThongTinDieuChinhChiPhi` relationship 1-1 (commit `b60247a`)
- Status codes đã sync (commit `eba9a27`)

## Tables
- `QuyetDinhDieuChinhDuToan`: Số QĐ, ngày, trích yếu, nội dung chính, lần, lý do, file đính kèm
- `ThongTinDieuChinhDuToan`: TongMucDauTu, ChiPhiXayLap, ChiPhiThietBi, ChiPhiDuPhong, ChiPhiKhac

## Notes
- Entity name sử dụng trong code: `QuyetDinhDieuChinh` (khác tên spec `QuyetDinhDieuChinhDuToan`)
- Cần bổ sung Loại điều chỉnh field và TMĐT update UI logic
- Workflow states: Chờ thẩm định, Chờ duyệt, Đã duyệt, Trả lại
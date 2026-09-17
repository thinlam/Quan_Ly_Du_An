# Journal — fix-01 von-giai-ngan sai Giai đoạn hiện tại

## 08/09 — Trace + xác định root cause

- Trace endpoint `GET /api/du-an/von-giai-ngan` → `DuAnController` → `TongHopVonGiaiNganQuery` → projection.
- Xác định `tenGiaiDoanHienTai` được đọc từ cột denormalized `DuAn.GiaiDoanHienTaiId`.
- Kiểm tra DB (read-only) dự án "dự án NT": `BuocHienTai` 6912 → master `DmBuoc 435` → `GiaiDoanId 22` (kết thúc đầu tư) nhưng `DuAn.GiaiDoanHienTaiId = 19` (xin chủ trương).
- Root cause: `DuAnUpdatePhaseCommand` chỉ nâng phase khi `currentPhase.Stt < latestPhase.Stt`; các phase mới (id 15–22) đều `Stt = 0` ⇒ phase bị đóng băng. Mapping `DmBuoc → GiaiDoan` đúng.
- Mức độ: 71/157 dự án có bước hiện tại bị lệch.

**Quyết định:** fix code-only. Sửa writer `DuAnUpdatePhaseCommand` (đồng bộ phase theo bước hiện tại) + fix projection `TongHopVonGiaiNganQuery` (suy phase từ bước hiện tại, fill `GiaiDoanHienTaiId`). Không backfill DB, không migration.

**Docs:** tạo `docs/issues/fix-01/` (index / report / journal / test-workflow).

---

## 08/09 — Implement fix

- `DuAnUpdatePhaseCommand.cs`: bỏ guard so `DmGiaiDoan.Stt`; `GiaiDoanHienTaiId` giờ đồng bộ theo phase của **bước hiện tại** (`BuocHienTai.Buoc.GiaiDoanId`). Không đổi chữ ký command.
- `TongHopVonGiaiNganQuery.cs`: `TenGiaiDoanHienTai` + `GiaiDoanHienTaiId` lấy từ bước hiện tại (nguồn chuẩn), fallback cột denormalized.
- Build `QLDA.Application` pass (0 lỗi). Build WebApi chỉ lỗi file-lock do app đang chạy (PID 31620) — không phải lỗi compile.

**Kết quả mong đợi:** dự án "dự án NT" (bước 19) → `tenGiaiDoanHienTai = "Giai đoạn kết thúc đầu tư"`, `giaiDoanHienTaiId = 22`.

---

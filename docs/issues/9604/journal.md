# Journal — Issue #9604

## 13/05 — ThoiGianThucHienGoiThau: string → int

**Issue:** #9604 — Trường Thời gian thực hiện gói thầu cho nhập số

**Mục tiêu:**
- Đổi `ThoiGianThucHienGoiThau` từ `string?` → `int?`
- Giống format với màn hình Kết quả trúng thầu (`SoNgayTrienKhai`)

---

### Domain Layer

- `GoiThau.cs`:
  - Change `ThoiGianThucHienGoiThau` từ `string?` → `int?`

### Application Layer

- `GoiThauDto.cs`: Change type
- `GoiThauInsertDto.cs`: Change type
- `GoiThauUpdateDto.cs`: Change type

### FakeDataTool

- `GoiThauFaker.cs`:
  - Generate int thay vì formatted string

### Infrastructure

- `ef.sh`: Add Linux equivalent of ef.bat

---

**Kết quả:** Build 0 errors

**Files changed:** 7 files

**Commit:** `feat: change ThoiGianThucHienGoiThau from string to int`
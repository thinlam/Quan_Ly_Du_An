<!-- gitnexus:start -->
# GitNexus — Code Intelligence

This project is indexed by GitNexus as **Quan_Ly_Du_An** (20781 symbols, 42093 relationships, 300 execution flows). Use the GitNexus MCP tools to understand code, assess impact, and navigate safely.

> Index stale? Run `node .gitnexus/run.cjs analyze` from the project root — it auto-selects an available runner. No `.gitnexus/run.cjs` yet? `npx gitnexus analyze` (npm 11 crash → `npm i -g gitnexus`; #1939).

## Always Do

- **MUST run impact analysis before editing any symbol.** Before modifying a function, class, or method, run `impact({target: "symbolName", direction: "upstream"})` and report the blast radius (direct callers, affected processes, risk level) to the user.
- **MUST run `detect_changes()` before committing** to verify your changes only affect expected symbols and execution flows. For regression review, compare against the default branch: `detect_changes({scope: "compare", base_ref: "main"})`.
- **MUST warn the user** if impact analysis returns HIGH or CRITICAL risk before proceeding with edits.
- When exploring unfamiliar code, use `query({search_query: "concept"})` to find execution flows instead of grepping. It returns process-grouped results ranked by relevance.
- When you need full context on a specific symbol — callers, callees, which execution flows it participates in — use `context({name: "symbolName"})`.
- For security review, `explain({target: "fileOrSymbol"})` lists taint findings (source→sink flows; needs `analyze --pdg`).

## Never Do

- NEVER edit a function, class, or method without first running `impact` on it.
- NEVER ignore HIGH or CRITICAL risk warnings from impact analysis.
- NEVER rename symbols with find-and-replace — use `rename` which understands the call graph.
- NEVER commit changes without running `detect_changes()` to check affected scope.

## Resources

| Resource | Use for |
|----------|---------|
| `gitnexus://repo/Quan_Ly_Du_An/context` | Codebase overview, check index freshness |
| `gitnexus://repo/Quan_Ly_Du_An/clusters` | All functional areas |
| `gitnexus://repo/Quan_Ly_Du_An/processes` | All execution flows |
| `gitnexus://repo/Quan_Ly_Du_An/process/{name}` | Step-by-step execution trace |

## CLI

| Task | Read this skill file |
|------|---------------------|
| Understand architecture / "How does X work?" | `.claude/skills/gitnexus/gitnexus-exploring/SKILL.md` |
| Blast radius / "What breaks if I change X?" | `.claude/skills/gitnexus/gitnexus-impact-analysis/SKILL.md` |
| Trace bugs / "Why is X failing?" | `.claude/skills/gitnexus/gitnexus-debugging/SKILL.md` |
| Rename / extract / split / refactor | `.claude/skills/gitnexus/gitnexus-refactoring/SKILL.md` |
| Tools, resources, schema reference | `.claude/skills/gitnexus/gitnexus-guide/SKILL.md` |
| Index, status, clean, wiki CLI commands | `.claude/skills/gitnexus/gitnexus-cli/SKILL.md` |

<!-- gitnexus:end -->

## Rule bổ sung về Architecture Clean + CQRS

Dự án đang dùng **Clean Architecture + CQRS**, nên khi sinh code hoặc đề xuất cấu trúc code phải tuân thủ đúng boundary hiện tại.

### Application Layer

`QLDA.Application` chỉ nên chứa các nhóm chính sau:

* `Commands`
* `Queries`
* `Handlers`
* `Dtos` / `DTOs`
* `Validators`

**Không tạo thêm `Services` trong Application layer.**

Lý do: với CQRS, bản thân `CommandHandler` và `QueryHandler` đã đóng vai trò xử lý use case / application service rồi. Nếu tách thêm `Service` trong Application sẽ làm sai pattern, dễ sinh thêm tầng trung gian không cần thiết và làm lệch kiến trúc dự án.

### Khi cần xử lý nghiệp vụ

* Logic ghi dữ liệu đặt trong `Command` + `CommandHandler`.
* Logic đọc dữ liệu đặt trong `Query` + `QueryHandler`.
* Dữ liệu request/response đặt trong `Dto`.
* Validate input đặt trong `Validator`.
* Business model/entity đặt ở `Domain`.
* EF configuration/repository/db context đặt ở `Persistence`.
* Controller ở `WebApi` chỉ gọi command/query, không chứa business logic.

### Tuyệt đối tránh

* Không tạo folder/class kiểu `Application/Services`.
* Không tạo `SomethingService` để xử lý CRUD nếu có thể xử lý bằng `CommandHandler` / `QueryHandler`.
* Không đưa business logic vào Controller.
* Không tạo model trong WebApi nếu đã có DTO/Application pattern.
* Không sửa architecture theo kiểu MVC service layer truyền thống.

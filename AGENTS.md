<!-- gitnexus:start -->
# GitNexus — Code Intelligence

This project is indexed by GitNexus as **Quan_Ly_Du_An** (20640 symbols, 41796 relationships, 300 execution flows). Use the GitNexus MCP tools to understand code, assess impact, and navigate safely.

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



## Docs layout (`/docs`)

Keep documentation under two primary folders. Do **not** invent free-form folder names.

```text
docs/
├── issues/      # Real PMIS / Redmine work
├── usecases/    # Excel business use cases
└── *.md         # Shared project docs only (architecture, code standards, README, …)
```

### `/docs/issues` — PMIS / Redmine work

Put bugs, change requests, development tasks, issue analysis, acceptance criteria, and fix results here.

**Naming (mandatory):** folder = the issue number only.

| Correct | Incorrect |
|---------|-----------|
| `docs/issues/118/` | `docs/issues/118-theo-doi-du-an/` |
| `docs/issues/9459/` | `docs/issues/quan-ly-phe-duyet/` |

**Template:** mirror `docs/issues/9459/`:

| File | Purpose |
|------|---------|
| `index.md` | Issue / BA description, actors, UI notes, related issues |
| `report.md` | Implementation report (summary, architecture, status, PR) |
| `journal.md` | Work log by date (commits, decisions) |
| `test-workflow.md` | How to run tests, coverage, verification steps |
| `image*.png` | Screenshots referenced from `index.md` (optional) |

Do not add agent brainstorms, Superpowers plans, or temporary prompts under `issues/`.

### `/docs/usecases` — Excel business use cases

Put business flows synthesized from Excel use-case docs here: actors, preconditions, steps, expected results, business rules, DB/API design for the UC.

**Naming (mandatory):** folder + main file = `uc{N}` (lowercase `uc` + use-case number).

| Correct | Incorrect |
|---------|-----------|
| `docs/usecases/uc63/uc63.md` | `docs/usecases/nghiem-thu-hop-dong/` |
| `docs/usecases/uc89/uc89.md` | `docs/usecases/UC89-TaoLapHoSo/` |

**Template:** mirror `docs/usecases/uc63/uc63.md` sections:

1. Original content (name, UC id, actors, business description)
2. Business analysis (related issues, process overview, steps, permissions)
3. Database design
4. API mapping for frontend
5. Sample workflow (FE flow)
6. Change history

### Shared docs at `/docs` root

Truly shared references (e.g. `architecture.md`, `code-standards.md`, `README.md`) stay directly under `/docs`. Do **not** put them under `issues/` or `usecases/`. Do **not** use `/docs` for agent journals or temporary prompts.

### Never

* Free-form issue folder names (must be the Redmine/PMIS id).
* Free-form use-case folder names (must be `uc{N}`).
* Recreate deleted folders: `feature/`, `features/`, `journals/`, `superpowers/`, `archive/`, `misc/`.
* Store the same lasting content in both `issues/` and `usecases/` — link across folders instead.

---

## Clean Architecture + CQRS

This project uses **Clean Architecture + CQRS**. Generated or proposed code must respect the existing layer boundaries.

### Application layer

`QLDA.Application` should only contain:

* `Commands`
* `Queries`
* `Handlers`
* `Dtos` / `DTOs`
* `Validators`

**Do not add `Services` in the Application layer.**

With CQRS, `CommandHandler` and `QueryHandler` already act as the use-case / application-service layer. Extra Application `Service` classes add an unnecessary middle tier and break the project pattern.

### Where business logic belongs

* Writes → `Command` + `CommandHandler`
* Reads → `Query` + `QueryHandler`
* Request/response shapes → `Dto`
* Input validation → `Validator`
* Business models/entities → `Domain`
* EF configuration / repository / DbContext → `Persistence`
* Controllers (`WebApi`) only send commands/queries — no business logic

### Never

* Create `Application/Services` folders or classes
* Add a `SomethingService` for CRUD when a `CommandHandler` / `QueryHandler` can do it
* Put business logic in Controllers
* Add WebApi models when Application DTOs already exist
* Reshape the architecture into a traditional MVC service layer

---

## Code comments

Comment only to explain **why**, never to narrate **what** the code already says.

### Do comment when

* A non-obvious business rule, exception, or temporary workaround needs context
* Domain / authorization / mapping constraints are easy to misread without context
* You intentionally deviate from the usual pattern (e.g. skip `FilterVisible` because of X)
* A short TODO/FIXME includes a ticket or concrete reason — never a vague TODO

### Do not

* Line-by-line narration (`// get list`, `// assign value`, `// return result`)
* Restate a clear method/class/property name
* Write long architecture essays already covered in `AGENTS.md` / docs
* Leave stale comments after a refactor — delete or update them

### Examples

```csharp
// ✅ WHY — explains the decision
// FE omits Id → use GetId() so GroupId stays stable when syncing files
entity.Id = model.Id == Guid.Empty ? model.GetId() : model.Id;

// ❌ WHAT — redundant; the code is already clear
// Assign Id on the entity
entity.Id = model.Id;
```
<!-- gitnexus:end -->
---
title: "Unified Testing Workflow Complete"
date: 2026-04-24
type: implementation
---

# Unified Testing Workflow: Bogus + SQLite - Complete

## Summary

Implemented full testing infrastructure for QLDA project across 4 phases: SQLite in-memory fixtures, repository abstraction, entity coverage with handler tests, and DevSeeder CLI tool.

## What was built

**Phase 1 - Core Infrastructure:**
- TestAppDbContext: clears SQL Server-specific defaults for SQLite compat
- SharedSqliteFixture: in-memory SQLite shared across test collection
- IsolatedSqliteFixture: file-based SQLite for clean-slate tests
- EntityFakerBase<T>: Bogus base with deterministic seed 12345
- DuAnFaker: realistic Vietnamese project data
- CatalogSeeder: seeds DanhMucLoaiDuAn (DanhMucTrangThaiDuAn via HasData)

**Phase 2 - Repository Abstraction:**
- IDuAnRepository interface + DuAnRepository implementation
- Custom query methods: GetById, GetAll, GetByTrangThai, Search
- Soft delete via IsDeleted flag
- 6 repository tests all passing

**Phase 3 - Full Entity Coverage:**
- GoiThauFaker, HopDongFaker with fluent methods
- BusinessDataSeeder: linked DuAn→GoiThau→HopDong chain
- 3 GoiThau + 4 HopDong repository tests
- DuAnHandlerTests + HopDongHandlerTests via MediatR
- Code review fixes: DI scope violation (CreateScope), mixed pattern cleanup
- Total: 25 tests, all passing

**Phase 4 - DevSeeder CLI:**
- QLDA.DevSeeder console app (McMaster.Extensions.CommandLineUtils)
- SeedCommand: -c (count), -t (type: duan/goithau/hopdong/all), -o (SQLite file), --connection-string (SQL Server)
- ClearCommand: removes seeded data, keeps catalog
- DataGeneratorService: Bogus generation with proper 1:1 GoiThau→HopDong relationship
- SqliteAppDbContext: same SQL Server default clearing pattern
- Scripts: generate-test-data.ps1, run-tests.sh
- docs/testing-guide.md with patterns and troubleshooting

## Key Decisions

1. TestAppDbContext pattern over custom SQLite configurations - clears all SQL Server defaults in one pass
2. SharedSqliteFixture for speed (single DB), accepts data accumulation trade-off
3. Handlers tested via MediatR (they're internal) with proper DI scope
4. DevSeeder duplicates Bogus rules rather than referencing test project (no circular dependency)
5. ExecuteDeleteAsync for ClearAsync (avoids loading all entities into memory)

## Lessons Learned

- SQL Server → SQLite compat requires clearing: DefaultValueSql, nvarchar(max) column types, bracket-style index filters
- GoiThau→HopDong is 1:1, random FK assignment causes EF Core relationship severing - must shuffle and assign unique IDs
- Feature branch property changes (removed SoDuToan etc.) cascade to fakers and DevSeeder - need to sync

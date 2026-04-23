# Multi-Repository Management Rules

**Document Date**: April 23, 2026  
**Scope**: Rules for managing multiple repositories with shared features  
**Applies To**: Main repo (QLDA) and Twin repo

---

## 🎯 Rule 1: Dual Repository Synchronization & Divergence Policy

### 1.1 Repository Relationship
```
Main Repository (QLDA)           Twin Repository (e.g., QLHD)
├── Primary features             ├── Shared features (copied)
├── Core business logic          ├── Domain-specific enhancements
├── Master reference             ├── Customizations
└── Release source               └── Parallel maintenance
```

### 1.2 Feature Synchronization Levels

#### Level A: Shared Infrastructure (MUST sync)
**Must always stay synchronized**:
- BuildingBlocks (base classes, interfaces)
- Common DTOs and validators
- UnitOfWork and Repository patterns
- Authentication & Authorization framework
- Audit & soft delete mechanisms

**Sync Rule**:
- Any changes to BuildingBlocks in main repo MUST be merged to twin repo within 1 week
- Document all breaking changes in `BREAKING_CHANGES.md`
- Notify twin repo team immediately

#### Level B: Core Patterns (SHOULD sync)
**Should stay synchronized with careful review**:
- CQRS pattern implementation
- SyncHelper utility
- Exception handling patterns
- Mapping patterns
- Generic CRUD implementations

**Sync Rule**:
- Review changes quarterly
- Evaluate applicability before merging
- Document any intentional divergences

#### Level C: Domain Models (CAN diverge)
**Can have significant differences**:
- Entity relationships
- Business rules
- Validation logic
- Workflow states
- Reporting requirements

**Divergence Rule**:
- Document domain differences in `DOMAIN_COMPARISON.md`
- Both repos maintain separate feature inventories
- Coordinate only for shared utilities

---

## 📋 Rule 2: Feature Implementation Documentation

### 2.1 Documentation Location
Every new feature MUST have documentation in these locations:

```
When implementing in Main Repo (QLDA):
├── docs/feature/id/FEATURE_IMPLEMENTATION_INVENTORY.md (update)
├── docs/feature/[feature-name]/implementation-guide.md (new)
├── BuildingBlocks/CLAUDE.md (update if using BuildingBlocks)
└── Feature-specific folder in docs/feature/

When feature is ready for Twin Repo:
├── docs/TWIN_REPO_IMPLEMENTATION_GUIDE.md (update with details)
├── Create: docs/TWIN_MIGRATION_[FeatureName].md (new)
└── Reference QLDA implementation paths exactly
```

### 2.2 Mandatory Documentation Sections

**For each feature to be migrated, document**:

```markdown
## Feature: [Feature Name]

### Overview
- Feature purpose and scope
- Business rules
- Key entities and relationships

### Implementation Status in Main Repo
- Status: COMPLETE/IN-PROGRESS/PLANNED
- Effort: X hours
- Files: List all relevant files with paths
- Dependencies: Other features required

### For Twin Repo Implementation
- Estimated effort: X hours
- Required from main repo: (list files to copy/adapt)
- Differences to note: (any customizations)
- Testing requirements: (specific test scenarios)
- Migration checklist: (step-by-step)
```

### 2.3 Document Naming Convention

```
For Main Repo Features:
- FEATURE_IMPLEMENTATION_INVENTORY.md (master list)
- docs/feature/[module-name]/implementation-details.md

For Twin Repo Guidance:
- docs/TWIN_REPO_IMPLEMENTATION_GUIDE.md (master guide)
- docs/TWIN_MIGRATION_[FeatureName].md (specific features)
- docs/TWIN_[FeatureName]_DIFFERENCES.md (deviations if any)
```

### 2.4 Update Triggers

**Update feature documentation IMMEDIATELY when**:
- [ ] Feature implementation completes
- [ ] Major architecture change made
- [ ] New patterns introduced
- [ ] Breaking changes implemented
- [ ] Ready for twin repo migration

---

## 🔄 Rule 3: Feature Migration Workflow

### 3.1 Pre-Migration Checklist (Main Repo)
When a feature is ready to move to twin repo:

- [ ] Feature is FULLY TESTED in main repo
- [ ] All unit tests pass (80%+ coverage)
- [ ] All integration tests pass
- [ ] Code review approved
- [ ] Documentation complete
- [ ] No known bugs or TODOs
- [ ] Performance tested

### 3.2 Documentation Package for Twin Repo
Main repo prepares:

```
[FeatureName]_MIGRATION_PACKAGE
├── IMPLEMENTATION_GUIDE.md
│   ├── Step-by-step instructions
│   ├── Code snippets to copy
│   └── Files to reference
├── SOURCE_FILES_MANIFEST.md
│   ├── Exact file paths in main repo
│   └── Lines/sections to copy (if partial)
├── CONFIGURATION_CHECKLIST.md
│   ├── EF Core configurations needed
│   ├── DI registrations
│   └── Migration steps
└── TESTING_GUIDE.md
    ├── Unit tests to adapt
    ├── Integration test scenarios
    └── API test cases
```

### 3.3 Migration Validation Checklist (Twin Repo)
Before marking feature as complete:

- [ ] All code copied/adapted correctly
- [ ] EF migrations created and tested
- [ ] All unit tests pass
- [ ] All integration tests pass
- [ ] Controller endpoints respond correctly
- [ ] Database schema matches expected
- [ ] No compilation errors
- [ ] Performance meets expectations

---

## 📝 Rule 4: Documentation Content Standards

### 4.1 Implementation Guide Template

**Every feature migration guide MUST include**:

```markdown
# Implementation Guide: [Feature Name]

## Quick Facts
- Effort: X hours
- Complexity: Simple | Medium | Complex
- Dependencies: [list]
- Files to implement: [count]

## Architecture Overview
[Diagram or architecture description]

## Entities to Create
- Entity1: [purpose]
- Entity2: [purpose]

## Implementation Steps
1. [Step 1]
2. [Step 2]
...

## Code Reference Locations
| Component | Path | Notes |
|-----------|------|-------|
| Entity | QLDA.Domain/Entities/[Name].cs | Copy all properties |
| Config | QLDA.Persistence/Configurations/[Name]Configuration.cs | Adapt to twin schema |
| DTOs | QLDA.Application/[Module]/DTOs/ | Adapt naming if needed |
| Commands | QLDA.Application/[Module]/Commands/ | Copy handlers with modifications |
| Queries | QLDA.Application/[Module]/Queries/ | Copy query handlers |
| Controller | QLDA.WebApi/Controllers/[Name]Controller.cs | Copy endpoints |

## Critical Implementation Notes
- ⚠️ [Critical point 1]
- ⚠️ [Critical point 2]

## Testing Checklist
- [ ] Unit tests for validators
- [ ] Unit tests for command handlers
- [ ] Integration tests
- [ ] API endpoint tests

## Known Issues & Workarounds
- Issue: [description] → Workaround: [solution]
```

### 4.2 File Reference Format

**When referencing source files, ALWAYS use**:

```
File: QLDA.Application/DuAns/Commands/DuAnUpdateCommand.cs
- Lines 1-50: Basic structure
- Lines 51-120: SyncKeHoachVonsAsync implementation
- Key method: SyncKeHoachVonsAsync() (line 122)
```

### 4.3 Code Snippet Format

**When providing code examples**:

```csharp
// Copy this method from main repo
// File: [source path]
// Context: [what this does]

[code here]

// Changes needed for twin repo:
// 1. [change 1]
// 2. [change 2]
```

---

## ✅ Rule 5: Maintenance & Updates

### 5.1 Version Alignment
Main repo and twin repo track versions:

```
Main Repo Version: 2.5.0
Twin Repo Version: 2.5.0 (for shared code)
Twin Repo Customizations: +1.2.0 (domain-specific)
```

### 5.2 Update Notification
When main repo makes significant changes:

- [ ] Document in `CHANGELOG.md`
- [ ] Identify impact on twin repo
- [ ] Create issue in twin repo if action needed
- [ ] Notify twin repo team within 48 hours

### 5.3 Documentation Debt
Quarterly review of:
- [ ] All migration guides accuracy
- [ ] File paths still valid
- [ ] Code examples still working
- [ ] Dependencies still correct

---

## 📊 Rule 6: Documentation Maintenance Schedule

### Weekly
- Update feature status in FEATURE_IMPLEMENTATION_INVENTORY.md
- Add new implementation notes to migration guides

### Monthly
- Review and update all TWIN_MIGRATION_*.md files
- Verify file paths are still correct
- Check code examples compile

### Quarterly
- Full documentation audit
- Performance note updates
- Architecture review notes

### Annually
- Major documentation restructuring if needed
- Effort estimate validation (update if patterns change)
- Best practices update

---

## 🚀 Rule 7: Feature Readiness Criteria

### Feature is READY for Twin Repo when:

**Code Quality**:
- ✅ No compiler warnings
- ✅ Code review approved
- ✅ All tests pass
- ✅ 80%+ test coverage
- ✅ Performance benchmarked

**Documentation**:
- ✅ TWIN_MIGRATION_[Feature].md complete
- ✅ All file paths verified
- ✅ Code examples tested
- ✅ Dependencies clearly listed
- ✅ Effort estimate accurate

**Testing in Main Repo**:
- ✅ Unit tests (all passing)
- ✅ Integration tests (all passing)
- ✅ API tests (all passing)
- ✅ Production deployment tested (if applicable)

**Sign-off**:
- ✅ Main repo team approved
- ✅ Twin repo team acknowledged
- ✅ Documentation reviewed by both teams

---

## 🔗 Cross-Repo Communication

### Issue Tracker Format
When creating issues that affect both repos:

```
Title: [SHARED] Feature Name - Issue Description

Labels: shared, feature-x, needs-twin-repo-action

Body:
## Affected Repos
- [ ] Main Repo (QLDA)
- [ ] Twin Repo (QLHD)

## Impact
[Description]

## Action Required for Twin Repo
[What twin repo needs to do]

## Timeline
[When it needs to be done]
```

---

## 📚 Summary: The Three-Document System

```
For Developers Managing Multiple Repos:

1. FEATURE_IMPLEMENTATION_INVENTORY.md
   ├── What we built in main repo
   ├── Status of each feature
   └── Quick reference for what exists

2. TWIN_REPO_IMPLEMENTATION_GUIDE.md
   ├── Master guide for twin repo
   ├── High-level roadmap
   ├── All features overview
   └── Implementation patterns

3. TWIN_MIGRATION_[FeatureName].md (per feature)
   ├── Detailed step-by-step guide
   ├── Exact file references
   ├── Code snippets
   └── Testing checklist
```

---

## 📞 Escalation & Questions

**Questions about feature documentation**:
- Check FEATURE_IMPLEMENTATION_INVENTORY.md first
- Then check specific TWIN_MIGRATION_*.md

**Questions about twin repo adaptation**:
- Check TWIN_REPO_IMPLEMENTATION_GUIDE.md
- Contact main repo team for clarification

**Updates needed**:
- Document in PR description
- Update relevant guides
- Notify twin repo team


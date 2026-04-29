# Unified Testing Workflow Planning: Bogus + SQLite Implementation

**Date**: 2026-04-24 14:06
**Severity**: Medium
**Component**: Testing Infrastructure
**Status**: Planned

## What Happened

Brainstorming session for unified testing workflow using Bogus for fake data generation and SQLite for testing instead of direct SQL Server connections. Explored three approaches and settled on Approach 1: Hybrid SQLite + Repository Pattern with full entity graph coverage.

## The Brutal Truth

The existing testing setup was brittle and slow due to direct SQL Server dependencies. The decision to use SQLite for testing feels risky but necessary for developer productivity. The disconnect between SQLite and SQL Server behaviors keeps me awake at night.

## Technical Details

Created detailed design document at C:\Users\tranp\.claude\plans\t-i-mu-n-x-y-d-ng-unified-bee.md and implementation plan at C:\Users\tranp\source\repos\Quan_Ly_Du_An\plans\260424-1340-unified-testing-workflow\. Selected Approach 1 based on Microsoft documentation recommending Repository pattern for test isolation.

## What We Tried

Explored three approaches: Hybrid SQLite + Repository (selected), DbContext Injection, and Testcontainers. Each had tradeoffs between complexity, performance, and fidelity to production.

## Root Cause Analysis

The original testing approach created tight coupling to SQL Server infrastructure, making tests slow and unreliable for developers. Needed a solution that balances speed and isolation while maintaining code quality.

## Lessons Learned

Repository pattern provides the right abstraction layer for test isolation. SQLite offers significant performance benefits for development workflow, but introduces behavioral differences from SQL Server that require mitigation.

## Next Steps

Implement 4-phase plan: Phase 1 - Set up Bogus fake data generators, Phase 2 - Configure SQLite testing infrastructure, Phase 3 - Implement Repository pattern with full entity coverage, Phase 4 - Migrate existing tests. Phase 5 will address SQL Server fidelity via Testcontainers.
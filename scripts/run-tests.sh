#!/usr/bin/env bash
# run-tests.sh - Run QLDA test suite
set -euo pipefail

echo "=== QLDA Test Suite ==="

# Run unit tests
echo ""
echo "Running tests..."
dotnet test QLDA.Tests/QLDA.Tests.csproj --verbosity normal

echo ""
echo "=== All tests completed ==="

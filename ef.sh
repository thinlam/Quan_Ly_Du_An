#!/bin/bash
# ============================================
# ef.sh - Unified EF Core Migration Tool (QLDA)
# ============================================
#
# Usage: ./ef.sh <Module> <Command> [MigrationName] [Options]
#   Module: QLDA (required)
#   Command: add, remove, update, list (required)
#   MigrationName: Required for 'add' command
#   Options:
#     --sqlite   Use SQLite provider (update only)
#     --force    Force remove without checking
#
# Examples:
#   ./ef.sh QLDA add AddUserTable          Add migration (SQL Server)
#   ./ef.sh QLDA remove                    Remove last migration
#   ./ef.sh QLDA remove 0                  Remove ALL migrations
#   ./ef.sh QLDA update                    Update SQL Server via dotnet ef
#   ./ef.sh QLDA update --sqlite           Create SQLite DB via Migrator
#   ./ef.sh QLDA list                      List all migrations
# ============================================

MODULE="$1"
COMMAND="$2"
MIGRATION_NAME="$3"
PROVIDER="sqlserver"
FORCE_FLAG=""

# === Parse options ===
for arg in "$@"; do
    case "$arg" in
        --sqlite)   PROVIDER="sqlite" ;;
        --sqlserver) PROVIDER="sqlserver" ;;
        --force)    FORCE_FLAG="--force" ;;
    esac
done

# === Clear MIGRATION_NAME if it's a flag ===
case "$MIGRATION_NAME" in
    --sqlite|--sqlserver|--force) MIGRATION_NAME="" ;;
esac

# === Set ASPNETCORE_ENVIRONMENT ===
if [ -z "$ASPNETCORE_ENVIRONMENT" ]; then
    export ASPNETCORE_ENVIRONMENT=Development
fi

# === Set dotnet root (ensure dotnet-ef can find .NET runtime) ===
export DOTNET_ROOT=/home/juju/.dotnet
export PATH="$PATH:$DOTNET_ROOT"

# === Validate Module ===
if [ -z "$MODULE" ]; then
    echo ""
    echo "ERROR: Module parameter is required"
    echo ""
    echo "Usage: $0 <Module> <Command> [MigrationName] [Options]"
    echo "  Module: QLDA"
    echo "  Command: add, remove, update, list"
    echo ""
    exit 1
fi

# === Set Module Paths ===
case "$MODULE" in
    QLDA|qlda)
        MODULE="QLDA"
        MIGRATOR_PATH="QLDA.Migrator/QLDA.Migrator.csproj"
        PERSISTENCE_PATH="QLDA.Persistence/QLDA.Persistence.csproj"
        ;;
    *)
        echo ""
        echo "ERROR: Invalid module \"$MODULE\""
        echo "Valid modules: QLDA"
        echo ""
        exit 1
        ;;
esac

# === Validate Command ===
if [ -z "$COMMAND" ]; then
    echo ""
    echo "ERROR: Command parameter is required"
    echo ""
    echo "Usage: $0 $MODULE <Command> [MigrationName] [Options]"
    echo "  Command: add, remove, update, list"
    echo ""
    exit 1
fi

# === Display Info ===
echo ""
echo "============================================"
echo "  $MODULE EF CORE MIGRATION"
echo "============================================"
echo "  Command: $COMMAND"
if [ -n "$MIGRATION_NAME" ]; then
    echo "  Migration: $MIGRATION_NAME"
fi
if [ "$COMMAND" = "update" ]; then
    echo "  Provider: $PROVIDER"
fi
echo "============================================"
echo ""

# ============================================
#  Functions (defined before use)
# ============================================

add_migration() {
    if [ -z "$MIGRATION_NAME" ]; then
        echo "ERROR: Migration name required for 'add' command"
        echo "Usage: $0 $MODULE add <MigrationName>"
        exit 1
    fi
    echo "Adding migration: $MIGRATION_NAME"
    dotnet ef migrations add "$MIGRATION_NAME" --project "$MIGRATOR_PATH" --startup-project "$MIGRATOR_PATH" --context AppDbContext
    result=$?
    end_script $result
}

remove_migration() {
    if [ "$MIGRATION_NAME" = "0" ] || [ "$FORCE_FLAG" = "--force" ]; then
        echo "Removing ALL migrations..."
        echo ""
        REMOVE_COUNT=0

        while dotnet ef migrations remove --project "$MIGRATOR_PATH" --startup-project "$MIGRATOR_PATH" --context AppDbContext --force 2>/dev/null; do
            REMOVE_COUNT=$((REMOVE_COUNT + 1))
            echo "Removed migration #$REMOVE_COUNT"
        done

        echo ""
        echo "============================================"
        echo "  Removed $REMOVE_COUNT migration(s)"
        echo "============================================"
        if [ $REMOVE_COUNT -gt 0 ]; then
            exit 0
        fi
    else
        echo "Removing last migration..."
        dotnet ef migrations remove --project "$MIGRATOR_PATH" --startup-project "$MIGRATOR_PATH" --context AppDbContext
        result=$?
        end_script $result
    fi
}

update_database() {
    if [ "$PROVIDER" = "sqlite" ]; then
        echo "Creating SQLite database via $MODULE.Migrator..."
        dotnet run --project "$MIGRATOR_PATH" -- --provider sqlite
        result=$?
        end_script $result
    else
        echo "Updating SQL Server database via dotnet ef..."
        if [ -z "$MIGRATION_NAME" ]; then
            dotnet ef database update --project "$MIGRATOR_PATH" --startup-project "$MIGRATOR_PATH" --context AppDbContext
        else
            dotnet ef database update "$MIGRATION_NAME" --project "$MIGRATOR_PATH" --startup-project "$MIGRATOR_PATH" --context AppDbContext
        fi
        result=$?
        end_script $result
    fi
}

list_migrations() {
    echo "Listing migrations..."
    dotnet ef migrations list --project "$MIGRATOR_PATH" --startup-project "$MIGRATOR_PATH" --context AppDbContext
    result=$?
    end_script $result
}

end_script() {
    result=$1
    echo ""
    if [ $result -eq 0 ]; then
        echo "============================================"
        echo "  OPERATION COMPLETED SUCCESSFULLY"
        echo "============================================"
    else
        echo "============================================"
        echo "  OPERATION FAILED"
        echo "============================================"
        echo "  Error code: $result"
    fi
    exit $result
}

# === Execute Command ===
case "$COMMAND" in
    add)       add_migration ;;
    remove)    remove_migration ;;
    update)    update_database ;;
    list)      list_migrations ;;
    *)
        echo ""
        echo "ERROR: Invalid command \"$COMMAND\""
        echo "Valid commands: add, remove, update, list"
        echo ""
        exit 1
        ;;
esac
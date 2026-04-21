$connectionString = "Server=192.168.1.13\sql2k16r2, 1439;Initial Catalog=VI_DACDT;user id=vietinfo;password=Vietinfo@#@!;TrustServerCertificate=True;"

$sqlScript = @"
-- Check if DuToanBanDauId column already exists as bigint
IF EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'DuAn' 
    AND COLUMN_NAME = 'DuToanBanDauId' 
    AND DATA_TYPE = 'bigint'
)
BEGIN
    PRINT 'DuToanBanDauId is already bigint - migration already applied'
END
ELSE
BEGIN
    BEGIN TRANSACTION;
    
    -- Step 1: Drop the foreign key constraint if exists
    IF EXISTS (
        SELECT 1 
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
        WHERE CONSTRAINT_NAME = 'FK_DuAn_DuToan_DuToanBanDauId1' 
        AND TABLE_NAME = 'DuAn'
    )
    BEGIN
        ALTER TABLE [DuAn] DROP CONSTRAINT [FK_DuAn_DuToan_DuToanBanDauId1];
        PRINT 'Dropped FK_DuAn_DuToan_DuToanBanDauId1'
    END
    
    -- Also drop new FK if it exists (from previous attempts)
    IF EXISTS (
        SELECT 1 
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
        WHERE CONSTRAINT_NAME = 'FK_DuAn_DuToan_DuToanBanDauId' 
        AND TABLE_NAME = 'DuAn'
    )
    BEGIN
        ALTER TABLE [DuAn] DROP CONSTRAINT [FK_DuAn_DuToan_DuToanBanDauId];
        PRINT 'Dropped FK_DuAn_DuToan_DuToanBanDauId'
    END
    
    -- Step 2: Drop the index if exists
    IF EXISTS (
        SELECT 1 
        FROM sys.indexes 
        WHERE name = 'IX_DuAn_DuToanBanDauId1' 
        AND object_id = OBJECT_ID('DuAn')
    )
    BEGIN
        DROP INDEX [IX_DuAn_DuToanBanDauId1] ON [DuAn];
        PRINT 'Dropped index IX_DuAn_DuToanBanDauId1'
    END
    
    -- Step 3: Drop the old column if exists
    IF EXISTS (
        SELECT 1 
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'DuAn' 
        AND COLUMN_NAME = 'DuToanBanDauId1'
    )
    BEGIN
        ALTER TABLE [DuAn] DROP COLUMN [DuToanBanDauId1];
        PRINT 'Dropped column DuToanBanDauId1'
    END
    
    -- Step 4: Rename old DuToanBanDauId to temp if it exists as uniqueidentifier
    IF EXISTS (
        SELECT 1 
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'DuAn' 
        AND COLUMN_NAME = 'DuToanBanDauId' 
        AND DATA_TYPE = 'uniqueidentifier'
    )
    BEGIN
        EXEC sp_rename 'DuAn.DuToanBanDauId', 'DuToanBanDauId_old', 'COLUMN';
        PRINT 'Renamed DuToanBanDauId to DuToanBanDauId_old'
    END
    
    -- Step 5: Add new DuToanBanDauId column as bigint
    IF NOT EXISTS (
        SELECT 1 
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'DuAn' 
        AND COLUMN_NAME = 'DuToanBanDauId'
    )
    BEGIN
        ALTER TABLE [DuAn] ADD [DuToanBanDauId] bigint NULL;
        PRINT 'Added new DuToanBanDauId as bigint'
    END
    
    -- Step 6: Add SoDuToanBanDau column if it doesn't exist
    IF NOT EXISTS (
        SELECT 1 
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'DuAn' 
        AND COLUMN_NAME = 'SoDuToanBanDau'
    )
    BEGIN
        ALTER TABLE [DuAn] ADD [SoDuToanBanDau] bigint NULL;
        PRINT 'Added SoDuToanBanDau column'
    END
    
    -- Step 7: Add SoTienDuToanBanDau column if it doesn't exist
    IF NOT EXISTS (
        SELECT 1 
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'DuAn' 
        AND COLUMN_NAME = 'SoTienDuToanBanDau'
    )
    BEGIN
        ALTER TABLE [DuAn] ADD [SoTienDuToanBanDau] decimal(18, 2) NULL;
        PRINT 'Added SoTienDuToanBanDau column'
    END
    
    -- Step 8: Create index on DuToanBanDauId if it doesn't exist
    IF NOT EXISTS (
        SELECT 1 
        FROM sys.indexes 
        WHERE name = 'IX_DuAn_DuToanBanDauId' 
        AND object_id = OBJECT_ID('DuAn')
    )
    BEGIN
        CREATE INDEX [IX_DuAn_DuToanBanDauId] ON [DuAn]([DuToanBanDauId]);
        PRINT 'Created index IX_DuAn_DuToanBanDauId'
    END
    
    -- Step 9: Add the foreign key constraint
    -- Note: DuToanBanDauId points to SoDuToan which is not a PK, so we won't create FK
    -- DuToanBanDauId will just store the number value without FK constraint
    PRINT 'Note: DuToanBanDauId stores SoDuToan value without FK constraint (SoDuToan is not a PK)'
    
    -- Step 10: Insert the migration record if not exists
    IF NOT EXISTS (
        SELECT 1 
        FROM [__EFMigrationsHistory] 
        WHERE MigrationId = '20260420014500_ChangeDuToanBanDauIdToBigint'
    )
    BEGIN
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES ('20260420014500_ChangeDuToanBanDauIdToBigint', '8.0.15');
        PRINT 'Inserted migration record'
    END
    
    -- Step 11: Drop the old column if it still exists (and its dependencies)
    IF EXISTS (
        SELECT 1 
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'DuAn' 
        AND COLUMN_NAME = 'DuToanBanDauId_old'
    )
    BEGIN
        -- Drop index on old column if exists
        IF EXISTS (
            SELECT 1 
            FROM sys.indexes 
            WHERE name = 'IX_DuAn_DuToanBanDauId' 
            AND object_id = OBJECT_ID('DuAn')
        )
        BEGIN
            DROP INDEX [IX_DuAn_DuToanBanDauId] ON [DuAn];
            PRINT 'Dropped index IX_DuAn_DuToanBanDauId (from old column)'
        END
        
        -- Now drop the old column
        ALTER TABLE [DuAn] DROP COLUMN [DuToanBanDauId_old];
        PRINT 'Dropped DuToanBanDauId_old'
        
        -- Recreate index on new column if it doesn't exist
        IF NOT EXISTS (
            SELECT 1 
            FROM sys.indexes 
            WHERE name = 'IX_DuAn_DuToanBanDauId' 
            AND object_id = OBJECT_ID('DuAn')
        )
        BEGIN
            CREATE INDEX [IX_DuAn_DuToanBanDauId] ON [DuAn]([DuToanBanDauId]);
            PRINT 'Recreated index IX_DuAn_DuToanBanDauId on new column'
        END
    END
    
    COMMIT TRANSACTION;
    PRINT 'Migration completed successfully'
END
"@

try {
    $sqlConnection = New-Object System.Data.SqlClient.SqlConnection $connectionString
    $sqlConnection.Open()
    
    $sqlCommand = $sqlConnection.CreateCommand()
    $sqlCommand.CommandText = $sqlScript
    $sqlCommand.CommandTimeout = 300
    $sqlCommand.ExecuteNonQuery()
    
    Write-Host "Database migration completed successfully!"
    $sqlConnection.Close()
} catch {
    Write-Error "Error during migration: $_"
}

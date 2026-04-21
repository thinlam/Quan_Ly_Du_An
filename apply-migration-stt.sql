-- ============================================================================
-- Migration: AddSttColumnToDmBuocManHinh (VERSION WITHOUT GO)
-- ============================================================================
-- Use this version if you're NOT using SQL Server Management Studio (SSMS)
-- or sqlcmd. Compatible with Azure Data Studio, DBeaver, etc.
-- ============================================================================

USE [VI_DACDT];

PRINT '========================================='
PRINT 'Starting Migration: AddSttColumnToDmBuocManHinh'
PRINT 'DateTime: ' + CONVERT(VARCHAR(30), GETDATE(), 120)
PRINT '=========================================';

-- Verify table exists
DECLARE @TableExists INT = 0;
SELECT @TableExists = 1 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA = 'dbo' 
AND TABLE_NAME = 'DmBuocManHinh';

IF @TableExists = 0
BEGIN
    PRINT 'ERROR: Table DmBuocManHinh does not exist!';
    RETURN;
END
ELSE
BEGIN
    PRINT 'Table DmBuocManHinh found ✓';
END

-- Check if column already exists
DECLARE @ColumnExists INT = 0;
SELECT @ColumnExists = 1 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'dbo'
AND TABLE_NAME = 'DmBuocManHinh' 
AND COLUMN_NAME = 'Stt';

IF @ColumnExists = 0
BEGIN
    PRINT 'Column Stt does not exist, adding now...';
    
    ALTER TABLE [dbo].[DmBuocManHinh]
    ADD [Stt] INT NOT NULL DEFAULT 0;
    
    PRINT 'Column Stt added successfully ✓';
END
ELSE
BEGIN
    PRINT 'Column Stt already exists - skipping add ✓';
END

-- Verify column was added/exists
DECLARE @ColumnExistsNow INT = 0;
SELECT @ColumnExistsNow = 1 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'dbo'
AND TABLE_NAME = 'DmBuocManHinh' 
AND COLUMN_NAME = 'Stt';

IF @ColumnExistsNow = 0
BEGIN
    PRINT 'ERROR: Stt column verification failed!';
    RETURN;
END
ELSE
BEGIN
    PRINT 'Verification: Stt column exists ✓';
END

-- Create index for performance
DECLARE @IndexExists INT = 0;
SELECT @IndexExists = 1 
FROM sys.indexes 
WHERE name = 'IX_DmBuocManHinh_BuocId_Stt'
AND object_id = OBJECT_ID('[dbo].[DmBuocManHinh]');

IF @IndexExists = 0
BEGIN
    PRINT 'Creating index IX_DmBuocManHinh_BuocId_Stt...';
    
    CREATE NONCLUSTERED INDEX [IX_DmBuocManHinh_BuocId_Stt] 
    ON [dbo].[DmBuocManHinh] ([BuocId] ASC, [Stt] ASC);
    
    PRINT 'Index created successfully ✓';
END
ELSE
BEGIN
    PRINT 'Index IX_DmBuocManHinh_BuocId_Stt already exists - skipping ✓';
END

-- Log applied migrations
DECLARE @MigrationTableExists INT = 0;
SELECT @MigrationTableExists = 1 
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'dbo'
AND TABLE_NAME = '__EFMigrationsHistory';

IF @MigrationTableExists = 1
BEGIN
    DECLARE @MigrationRecordExists INT = 0;
    SELECT @MigrationRecordExists = 1 
    FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260420000000_AddSttColumnToDmBuocManHinh';
    
    IF @MigrationRecordExists = 0
    BEGIN
        PRINT 'Recording migration in __EFMigrationsHistory...';
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES (N'20260420000000_AddSttColumnToDmBuocManHinh', N'8.0.15');
        PRINT 'Migration recorded successfully ✓';
    END
    ELSE
    BEGIN
        PRINT 'Migration already recorded in history - skipping ✓';
    END
END
ELSE
BEGIN
    PRINT 'WARNING: __EFMigrationsHistory table not found - migration history not recorded';
END

-- Display summary
PRINT '';
PRINT '=========================================';
PRINT 'Migration Summary:';
PRINT '=========================================';
PRINT '';
PRINT 'DmBuocManHinh Table Structure:';

SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo'
AND TABLE_NAME = 'DmBuocManHinh'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT 'Index Information:';

SELECT 
    INDEX_NAME,
    COLUMN_NAME,
    KEY_ORDINAL
FROM INFORMATION_SCHEMA.STATISTICS
WHERE TABLE_SCHEMA = 'dbo'
AND TABLE_NAME = 'DmBuocManHinh'
AND INDEX_NAME = 'IX_DmBuocManHinh_BuocId_Stt'
ORDER BY KEY_ORDINAL;

PRINT '';
PRINT 'Total records in DmBuocManHinh:';

SELECT COUNT(*) AS [Record Count] FROM [dbo].[DmBuocManHinh];

PRINT '';
PRINT '=========================================';
PRINT 'Migration completed successfully! ✓';
PRINT 'DateTime: ' + CONVERT(VARCHAR(30), GETDATE(), 120);
PRINT '=========================================';

-- Script to change DuToanBanDauId from uniqueidentifier to bigint
-- This script performs the migration manually

BEGIN TRANSACTION;

-- Step 1: Drop the foreign key constraint
ALTER TABLE [DuAn] DROP CONSTRAINT [FK_DuAn_DuToan_DuToanBanDauId1];

-- Step 2: Drop the index
DROP INDEX [IX_DuAn_DuToanBanDauId1] ON [DuAn];

-- Step 3: Drop the old columns
ALTER TABLE [DuAn] DROP COLUMN [DuToanBanDauId1];
ALTER TABLE [DuAn] DROP COLUMN [DuToanBanDauId];

-- Step 4: Add new DuToanBanDauId column as bigint
ALTER TABLE [DuAn] ADD [DuToanBanDauId] bigint NULL;

-- Step 5: Add SoDuToanBanDau and SoTienDuToanBanDau columns if they don't exist
ALTER TABLE [DuAn] ADD [SoDuToanBanDau] bigint NULL;
ALTER TABLE [DuAn] ADD [SoTienDuToanBanDau] decimal(18, 2) NULL;

-- Step 6: Create index on DuToanBanDauId
CREATE INDEX [IX_DuAn_DuToanBanDauId] ON [DuAn]([DuToanBanDauId]);

-- Step 7: Add the foreign key constraint (DuToanBanDauId -> DuToan.SoDuToan)
ALTER TABLE [DuAn] ADD CONSTRAINT [FK_DuAn_DuToan_DuToanBanDauId] 
FOREIGN KEY ([DuToanBanDauId]) REFERENCES [DuToan]([SoDuToan]);

-- Step 8: Insert the migration record into __EFMigrationsHistory
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES ('20260420014500_ChangeDuToanBanDauIdToBigint', '8.0.15');

COMMIT TRANSACTION;

-- Check if SoNgayThucHienHopDong column exists, if not add it
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'KetQuaTrungThau' 
    AND COLUMN_NAME = 'SoNgayThucHienHopDong'
)
BEGIN
    ALTER TABLE [dbo].[KetQuaTrungThau] 
    ADD [SoNgayThucHienHopDong] [bigint] NULL;
    PRINT 'Column SoNgayThucHienHopDong added successfully';
END
ELSE
BEGIN
    PRINT 'Column SoNgayThucHienHopDong already exists';
END

-- Display table structure
EXEC sp_columns @table_name = 'KetQuaTrungThau';

-- Insert sample record
DECLARE @DuAnId UNIQUEIDENTIFIER = NEWID();
DECLARE @GoiThauId UNIQUEIDENTIFIER = NEWID();

INSERT INTO [dbo].[KetQuaTrungThau] 
    ([DuAnId], [GoiThauId], [GiaTriTrungThau], [SoNgayThucHienHopDong])
VALUES 
    (@DuAnId, @GoiThauId, 5000000, 180);

PRINT 'Sample record inserted successfully';
PRINT 'DuAnId: ' + CAST(@DuAnId AS VARCHAR(36));
PRINT 'GoiThauId: ' + CAST(@GoiThauId AS VARCHAR(36));

-- Select the inserted record to verify
SELECT * FROM [dbo].[KetQuaTrungThau] 
WHERE [DuAnId] = @DuAnId 
    AND [GoiThauId] = @GoiThauId;

PRINT 'Verification completed';

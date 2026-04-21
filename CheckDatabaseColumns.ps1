$connectionString = "Server=192.168.1.13\sql2k16r2, 1439;Initial Catalog=VI_DACDT;user id=vietinfo;password=Vietinfo@#@!;TrustServerCertificate=True;"

$sqlScript = @"
-- Check if columns exist in DuAn table
SELECT 
    COLUMN_NAME, 
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'DuAn' 
AND COLUMN_NAME IN ('SoDuToanBanDau', 'SoTienDuToanBanDau', 'DuToanBanDauId')
ORDER BY COLUMN_NAME;

-- Check if NghiemThu table exists and has GiaTri column
PRINT ''
PRINT 'NghiemThu table structure:'
SELECT 
    COLUMN_NAME, 
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'NghiemThu'
ORDER BY COLUMN_NAME;
"@

try {
    $sqlConnection = New-Object System.Data.SqlClient.SqlConnection $connectionString
    $sqlConnection.Open()
    
    $sqlCommand = $sqlConnection.CreateCommand()
    $sqlCommand.CommandText = $sqlScript
    $sqlCommand.CommandTimeout = 300
    
    $result = $sqlCommand.ExecuteReader()
    
    Write-Host "=== DuAn table columns ===" -ForegroundColor Cyan
    while ($result.Read()) {
        Write-Host "$($result['COLUMN_NAME']) - $($result['DATA_TYPE']) - Nullable: $($result['IS_NULLABLE'])"
    }
    
    $result.NextResult() | Out-Null
    $result.NextResult() | Out-Null
    
    Write-Host "`n=== NghiemThu table columns ===" -ForegroundColor Cyan
    while ($result.Read()) {
        Write-Host "$($result['COLUMN_NAME']) - $($result['DATA_TYPE']) - Nullable: $($result['IS_NULLABLE'])"
    }
    
    $result.Close()
    $sqlConnection.Close()
    
} catch {
    Write-Error "Error: $_"
}

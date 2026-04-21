$connectionString = "Server=192.168.1.13\sql2k16r2, 1439;Initial Catalog=VI_DACDT;user id=vietinfo;password=Vietinfo@#@!;TrustServerCertificate=True;"

$sqlScript = @"
-- Check if NghiemThu table exists
IF OBJECT_ID('NghiemThu', 'U') IS NOT NULL
BEGIN
    PRINT 'NghiemThu table exists'
    SELECT 
        COLUMN_NAME, 
        DATA_TYPE,
        IS_NULLABLE
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'NghiemThu'
    ORDER BY COLUMN_NAME;
END
ELSE
BEGIN
    PRINT 'NghiemThu table does NOT exist'
    PRINT 'Checking for similar tables...'
    SELECT TABLE_NAME 
    FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_NAME LIKE '%NghiemThu%' OR TABLE_NAME LIKE '%ThanhToan%'
END

-- Check a few sample rows
IF OBJECT_ID('NghiemThu', 'U') IS NOT NULL
BEGIN
    PRINT ''
    PRINT 'Sample data from NghiemThu:'
    SELECT TOP 5 * FROM NghiemThu
END
"@

try {
    $sqlConnection = New-Object System.Data.SqlClient.SqlConnection $connectionString
    $sqlConnection.Open()
    
    $sqlCommand = $sqlConnection.CreateCommand()
    $sqlCommand.CommandText = $sqlScript
    $sqlCommand.CommandTimeout = 300
    
    $result = $sqlCommand.ExecuteReader()
    
    while ($result.Read()) {
        for ($i = 0; $i -lt $result.FieldCount; $i++) {
            Write-Host "$($result.GetName($i)): $($result[$i])"
        }
        Write-Host "---"
    }
    
    $result.Close()
    $sqlConnection.Close()
    
} catch {
    Write-Error "Error: $_"
}

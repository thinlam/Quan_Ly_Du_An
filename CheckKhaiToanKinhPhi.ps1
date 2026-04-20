$connectionString = "Server=192.168.1.13\sql2k16r2, 1439;Initial Catalog=VI_DACDT;user id=vietinfo;password=Vietinfo@#@!;TrustServerCertificate=True;"

$sqlScript = @"
-- Check for columns related to "Khái toán kinh phí"
SELECT 
    COLUMN_NAME, 
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'DuAn' 
AND (COLUMN_NAME LIKE '%KhaiToan%' 
     OR COLUMN_NAME LIKE '%KinhPhi%'
     OR COLUMN_NAME LIKE '%DuToan%')
ORDER BY COLUMN_NAME;

PRINT ''
PRINT 'All columns in DuAn table:'
SELECT 
    COLUMN_NAME, 
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'DuAn'
ORDER BY ORDINAL_POSITION;
"@

try {
    $sqlConnection = New-Object System.Data.SqlClient.SqlConnection $connectionString
    $sqlConnection.Open()
    
    $sqlCommand = $sqlConnection.CreateCommand()
    $sqlCommand.CommandText = $sqlScript
    $sqlCommand.CommandTimeout = 300
    
    $result = $sqlCommand.ExecuteReader()
    
    Write-Host "=== Cột liên quan tới Khái toán kinh phí ===" -ForegroundColor Cyan
    $hasData = $false
    while ($result.Read()) {
        Write-Host "$($result['COLUMN_NAME']) - $($result['DATA_TYPE']) - Nullable: $($result['IS_NULLABLE'])"
        $hasData = $true
    }
    
    if (-not $hasData) {
        Write-Host "❌ KHÔNG TÌM THẤY cột nào liên quan tới 'Khái toán kinh phí'" -ForegroundColor Red
    }
    
    $result.NextResult() | Out-Null
    
    Write-Host "`n=== Tất cả cột trong bảng DuAn ===" -ForegroundColor Cyan
    while ($result.Read()) {
        Write-Host "$($result['COLUMN_NAME']) - $($result['DATA_TYPE'])"
    }
    
    $result.Close()
    $sqlConnection.Close()
    
}
catch {
    Write-Error "Error: $($_)"
}

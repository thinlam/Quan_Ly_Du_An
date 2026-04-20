$connectionString = "Server=192.168.1.13\sql2k16r2, 1439;Initial Catalog=VI_DACDT;user id=vietinfo;password=Vietinfo@#@!;TrustServerCertificate=True;"

$sqlScript = @"
-- Check for columns related to Khhai toan kinh phi
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
"@

try {
    $sqlConnection = New-Object System.Data.SqlClient.SqlConnection $connectionString
    $sqlConnection.Open()
    
    $sqlCommand = $sqlConnection.CreateCommand()
    $sqlCommand.CommandText = $sqlScript
    $sqlCommand.CommandTimeout = 300
    
    $result = $sqlCommand.ExecuteReader()
    
    Write-Host "=== Col lien quan toi Khhai toan kinh phi ===" -ForegroundColor Cyan
    $hasData = $false
    while ($result.Read()) {
        Write-Host "$($result['COLUMN_NAME']) - $($result['DATA_TYPE']) - Nullable: $($result['IS_NULLABLE'])"
        $hasData = $true
    }
    
    if (-not $hasData) {
        Write-Host "KHONG TIM THAY col nao lien quan toi Khhai toan kinh phi" -ForegroundColor Red
    }
    
    $result.Close()
    $sqlConnection.Close()
}
catch {
    Write-Host "Error: $($_)"
}

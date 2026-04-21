$cs = "Server=192.168.1.13\sql2k16r2,1439;Database=VI_DACDT;User Id=sa;Password=123456;TrustServerCertificate=true;";
try {
    $conn = New-Object System.Data.SqlClient.SqlConnection($cs);
    $conn.Open();
    $cmd = $conn.CreateCommand();
    $cmd.CommandText = @"
SELECT 
    d.Id,
    d.TenDuAn,
    d.SoDuToanBanDau AS [Kinh phí ban đầu],
    d.KhaiToanKinhPhi AS [Khái toán kinh phí],
    ISNULL(SUM(nt.GiaTri), 0) AS [Tổng giá trị nghiệm thu]
FROM dbo.DuAn d
LEFT JOIN dbo.NghiemThu nt ON nt.DuAnId = d.Id AND nt.IsDeleted = 0
GROUP BY d.Id, d.TenDuAn, d.SoDuToanBanDau, d.KhaiToanKinhPhi
ORDER BY d.Id DESC
"@;
    $reader = $cmd.ExecuteReader();
    Write-Host "DuAn Report with KhaiToanKinhPhi:`n";
    while($reader.Read()) {
        Write-Host "ID: $($reader[0]) | Dự án: $($reader[1]) | Ban đầu: $($reader[2]) | Khái toán: $($reader[3]) | Nghiệm thu: $($reader[4])"
    };
    $reader.Close();
    $conn.Close();
}
catch {
    Write-Host "Error: $_"
}

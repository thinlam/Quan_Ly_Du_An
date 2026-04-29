@echo off
:: Fake Data Generator for QLDA — always inserts directly
:: Usage: fake.bat <entity> [count] [options]
:: Examples:
::   fake.bat da 10              - Insert 10 DuAn to SQLite
::   fake.bat gt 5               - Insert 5 GoiThau (auto-seed DuAn)
::   fake.bat hd 3               - Insert 3 HopDong (auto-seed DuAn+GoiThau)
::   fake.bat all 20             - Insert full chain (20 each)
::   fake.bat all 50 --schema dev - Insert to SQL Server dev schema
::   fake.bat clear              - Clear seeded data

dotnet run --project QLDA.FakeDataTool\FakeDataTool.csproj -- %*
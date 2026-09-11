$ErrorActionPreference = "Stop"
Set-Location (Split-Path $PSScriptRoot -Parent)
function Run-Dotnet {
 & dotnet @args
 if ($LASTEXITCODE -ne 0) { throw "dotnet failed. Xem lỗi phía trên." }
}
Run-Dotnet --version
Run-Dotnet tool restore
Run-Dotnet restore EnglishLearningPlatform.sln
Run-Dotnet build EnglishLearningPlatform.sln --no-restore
$migrations = Get-ChildItem "src/EnglishLearning.Infrastructure/Migrations" -Filter "*InitialCreate.cs" -ErrorAction SilentlyContinue
if (-not $migrations) {
 Run-Dotnet ef migrations add InitialCreate --project src/EnglishLearning.Infrastructure --startup-project src/EnglishLearning.Web --output-dir Migrations
}
Run-Dotnet ef database update --project src/EnglishLearning.Infrastructure --startup-project src/EnglishLearning.Web
Run-Dotnet run --project src/EnglishLearning.Web --no-launch-profile -- --seed
Run-Dotnet test EnglishLearningPlatform.sln --no-restore
Write-Host "Hoàn tất. Chạy: dotnet watch --project src/EnglishLearning.Web run"

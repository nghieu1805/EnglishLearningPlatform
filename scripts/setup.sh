#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet tool restore
dotnet restore EnglishLearningPlatform.sln
dotnet build EnglishLearningPlatform.sln --no-restore
if ! compgen -G "src/EnglishLearning.Infrastructure/Migrations/*InitialCreate.cs" > /dev/null; then
 dotnet ef migrations add InitialCreate --project src/EnglishLearning.Infrastructure --startup-project src/EnglishLearning.Web --output-dir Migrations
fi
dotnet ef database update --project src/EnglishLearning.Infrastructure --startup-project src/EnglishLearning.Web
dotnet run --project src/EnglishLearning.Web --no-launch-profile -- --seed
dotnet test EnglishLearningPlatform.sln --no-restore

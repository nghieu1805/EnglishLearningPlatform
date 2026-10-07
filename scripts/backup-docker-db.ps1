$ErrorActionPreference = "Stop"

$projectRoot =
    Resolve-Path(
        Join-Path $PSScriptRoot "..")

$backupDirectory =
    Join-Path $projectRoot "backups"

$timestamp =
    Get-Date -Format "yyyyMMdd-HHmmss"

$backupFile =
    Join-Path `
        $backupDirectory `
        "EnglishLearningDb-$timestamp.sql"

$containerBackup =
    "/tmp/eapp-database-backup.sql"

New-Item `
    -ItemType Directory `
    -Path $backupDirectory `
    -Force |
    Out-Null

Push-Location $projectRoot

try
{
    Write-Host "Checking Docker containers..."

    docker compose ps

    if ($LASTEXITCODE -ne 0)
    {
        throw "Docker Compose is not available."
    }

    Write-Host "Creating database backup..."

    docker compose exec -T mysql sh -c `
        'mysqldump -uroot -p"$MYSQL_ROOT_PASSWORD" --single-transaction --routines --triggers "$MYSQL_DATABASE" > /tmp/eapp-database-backup.sql'

    if ($LASTEXITCODE -ne 0)
    {
        throw "mysqldump failed."
    }

    docker cp `
        "eapp-mysql:$containerBackup" `
        "$backupFile"

    if ($LASTEXITCODE -ne 0)
    {
        throw "Could not copy backup from container."
    }

    $backupInfo =
        Get-Item $backupFile

    if ($backupInfo.Length -le 0)
    {
        throw "Backup file is empty."
    }

    Write-Host ""
    Write-Host "Backup completed successfully."
    Write-Host "File: $backupFile"
    Write-Host "Size: $($backupInfo.Length) bytes"
}
finally
{
    docker compose exec -T mysql `
        rm -f $containerBackup 2>$null

    Pop-Location
}
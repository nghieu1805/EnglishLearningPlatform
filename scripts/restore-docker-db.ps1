param(
    [Parameter(Mandatory = $true)]
    [string]$BackupFile,

    [switch]$ConfirmRestore
)

$ErrorActionPreference = "Stop"

if (-not $ConfirmRestore)
{
    throw `
        "Restore cancelled. Add -ConfirmRestore to confirm."
}

$projectRoot =
    Resolve-Path(
        Join-Path $PSScriptRoot "..")

$resolvedBackup =
    Resolve-Path $BackupFile

$backupInfo =
    Get-Item $resolvedBackup

if ($backupInfo.Length -le 0)
{
    throw "The selected backup file is empty."
}

$containerRestore =
    "/tmp/eapp-database-restore.sql"

Push-Location $projectRoot

try
{
    Write-Host "Stopping web container..."

    docker compose stop web

    if ($LASTEXITCODE -ne 0)
    {
        throw "Could not stop the web container."
    }

    Write-Host "Copying backup into MySQL container..."

    docker cp `
        "$($resolvedBackup.Path)" `
        "eapp-mysql:$containerRestore"

    if ($LASTEXITCODE -ne 0)
    {
        throw "Could not copy the backup file."
    }

    Write-Host "Restoring database..."

    docker compose exec -T mysql sh -c `
        'mysql -uroot -p"$MYSQL_ROOT_PASSWORD" "$MYSQL_DATABASE" < /tmp/eapp-database-restore.sql'

    if ($LASTEXITCODE -ne 0)
    {
        throw "Database restore failed."
    }

    Write-Host ""
    Write-Host "Database restored successfully."
    Write-Host "Source: $($resolvedBackup.Path)"
}
finally
{
    docker compose exec -T mysql `
        rm -f $containerRestore 2>$null

    Write-Host "Starting web container..."

    docker compose start web

    Pop-Location
}
[CmdletBinding()]
param(
    [switch]$Release
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$configuration = if ($Release) { "Release" } else { "Debug" }
$projectPath = Join-Path $repoRoot "src\AccessNote\AccessNote.csproj"
$exePath = Join-Path $repoRoot "src\AccessNote\bin\$configuration\net8.0-windows\AccessNote.exe"

if (Test-Path $exePath) {
    Start-Process -FilePath $exePath
    exit 0
}

Write-Host "AccessNote.exe not found at $exePath"
Write-Host "Running with dotnet instead..."
dotnet run --project $projectPath -c $configuration

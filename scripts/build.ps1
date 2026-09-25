# 개발용 빠른 빌드 (zip·main 배포 없음)
$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path $PSScriptRoot -Parent
$Project = Join-Path $RepoRoot "vArchiveHelper\vArchiveHelper.csproj"
dotnet build $Project -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
$exe = Join-Path $RepoRoot "vArchiveHelper\bin\Release\net472\vArchiveHelper.exe"
Write-Host "완료: $exe"

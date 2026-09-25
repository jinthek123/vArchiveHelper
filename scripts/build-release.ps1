# source 브랜치에서 실행 → releases-local zip + main 브랜치에 설치본 배포

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path $PSScriptRoot -Parent
$Project = Join-Path $RepoRoot "vArchiveHelper\vArchiveHelper.csproj"
$OutDir = Join-Path $RepoRoot "vArchiveHelper\bin\Release\net472"
$ExampleSettings = Join-Path $RepoRoot "vArchiveHelper\appsettings.example.json"
$ReleasesLocal = Join-Path $RepoRoot "releases-local"

[xml]$csproj = Get-Content $Project
$version = $csproj.Project.PropertyGroup.Version | Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($version)) {
    throw "vArchiveHelper.csproj 에 <Version> 이 없습니다."
}

Write-Host "빌드 중 — v$version"
dotnet build $Project -c Release --nologo -v q
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build 실패"
}

if (-not (Test-Path $OutDir)) {
    throw "출력 폴더 없음: $OutDir"
}

if (Test-Path $ExampleSettings) {
    Copy-Item $ExampleSettings (Join-Path $OutDir "appsettings.json") -Force
}

$publishFiles = Get-ChildItem $OutDir -File | Where-Object {
    $_.Extension -notin ".pdb", ".lnk"
}

New-Item -ItemType Directory -Force -Path $ReleasesLocal | Out-Null
$zipName = "vArchiveHelper.zip"
$zipPath = Join-Path $ReleasesLocal $zipName
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}
Compress-Archive -Path ($publishFiles.FullName) -DestinationPath $zipPath -Force
Write-Host "로컬 zip: $zipPath"
Write-Host "exe: $(Join-Path $OutDir 'vArchiveHelper.exe')"
Write-Host "GitHub에는 이 zip만 Releases에 올립니다. main 루트에는 실행 파일을 넣지 않습니다."

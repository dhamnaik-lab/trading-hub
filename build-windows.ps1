# Builds Windows installers (x64 and ARM64) on Windows: publishes the self-contained app and packages it with
# NSIS (winget install NSIS.NSIS). Usage: .\build-windows.ps1 [-Version 1.1.0]
# Outputs dist\TradingHub-Setup-x64.exe (Intel/AMD) and dist\TradingHub-Setup-arm64.exe (Windows on ARM).
param([string]$Version = "")
$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot
if (-not $Version) {
  $Version = ([xml](Get-Content Directory.Build.props)).Project.PropertyGroup | ForEach-Object { $_.Version } | Where-Object { $_ } | Select-Object -First 1
}
$makensis = Get-Command makensis -ErrorAction SilentlyContinue
$makensis = if ($makensis) { $makensis.Source } else { Join-Path ${env:ProgramFiles(x86)} "NSIS\makensis.exe" }
if (-not (Test-Path $makensis)) { throw "NSIS not found: install it with winget install NSIS.NSIS" }
New-Item -ItemType Directory -Force dist | Out-Null
foreach ($arch in "x64", "arm64") {
  $out = "dist\win-$arch"
  if (Test-Path $out) { Remove-Item -Recurse -Force $out }
  dotnet publish src\TradingHub.App\TradingHub.App.csproj -c Release -r "win-$arch" --self-contained true -o $out -v q "-p:Version=$Version"
  if ($LASTEXITCODE) { throw "dotnet publish failed ($arch)" }
  & $makensis -V2 "-DVERSION=$Version" "-DARCH=$arch" "-DSRC=$PWD\$out" "-DOUT=$PWD\dist\TradingHub-Setup-$arch.exe" installer\TradingHub.nsi
  if ($LASTEXITCODE) { throw "makensis failed ($arch)" }
  Write-Host "Built: dist\TradingHub-Setup-$arch.exe"
}

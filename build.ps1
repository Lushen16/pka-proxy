$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
$sourceRoot = Join-Path $PSScriptRoot 'src'
if (!(Test-Path -LiteralPath $sourceRoot)) { $sourceRoot = $PSScriptRoot }
$sources = Get-ChildItem -LiteralPath $sourceRoot -Filter '*.cs' | Where-Object { $_.Name -ne 'Setup.cs' -and $_.Name -ne 'Integrity.cs' } | ForEach-Object { $_.FullName }
& $compiler /nologo /target:winexe ('/win32icon:' + (Join-Path $PSScriptRoot 'PKAproxy.ico')) ('/out:' + (Join-Path $PSScriptRoot 'PKA-Proxy.exe')) /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll /reference:System.Security.dll $sources
if ($LASTEXITCODE -ne 0) { throw 'Falha na compilação.' }


Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'PKA-Proxy.exe') -Destination (Join-Path $PSScriptRoot 'PKAproxy.exe') -Force


$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
$sourceRoot = Join-Path $PSScriptRoot 'src'
if (!(Test-Path -LiteralPath $sourceRoot)) { $sourceRoot = $PSScriptRoot }
$sources = Get-ChildItem -LiteralPath $sourceRoot -Filter '*.cs' | ForEach-Object { $_.FullName }
& $compiler /nologo /target:winexe ('/out:' + (Join-Path $PSScriptRoot 'PKA-Proxy.exe')) /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll /reference:System.Security.dll $sources
if ($LASTEXITCODE -ne 0) { throw 'Falha na compilação.' }


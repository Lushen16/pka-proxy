$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
$sourceRoot = Join-Path $PSScriptRoot 'src'
if (!(Test-Path -LiteralPath $sourceRoot)) { $sourceRoot = $PSScriptRoot }
$sources = Get-ChildItem -LiteralPath $sourceRoot -Filter '*.cs' | Where-Object { $_.Name -ne 'Setup.cs' -and $_.Name -ne 'Integrity.cs' -and $_.Name -ne 'RouteProbe.cs' } | ForEach-Object { $_.FullName }
$probeSource = Join-Path $PSScriptRoot 'probe/RouteProbe.cs'
if (!(Test-Path -LiteralPath $probeSource)) { $probeSource = Join-Path $PSScriptRoot 'RouteProbe.cs' }
& $compiler /nologo /target:exe ('/out:' + (Join-Path $PSScriptRoot 'PKArouteProbe.exe')) $probeSource
if ($LASTEXITCODE -ne 0) { throw 'Falha ao compilar consulta de roteamento.' }
& $compiler /nologo /target:winexe ('/resource:' + (Join-Path $PSScriptRoot 'PKArouteProbe.exe') + ',PKArouteProbe.exe') ('/win32icon:' + (Join-Path $PSScriptRoot 'PKAproxy.ico')) ('/out:' + (Join-Path $PSScriptRoot 'PKA-Proxy.exe')) /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll /reference:System.Security.dll /reference:System.ServiceProcess.dll $sources
if ($LASTEXITCODE -ne 0) { throw 'Falha na compilação.' }


Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'PKA-Proxy.exe') -Destination (Join-Path $PSScriptRoot 'PKAproxy.exe') -Force




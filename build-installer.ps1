$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'build.ps1')
$installerRoot = Join-Path $PSScriptRoot 'installer'
if (!(Test-Path -LiteralPath $installerRoot)) { $installerRoot = $PSScriptRoot }
$payload = Join-Path $PSScriptRoot 'PKAproxy.exe'
$payloadHash = (Get-FileHash -LiteralPath $payload -Algorithm SHA256).Hash
$integrity = 'public static class InstallerIntegrity { public const string PayloadHash = "' + $payloadHash + '"; }'
$integrity | Set-Content -Encoding utf8 (Join-Path $installerRoot 'Integrity.cs')
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
& $compiler /nologo /target:winexe ('/win32icon:' + (Join-Path $PSScriptRoot 'PKAproxy.ico')) ('/out:' + (Join-Path $PSScriptRoot 'PKAproxy-Setup.exe')) ('/resource:' + $payload + ',PKAproxy.Payload') /reference:System.Windows.Forms.dll /reference:System.Drawing.dll (Join-Path $installerRoot 'Setup.cs') (Join-Path $installerRoot 'Integrity.cs')
if ($LASTEXITCODE -ne 0) { throw 'Falha ao compilar instalador.' }


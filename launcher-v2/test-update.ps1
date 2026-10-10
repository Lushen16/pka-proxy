param([string]$WorkDirectory=(Join-Path $env:TEMP ('PKA-updater-tests-'+[Guid]::NewGuid().ToString('N'))))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $WorkDirectory -Force | Out-Null
$WorkDirectory=(Resolve-Path -LiteralPath $WorkDirectory).Path
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$rsa=New-Object Security.Cryptography.RSACryptoServiceProvider(2048)
$rsa.PersistKeyInCsp=$false
try {
 $private=Join-Path $WorkDirectory 'fixture-key.xml'
 [IO.File]::WriteAllText($private,$rsa.ToXmlString($true))
 $trust=Join-Path $WorkDirectory 'FixtureTrust.cs'
 [IO.File]::WriteAllText($trust,('public static class UpdateTrust {public const string PublicKey=@"'+$rsa.ToXmlString($false)+'";}'))
}finally{$rsa.Dispose()}
$fixture=Join-Path $WorkDirectory 'NewFixture.cs'
@'
using System;using System.IO;
[assembly:System.Reflection.AssemblyVersion("2.0.2.0")]
class Fixture {static void Main(){File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"updated-ok"),"2.0.2.0");}}
'@ | Set-Content $fixture -Encoding UTF8
$newExe=Join-Path $WorkDirectory 'new-fixture.exe'
& $compiler /nologo /platform:x64 /target:exe ('/out:'+$newExe) $fixture
if($LASTEXITCODE -ne 0){throw 'Fixture compilation failed'}
$stage=Join-Path $WorkDirectory 'release'
& (Join-Path $PSScriptRoot 'prepare-release.ps1') -PrivateKeyPath $private -ExePath $newExe -ReleaseDirectory $stage
$parent=Join-Path $WorkDirectory 'PKA-Proxy.exe'
$source=Join-Path $PSScriptRoot 'src'
& $compiler /nologo /platform:x64 /target:exe ('/out:'+$parent) /reference:System.Web.Extensions.dll /reference:System.Windows.Forms.dll (Join-Path $PSScriptRoot 'tests\UpdaterTests.cs') (Join-Path $source 'UpdateService.cs') (Join-Path $source 'UpdateInstaller.cs') $trust
if($LASTEXITCODE -ne 0){throw 'Updater tests compilation failed'}
& $parent $stage
if($LASTEXITCODE -ne 0){throw 'Updater unit tests failed'}
$process=Start-Process -FilePath $parent -ArgumentList @('--parent',('"'+$stage+'"')) -WindowStyle Hidden -PassThru
if(!$process.WaitForExit(20000)){throw 'Parent did not exit'}
if($process.ExitCode -ne 0){throw 'Update handshake failed'}
$timer=[Diagnostics.Stopwatch]::StartNew()
while(!(Test-Path -LiteralPath (Join-Path $WorkDirectory 'updated-ok')) -and $timer.ElapsedMilliseconds -lt 15000){Start-Sleep -Milliseconds 100}
if(!(Test-Path -LiteralPath (Join-Path $WorkDirectory 'updated-ok'))){throw 'Updated executable did not restart'}
if([Reflection.AssemblyName]::GetAssemblyName($parent).Version.ToString() -ne '2.0.2.0'){throw 'Target version mismatch'}
if(Get-ChildItem -LiteralPath $WorkDirectory -Filter 'PKA-Proxy.exe.backup-*'){throw 'Previous launcher not removed after restart'}
Write-Output 'PASS: end-to-end update with helper readiness, parent exit, signed binary replacement, backup and automatic restart.'

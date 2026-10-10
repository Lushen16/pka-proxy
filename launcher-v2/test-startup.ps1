param([string]$WorkDirectory=(Join-Path $env:TEMP ('Litfix-startup-'+[Guid]::NewGuid().ToString('N'))),[string]$BaselineMainForm)
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $WorkDirectory -Force | Out-Null
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$sources=Get-ChildItem (Join-Path $PSScriptRoot 'src') -Filter '*.cs' | Where-Object {$_.Name -ne 'MainForm.cs'} | ForEach-Object {$_.FullName}
$sources+=if($BaselineMainForm){$BaselineMainForm}else{Join-Path $PSScriptRoot 'src\MainForm.cs'}
$exe=Join-Path $WorkDirectory 'startup-tests.exe'
& $compiler /nologo /platform:x64 /target:exe /main:StartupTests ('/out:'+$exe) /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll /reference:System.Security.dll /reference:System.ServiceProcess.dll (Join-Path $PSScriptRoot 'tests\StartupTests.cs') $sources
if($LASTEXITCODE -ne 0){throw 'Falha ao compilar o teste de abertura.'}
if($BaselineMainForm){& $exe (Join-Path $WorkDirectory 'profile') --expect-failure}else{& $exe (Join-Path $WorkDirectory 'profile')}
if($LASTEXITCODE -ne 0){throw 'Falha no teste de abertura.'}

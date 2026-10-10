param([Parameter(Mandatory=$true)][string]$CompilerPath,[switch]$LocalUnsigned)
$ErrorActionPreference='Stop'
if (!$LocalUnsigned) { & (Join-Path $PSScriptRoot 'Verificar-assinatura.ps1') }
if ($LocalUnsigned) { & $CompilerPath /DLocalUnsigned (Join-Path $PSScriptRoot 'installer\PKA.iss') }
else { & $CompilerPath (Join-Path $PSScriptRoot 'installer\PKA.iss') }
if($LASTEXITCODE -ne 0){throw 'Falha ao compilar instalador.'}

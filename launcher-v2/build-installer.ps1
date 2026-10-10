param([Parameter(Mandatory=$true)][string]$CompilerPath)
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Verificar-assinatura.ps1')
& $CompilerPath (Join-Path $PSScriptRoot 'installer\PKA.iss')
if($LASTEXITCODE -ne 0){throw 'Falha ao compilar instalador.'}

param(
 [Parameter(Mandatory=$true)][string]$PrivateKeyPath,
 [string]$ExePath = (Join-Path $PSScriptRoot 'PKA-Proxy.exe'),
 [string]$ReleaseDirectory = (Join-Path $PSScriptRoot 'release')
)
$ErrorActionPreference = 'Stop'
$resolvedExe = (Resolve-Path -LiteralPath $ExePath).Path
$releaseVersion = [System.Reflection.AssemblyName]::GetAssemblyName($resolvedExe).Version.ToString()
$manifest = [ordered]@{ version=$releaseVersion; file='PKA-Proxy.exe'; sha256=(Get-FileHash -LiteralPath $resolvedExe -Algorithm SHA256).Hash.ToLowerInvariant(); size=(Get-Item -LiteralPath $resolvedExe).Length }
$manifestBytes = [System.Text.Encoding]::UTF8.GetBytes(($manifest | ConvertTo-Json -Compress))
$rsa = New-Object System.Security.Cryptography.RSACryptoServiceProvider
$rsa.PersistKeyInCsp = $false
try {
 $rsa.FromXmlString([System.IO.File]::ReadAllText((Resolve-Path -LiteralPath $PrivateKeyPath).Path))
 $signature = $rsa.SignData($manifestBytes,[System.Security.Cryptography.CryptoConfig]::MapNameToOID('SHA256'))
 New-Item -ItemType Directory -Force -Path $ReleaseDirectory | Out-Null
 $releaseRoot = (Resolve-Path -LiteralPath $ReleaseDirectory).Path
 [System.IO.File]::WriteAllBytes((Join-Path $releaseRoot 'update.json'),$manifestBytes)
 [System.IO.File]::WriteAllBytes((Join-Path $releaseRoot 'update.sig'),$signature)
 if ($resolvedExe -ne (Join-Path $releaseRoot 'PKA-Proxy.exe')) { Copy-Item -LiteralPath $resolvedExe -Destination (Join-Path $releaseRoot 'PKA-Proxy.exe') -Force }
 Write-Output "Release pronta. Tag obrigatória: v$releaseVersion"
 Write-Output 'Publique somente PKA-Proxy.exe, update.json e update.sig. Nunca publique a chave privada.'
} finally { $rsa.Dispose() }

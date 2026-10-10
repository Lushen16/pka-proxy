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
 $keyFile=(Resolve-Path -LiteralPath $PrivateKeyPath).Path
 $keyBytes=[System.IO.File]::ReadAllBytes($keyFile)
 if([System.IO.Path]::GetExtension($keyFile) -eq '.dpapi'){
  $keyBytes=[System.Security.Cryptography.ProtectedData]::Unprotect($keyBytes,$null,[System.Security.Cryptography.DataProtectionScope]::CurrentUser)
 }
 try{$rsa.FromXmlString([System.Text.Encoding]::UTF8.GetString($keyBytes))}finally{[Array]::Clear($keyBytes,0,$keyBytes.Length)}
 $signature = $rsa.SignData($manifestBytes,[System.Security.Cryptography.CryptoConfig]::MapNameToOID('SHA256'))
 New-Item -ItemType Directory -Force -Path $ReleaseDirectory | Out-Null
 $releaseRoot = (Resolve-Path -LiteralPath $ReleaseDirectory).Path
 [System.IO.File]::WriteAllBytes((Join-Path $releaseRoot 'update.json'),$manifestBytes)
 [System.IO.File]::WriteAllBytes((Join-Path $releaseRoot 'update.sig'),$signature)
 $exeBytes=[System.IO.File]::ReadAllBytes($resolvedExe)
 [System.IO.File]::WriteAllBytes((Join-Path $releaseRoot 'PKA-Proxy.exe.sig'),$rsa.SignData($exeBytes,[System.Security.Cryptography.CryptoConfig]::MapNameToOID('SHA256')))
 $publicXml=$rsa.ToXmlString($false)
 [System.IO.File]::WriteAllText((Join-Path $releaseRoot 'PUBLIC-KEY.xml'),$publicXml,(New-Object System.Text.UTF8Encoding($false)))
 $sha=[System.Security.Cryptography.SHA256]::Create()
 try{$fingerprint=[BitConverter]::ToString($sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($publicXml))).Replace('-','')}
 finally{$sha.Dispose()}
 [System.IO.File]::WriteAllText((Join-Path $releaseRoot 'PUBLIC-KEY-SHA256.txt'),$fingerprint+[Environment]::NewLine)
 if ($resolvedExe -ne (Join-Path $releaseRoot 'PKA-Proxy.exe')) { Copy-Item -LiteralPath $resolvedExe -Destination (Join-Path $releaseRoot 'PKA-Proxy.exe') -Force }
 Write-Output "Release pronta. Tag obrigatória: v$releaseVersion"
 Write-Output 'Assinaturas RSA/SHA-256 criadas. Publique somente arquivos públicos da release. Nunca publique a chave privada.'
} finally { $rsa.Dispose() }

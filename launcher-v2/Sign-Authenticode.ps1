param(
 [Parameter(Mandatory=$true)][string]$CertificateThumbprint,
 [string]$ExePath=(Join-Path $PSScriptRoot 'PKA-Proxy.exe'),
 [Parameter(Mandatory=$true)][string]$TimestampServer
)
$ErrorActionPreference='Stop'
$thumbprint=$CertificateThumbprint.Replace(' ','')
if($thumbprint -notmatch '^[A-Fa-f0-9]{40}$'){throw 'Thumbprint inválido.'}
if(([uri]$TimestampServer).Scheme -notin @('http','https')){throw 'Use o servidor de timestamp fornecido pela certificadora.'}
$certificate=Get-Item -LiteralPath ('Cert:\CurrentUser\My\'+$thumbprint)
if(!$certificate.HasPrivateKey -or $certificate.NotAfter -lt [DateTime]::Now){throw 'Certificado expirado ou sem chave privada.'}
if($certificate.EnhancedKeyUsageList.ObjectId -notcontains '1.3.6.1.5.5.7.3.3'){throw 'O certificado não permite assinatura de código.'}
$result=Set-AuthenticodeSignature -LiteralPath (Resolve-Path -LiteralPath $ExePath).Path -Certificate $certificate -HashAlgorithm SHA256 -TimestampServer $TimestampServer
if($result.Status -ne 'Valid'){throw ('Assinatura não reconhecida como válida pelo Windows: '+$result.Status)}
Write-Output 'Authenticode validado. Agora gere novamente as assinaturas da release com prepare-release.ps1.'

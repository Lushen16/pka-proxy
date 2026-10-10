param(
 [Parameter(Mandatory=$true)][string]$PrivateKeyPath,
 [Parameter(Mandatory=$true)][string]$ProtectedKeyPath
)
$ErrorActionPreference='Stop'
$source=(Resolve-Path -LiteralPath $PrivateKeyPath).Path
$destination=[IO.Path]::GetFullPath($ProtectedKeyPath)
if($source -eq $destination -or (Test-Path -LiteralPath $destination)){throw 'Escolha um arquivo novo para a chave protegida.'}
if([IO.Path]::GetExtension($destination) -ne '.dpapi'){throw 'A chave protegida deve terminar em .dpapi.'}
$rsa=New-Object Security.Cryptography.RSACryptoServiceProvider
$rsa.PersistKeyInCsp=$false
$bytes=[IO.File]::ReadAllBytes($source)
try {
 $rsa.FromXmlString([Text.Encoding]::UTF8.GetString($bytes))
 if($rsa.PublicOnly){throw 'O arquivo precisa conter a chave privada de assinatura.'}
 $protected=[Security.Cryptography.ProtectedData]::Protect($bytes,$null,[Security.Cryptography.DataProtectionScope]::CurrentUser)
 $check=[Security.Cryptography.ProtectedData]::Unprotect($protected,$null,[Security.Cryptography.DataProtectionScope]::CurrentUser)
 try{if([Convert]::ToBase64String($check) -ne [Convert]::ToBase64String($bytes)){throw 'Verificação da chave protegida falhou.'}}
 finally{[Array]::Clear($check,0,$check.Length)}
 [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($destination)) | Out-Null
 [IO.File]::WriteAllBytes($destination,$protected)
 Write-Output 'Chave privada protegida por DPAPI CurrentUser. A chave original não foi removida.'
 Write-Output 'Mantenha um backup privado seguro: esta cópia depende da conta Windows atual.'
}finally{[Array]::Clear($bytes,0,$bytes.Length);$rsa.Dispose()}

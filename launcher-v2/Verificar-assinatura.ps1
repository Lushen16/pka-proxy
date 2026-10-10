param([string]$ExePath=(Join-Path $PSScriptRoot 'PKA-Proxy.exe'),[string]$SignaturePath)
$ErrorActionPreference='Stop'
if(!$SignaturePath){$SignaturePath=$ExePath+'.sig'}
# Esta chave é fixa; nunca aceitar uma chave substituta enviada junto de um arquivo desconhecido.
$publicKey='<RSAKeyValue><Modulus>vss7WWSiJqAdG6UZ1y0msPA5aeEsYVI+t0x6llQB8MnNo4SArXYGsbtkZJKdWPFJZQkdqGO6JsJXGQJmeXzTZ9eFJSXEn26ZUd64pqmWcGh1t5Km1p9jNkHQFnPXGxDaD9294UCEf6HMhraE1TMOIRL8u6FmEt69Etb3MnXlM7J2aBkeDrhfR/nPDuBBS9sxD4GXVbwDo3BcqBIHF8c7ssdf9IZ7K4yq4qyIP6yukSxOWLWDCur+totXKwrn+uJh40G5VaSaiYfM+Y0VXGT8xpLJyN1kwhodA2DqrUR5z0o6K9a33oDu9SyAONA6d5BLTio8fdbagqE0vxHnngBAPfCyAPL4tdqxoZTKm8/Zb5gBDcTzOR2QOeyXNN65NdamZrx7kgtVht4NYBpQSkqohvYfqdGymFkQjZTgD0bexWN7Ow3mwhynnNsrm4n0qazJb9j1FdaDYi/XsV5Up6kdPCrXXq9O1C3crmRL3xmayL+fsDqKd08FYJ7a+qKtdnot</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>'
$rsa=New-Object Security.Cryptography.RSACryptoServiceProvider
$rsa.PersistKeyInCsp=$false
try{
 $rsa.FromXmlString($publicKey)
 $signature=[IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $SignaturePath).Path)
 if($signature.Length -gt 1024){throw 'Assinatura inválida.'}
 $stream=[IO.File]::OpenRead((Resolve-Path -LiteralPath $ExePath).Path)
 $sha=[Security.Cryptography.SHA256]::Create()
 try{$hash=$sha.ComputeHash($stream);$valid=$rsa.VerifyHash($hash,[Security.Cryptography.CryptoConfig]::MapNameToOID('SHA256'),$signature)}finally{$stream.Dispose();$sha.Dispose()}
 if(!$valid){throw 'ARQUIVO ADULTERADO OU ASSINATURA DE OUTRA CHAVE. Não execute este arquivo.'}
 Write-Output 'ASSINATURA VÁLIDA: o arquivo é idêntico ao assinado pela chave oficial do PKA.'
 Write-Output 'Esta verificação RSA é separada do certificado Authenticode reconhecido pelo Windows.'
}finally{$rsa.Dispose()}

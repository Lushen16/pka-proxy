# Autenticidade e criptografia do PKA

## O que está protegido

- Executável: assinatura destacada RSA de 3072 bits com SHA-256 em PKA-Proxy.exe.sig.
- Atualizações: manifesto update.json assinado em update.sig, contendo versão, tamanho, nome e SHA-256 do binário. O updater usa uma chave pública fixa e rejeita alterações.
- Chave privada de publicação: nova cópia protegida por DPAPI CurrentUser, fora do pacote e do repositório. Não está incorporada no aplicativo.
- Credenciais, perfil e pedidos ao motor: DPAPI CurrentUser, já implementado. A configuração ativa em texto do sing-box fica na pasta administrativa com ACL restrita; isso não mudou.

A assinatura identifica arquivos produzidos por quem possui a chave privada do projeto. Não é Authenticode reconhecido pelo Windows, não garante ausência de bugs e não impede alguém de copiar a aparência, recompilar o código público ou distribuir outro arquivo com o mesmo nome. Uma cópia modificada não consegue manter uma assinatura válida para esta chave sem acesso à chave privada. Sempre use um verificador obtido de uma origem confiável.

Não usamos um empacotador que prometa criptografar o executável durante sua execução, nem escondemos código de terceiros. O repositório e as fontes continuam públicos. Essa criptografia não torna o programa indecifrável; protege os segredos e permite conferir autenticidade.

## Conferir um download

Baixe PKA-Proxy.exe, PKA-Proxy.exe.sig e Verificar-assinatura.ps1 da Release oficial no repositório Lushen16/LIT-fix. Coloque os três na mesma pasta e execute:

```powershell
.\Verificar-assinatura.ps1
```

O script tem a chave pública fixa do PKA e retorna erro se a assinatura não corresponder ao arquivo. Não executa o programa, não instala certificados e não altera a rede. Não aceite um verificador de um distribuidor desconhecido: ele pode ter trocado a chave de verificação.

PUBLIC-KEY.xml contém apenas a chave pública. O SHA-256 de seu XML normalizado UTF-8, sem BOM, é:

`269624AD62E9DE6D348971A50E36FF22887DFF6DE45F576ACBE0187B3C593FDA`

Este fingerprint identifica os bytes da chave pública do projeto; não é um thumbprint de certificado X.509. Confira-o por uma origem independente e confiável quando possível.

## Proteger a chave privada existente

```powershell
.\Protect-SigningKey.ps1 -PrivateKeyPath 'CAMINHO-PRIVADO\chave.xml' -ProtectedKeyPath 'CAMINHO-PRIVADO\chave.dpapi'
.\prepare-release.ps1 -PrivateKeyPath 'CAMINHO-PRIVADO\chave.dpapi'
```

A proteção usa a mesma conta Windows. Não apaga o original: confirme a assinatura e guarde um backup privado antes de decidir o que fazer com o arquivo XML antigo. A cópia original no local anterior continua em texto e precisa de custódia privada. DPAPI não protege contra um processo já executando como você, administrador ou comprometimento da conta Windows. Não publique .dpapi, XML privado, PFX, senhas ou tokens.

prepare-release.ps1 aceita a chave em XML ou DPAPI, gera as assinaturas do manifesto e do executável e exporta somente a chave pública/fingerprint. Arquivos privados são excluídos pelo .gitignore. A chave usada pelo updater foi preservada, portanto a confiança das atualizações anteriores não foi trocada.

## Assinatura reconhecida pelo Windows

Não existe certificado Authenticode de assinatura de código disponível nesta entrega. O Windows ainda pode mostrar editor desconhecido. Certificados de teste não foram instalados como raiz confiável para disfarçar essa limitação.

Quando houver um certificado válido, use Sign-Authenticode.ps1 com seu thumbprint e servidor de timestamp. Assine antes de rodar prepare-release.ps1, pois Authenticode modifica o arquivo e exige novas assinaturas RSA e hashes. O script recusa certificado expirado, sem chave privada, sem uso de assinatura de código ou resultado não reconhecido como válido pelo Windows.

[Opções oficiais de assinatura Windows](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/code-signing-options).

## Validação desta entrega

- Executável oficial aceito pelo verificador público.
- Cópia com um byte adulterado rejeitada.
- Cópia DPAPI da chave criada, recuperada e usada para produzir uma assinatura válida da release real.
- Manifesto real verificado com a chave pública fixa do aplicativo.
- 13 verificações do updater e teste completo de troca, backup e reinício aprovados após a alteração do empacotamento.

As validações de rede/WFP em VM continuam pendentes; assinatura criptográfica não substitui esses testes.

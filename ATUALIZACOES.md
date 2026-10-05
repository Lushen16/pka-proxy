# Atualizador do PKA Proxy — versão 1.1.0.0

O aplicativo agora possui o botão Atualizações. Informe o repositório público GitHub, marque Verificar novas versões ao abrir e clique em Salvar e verificar. Uma versão mais recente mostra o botão Atualizar agora: baixa o EXE, valida assinatura RSA/SHA-256 e hash/tamanho/versão, inicia um auxiliar, fecha o app, substitui somente o EXE com backup e reabre. Uma falha na verificação não instala o arquivo. Uma falha ao abrir a nova versão tenta restaurar o backup; não há teste de saúde do aplicativo após iniciar.

Sem repositório e arquivos publicados, a consulta não pode funcionar. Repositório oficial: https://github.com/Lushen16/pka-proxy. Os arquivos de release são publicados separadamente do código-fonte. Não é necessário colocar token GitHub no aplicativo. A distribuição já vem configurada para Lushen16/pka-proxy com verificação ao abrir habilitada; pode ser desativada em Atualizações.

## Primeira publicação

1. Depois do login, crie um repositório público chamado pka-proxy com um README inicial.
2. Crie uma Release com a tag exata v1.1.0.0 (quatro números). Não marque como draft ou pre-release e defina como Latest.
3. Anexe SOMENTE os três arquivos da pasta release: PKA-Proxy.exe, update.json e update.sig. Podem ser anexados também o ZIP público do aplicativo e o código público, sem a chave privada.
4. Publique a Release. Abra PKA-Proxy.exe > Atualizações, coloque https://github.com/SEU_USUARIO/pka-proxy e clique em Salvar e verificar.

A assinatura específica desta distribuição é verificada por uma chave pública embutida em src/UpdateTrust.cs. Alterar o repositório não permite instalar pacotes sem essa assinatura. O hash sozinho não é usado como prova de autenticidade. O EXE continua sem assinatura Authenticode reconhecida pelo Windows: a assinatura de atualização é própria do aplicativo.

## Próximas versões

Altere AssemblyVersion e AssemblyFileVersion em src/AssemblyInfo.cs, por exemplo para 1.1.1.0, compile com build.ps1 e execute:

```powershell
.\prepare-release.ps1 -PrivateKeyPath 'CAMINHO_PRIVADO\CHAVE-PRIVADA-NAO-PUBLICAR.xml'
```

Publique os três arquivos da pasta release com a tag indicada pelo script, por exemplo v1.1.1.0. Não mude os nomes dos arquivos. O cliente compara versões numericamente, rejeita versões antigas/iguais e baixa o executável pelo tag específico. A assinatura é calculada sobre os bytes exatos de update.json; editar o JSON depois de assinar invalida a atualização.

A chave privada está em um arquivo separado dos ZIPs públicos. Guarde-a em uma pasta privada, com backup, antes de publicar qualquer fonte ou release. Ela permite assinar versões aceitas pelos usuários: NÃO envie ao GitHub, nem coloque dentro do pacote distribuído. Se a perder, será necessário distribuir manualmente um cliente com nova chave confiável.

## Preservação

O auxiliar substitui apenas PKA-Proxy.exe e mantém app-config.json e demais arquivos do usuário. Campos e modo da janela são guardados no perfil local Windows, em LocalAppData\PkaProxy\session.json. A senha dessa sessão usa proteção DPAPI vinculada ao usuário Windows. O JSON exportado para ProxiFyre continua com a senha em texto claro. A preferência de atualizações fica no mesmo perfil, em updates.json. O app precisa estar em uma pasta gravável pelo usuário; não solicita elevação automaticamente. A configuração do motor e seus drivers não são atualizados por este mecanismo.

O modo global exclui este configurador para que a consulta direta do Check continue fora da regra. Não mova o EXE depois de gerar a regra sem atualizá-la. As limitações de TCP/UDP/IPv6/DNS e processos filhos continuam descritas em ALTERACOES.md.

## Arquivos novos

- UpdateService.cs: validação de repositório, consulta/download HTTPS, assinatura, manifesto e integridade.
- UpdateInstaller.cs: auxiliar, validação antes da substituição, backup e reinício.
- UpdateDialog.cs: configuração do repositório, consulta e instalação com um clique.
- UpdatePreferences (em UpdateService.cs): guarda opção de consulta ao abrir.
- SessionStore.cs: preserva campos do formulário com senha protegida pelo Windows.
- UpdateTrust.cs: chave pública embutida; não contém chave privada.
- AssemblyInfo.cs: versão do aplicativo.
- prepare-release.ps1: gera o manifesto e a assinatura usando sua chave privada.
- tests/UpdateTests.cs: valida assinatura, adulteração, hash/tamanho, destino, substituição e preservação.

MainForm.cs e Program.cs conectam os novos módulos à interface e ao auxiliar; build.ps1 foi atualizado.

## Verificação realizada

Compilação e testes de configuração, SOCKS5 e atualização passaram. A janela Atualizações foi renderizada e inspecionada. Um teste local com versão anterior conseguiu substituir o EXE para 1.1.0.0, criar backup e preservar app-config.json. O manifesto e o executável publicados precisam ser verificados também após a publicação no GitHub. Não foi validado o login/jogo/Webshare.

Fontes: https://docs.github.com/en/repositories/releasing-projects-on-github/linking-to-releases e https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.rsacryptoserviceprovider.verifydata


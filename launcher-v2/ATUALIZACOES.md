Canal das versões de teste (desde 2.0.3.0): ao abrir, consulta até 100 releases publicadas via API do GitHub, incluindo pré-releases. Ignora drafts e pacotes sem manifesto/assinatura/executável. Escolhe a maior versão numérica; verifica assinatura, correspondência da tag, hash e versão antes de aplicar. Sem rede ou com erro mantém o aplicativo atual. Builds anteriores usam somente /latest e precisam instalar esta correção manualmente uma vez.

# Atualizações automáticas — V2.0.1.0

Ao abrir normalmente, o PKA consulta as Releases estáveis de Lushen16/LIT-fix, verifica a assinatura RSA do manifesto e, quando encontra uma versão mais recente, baixa, valida, instala e reinicia sem confirmação adicional. Não consulta commits da branch nem compila código baixado. Não instala drafts ou prereleases.

Uma vez, substitua a versão anterior pelo novo PKA-Proxy.exe entregue. A V2.0.0 não contém este mecanismo, portanto não consegue instalar esta primeira atualização automaticamente.

Durante a verificação/download, o estado aparece na barra lateral e os controles de ativação ficam bloqueados. Sem internet, sem release compatível ou com download/assinatura inválidos, a versão instalada é mantida e a verificação será tentada na próxima abertura. O programa não solicita login GitHub ou usa tokens; o repositório precisa continuar público.

Se houver motor ativo de outra instância, a atualização fica adiada para outra abertura. A atualização não remove filtros WFP ou libera rede direta, não altera perfil DPAPI e não transmite credenciais de proxy. O executável da interface já é uma exceção explícita do roteamento para diagnóstico; a consulta HTTPS do updater ocorre por esse processo.

O helper usa uma cópia do executável instalado. Ele valida novamente o manifesto e o arquivo, confere PID/caminho/hora de início do processo de origem, confirma prontidão antes do fechamento e só então aguarda a janela encerrar. A troca usa File.Replace, com backup `.backup-*` na pasta do executável. Se o Windows recusar a troca, o arquivo anterior continua preservado; se o lançamento falhar após a troca, o helper tenta restaurar e iniciar o backup. Uma falha interna depois de o novo aplicativo já ter iniciado não é detectada por esse rollback. Perfis são separados do executável.

Mantenha o PKA numa pasta gravável pela sua conta. O updater testa acesso de escrita antes de fechar a janela; não usa elevação silenciosa nem altera permissões de Program Files. Backups e cache de atualização ficam preservados e podem ser removidos manualmente depois de confirmar que a nova versão funciona. A autenticação RSA do pacote é separada de Authenticode; o aplicativo continua sem assinatura Authenticode.

## Publicar futuras atualizações

1. Aumente a versão em src/AssemblyInfo.cs. A versão deve ser maior que a instalada; use quatro componentes, como 2.0.2.0.
2. Compile com build.ps1.
3. Execute prepare-release.ps1 com o caminho da chave privada existente do projeto:

```powershell
.\prepare-release.ps1 -PrivateKeyPath 'CAMINHO-LOCAL-DA-CHAVE.xml'
```

4. Além dos três arquivos obrigatórios do updater, o script gera PKA-Proxy.exe.sig, PUBLIC-KEY.xml e PUBLIC-KEY-SHA256.txt para verificação independente. Inclua também Verificar-assinatura.ps1. Consulte SEGURANCA.md. Publique uma Release estável na tag exata informada pelo script, por exemplo v2.0.2.0, com os três arquivos gerados em release/: PKA-Proxy.exe, update.json e update.sig. Marque-a como latest. Só publique como estável depois de concluir a validação do aplicativo.

O cliente verifica `releases/latest/download/update.json` e `update.sig`, mas o executável é baixado da tag da versão assinada para evitar a troca de versão durante o download. O manifesto limita tamanho, nome, SHA-256 e versão do assembly. A chave pública existente do projeto permanece no executável; uma alteração do repositório não é suficiente para produzir um pacote válido sem a chave privada.

**Nunca publique a chave privada**, nem a inclua em ZIP, commit ou Release. Ela foi reutilizada somente para assinar os arquivos; permanece no local anterior. O pacote entregue inclui apenas chave pública, manifesto e assinatura. A integridade depende da custódia dessa chave e da segurança do Windows/TLS.

## Testes

`test-update.ps1` cria uma chave de teste independente e executáveis pequenos em diretório temporário. Valida assinatura alterada, download incompleto/corrompido, versão incompatível, backup, preservação de perfil e rollback. Em seguida executa o helper real contra um aplicativo de teste, confirma saída do processo, substituição e reinício automático. Não atualiza o PKA real, não usa a chave real e não muda rede ou firewall.

Verificação desta entrega: 13 verificações do updater aprovadas, fluxo de atualização completo aprovado, pacote real validado com a chave pública existente e consulta real ao GitHub aprovada. Continuam pendentes os testes de roteamento/WFP descritos em VALIDACAO-VM.md.

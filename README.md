# PKA Proxy

Aplicativo desktop Windows para configurar SOCKS5 Webshare por aplicativo, comparar IP direto e IP pelo proxy e receber atualizações assinadas.

## Baixar

Baixe **PKA-Proxy-Windows.zip** em [Releases](https://github.com/Lushen16/pka-proxy/releases/latest), extraia em uma pasta pessoal e abra **PKA-Proxy.exe**. Requer .NET Framework 4.x (recomendado 4.7.2 ou mais recente). Não usa navegador ou Python.

## Roteamento

Selecione o cliente final .exe que o launcher abre. Os filhos não herdam automaticamente a regra do launcher. A janela Roteamento permite incluir o launcher separadamente ou gerar uma regra Global / Todos os aplicativos, com opções UDP e IPv6.

O aplicativo gera a configuração, mas a interceptação depende de **ProxiFyre + Windows Packet Filter**, instalados separadamente pela [distribuição oficial](https://github.com/wiresock/proxifyre/releases). Aplique a configuração e reinicie o serviço pela interface oficial. Salvar não ativa o serviço. Não há motor/driver próprio incluído.

Global cobre os protocolos selecionados dos aplicativos identificados pelo motor, com o configurador excluído. Não é garantia de todo pacote do sistema: UDP depende do proxy, ICMP não é coberto, DNS e processos sem identificação precisam de verificação, e não há kill switch. Consulte ALTERACOES.md.

## Check

Consulta HTTPS ao ipify pela conexão direta e por um túnel SOCKS5 autenticado. Exibe os dois IPs. Uma falha no proxy não faz fallback direto. O resultado comprova apenas o teste, não as conexões do jogo/sistema. Uma VPN pode afetar a saída direta.

## Atualizações

A distribuição vem configurada para este repositório e consulta novas versões ao abrir. Em Atualizações, você pode desativar essa consulta. A instalação ocorre ao clicar em Atualizar agora.

O cliente aceita apenas manifestos com assinatura RSA/SHA-256 da chave pública embutida, verifica o hash/tamanho/versão do EXE, cria backup e reabre. A assinatura da atualização é própria do aplicativo; o EXE ainda não tem assinatura Authenticode reconhecida pelo Windows. Campos são preservados no perfil local, com senha protegida por DPAPI. O JSON exportado para o motor contém a senha em texto claro.

## Compilar e publicar

Execute build.ps1. Neste repositório os módulos .cs estão na raiz; no ZIP estão na pasta src. O script suporta os dois layouts. Aumente a versão em AssemblyInfo.cs antes de publicar uma nova release.

Use prepare-release.ps1 com a chave privada guardada fora do repositório, para gerar PKA-Proxy.exe, update.json e update.sig. Publique os três arquivos com a tag exata indicada pelo script (exemplo: v1.1.0.0). Não publique chaves privadas ou credenciais.

A chave privada não integra este repositório nem os pacotes públicos. Os binários Drover não foram executados, reutilizados ou incluídos. Não foi validado tráfego real do PokeAlliance/Webshare; faça testes na sua VM.

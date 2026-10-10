# PKA Proxy Launcher V2.0.1 — candidato para validação

Interface Windows em preto, azul e azul-claro, com Dashboard, Aplicativos e Teste. Usa um túnel sing-box real e filtros persistentes da Windows Filtering Platform (WFP). Não usa variáveis de ambiente, proxy WinINET ou uma simulação de interceptação.

**Estado de validação:** compilação, 54 verificações automatizadas, três telas renderizadas e configurações aceitas pelo sing-box 1.14.2. A ativação administrativa dos filtros WFP, roteamento de aplicações reais, queda do motor e Webshare ainda precisam de teste em uma VM Windows. Não trate esta versão como uma garantia de ausência de vazamentos em produção.

## Usar

1. Extraia o pacote em uma pasta fixa. Execute `PKA-Proxy.exe` em Windows x64 com .NET Framework 4.8. Não execute simultaneamente uma versão anterior, ProxiFyre ou outra VPN/TUN.
2. Em Aplicativos, adicione os executáveis `.exe` e marque os que devem usar proxy. Cadastre também o cliente final e cada executável auxiliar que abre conexões; processos filhos não herdam automaticamente a seleção. Feche os aplicativos antes de ativar.
3. No Dashboard, informe o IPv4, porta, usuário e senha SOCKS5 da Webshare. Use o IPv4 para evitar resolução DNS direta do próprio servidor proxy. Sem autenticação, deixe os dois campos vazios.
4. Escolha modo global ou por aplicativo. Opcionalmente marque abertura automática e escolha um aplicativo marcado.
5. Clique em Ativar proxy e permita a elevação do Windows. O motor testa o SOCKS5, arma o bloqueio de saída, inicia o TUN, permite somente a interface do túnel e testa HTTPS de um processo protegido. Só então abre o aplicativo escolhido.
6. Em Teste, execute Testar SOCKS5 e Testar túnel. O primeiro negocia SOCKS5, autenticação e CONNECT, valida TLS e consulta IP público. O segundo usa outro executável sujeito ao túnel e ao bloqueio. Um resultado positivo não comprova todos os sockets do jogo. Enquanto ativo, o launcher repete uma consulta HTTPS do processo protegido a cada 30 segundos; falhas aparecem no Dashboard e no log, sem desligar o bloqueio.
7. Parar túnel e fechar a janela **mantêm o bloqueio persistente**. Para voltar à rede normal, pare o motor e clique em Liberar rede direta. Se a janela não abrir, execute `Recuperar-rede.cmd` como administrador. Não apague o pacote enquanto precisar recuperar a rede.

## Alcance da proteção

| Modo | Tráfego encaminhado | Tráfego bloqueado |
| --- | --- | --- |
| Por aplicativo | TCP/IPv4 dos caminhos marcados e do processo de teste | Saída desses executáveis por outras interfaces; IPv6 desses executáveis; UDP/ICMP dos executáveis marcados no túnel |
| Global | TCP/IPv4 dos demais processos | Saída por interfaces diferentes do TUN; IPv6; UDP/ICMP no túnel |

O motor e o executável exato da interface ficam fora do bloqueio e do roteamento, para transporte SOCKS5 e diagnóstico. Não há exceção por nome de processo, nem bypass automático da rede local para apps protegidos. No modo global, isso também restringe rede local, serviços do sistema e protocolos como QUIC, DHCP e descoberta de rede. Depois de uma mudança de rede, talvez seja necessário liberar explicitamente o bloqueio para renovar conectividade e depois reativar a proteção.

DNS na porta 53 é encaminhado pelo TUN para DNS-over-HTTPS via SOCKS5. Filtros persistentes também bloqueiam a porta 53 fora do TUN, mesmo no modo por aplicativo, portanto DNS compartilhado do Windows é afetado. No modo por aplicativo, outros processos, auxiliares não cadastrados e serviços compartilhados que usam DoH/DoT fora da porta 53 não estão cobertos. Se um app delegar sua rede a outro processo ou serviço, a proteção depende de proteger esse processo também; considere o modo global. Não afirmamos isolamento completo de um aplicativo arbitrário nem anonimato.

Os filtros são persistentes no BFE, não apenas enquanto a janela está aberta. Uma falha do proxy não muda o destino para saída direta. Falha do motor ou da janela deixa as restrições instaladas; recuperação é uma ação explícita. O bloqueio é transacional: se a instalação falhar, o app não é iniciado. Antes do TUN ficar pronto, as conexões dos alvos ficam bloqueadas. Não existem filtros de boot anteriores ao BFE: proteção durante inicialização do Windows, BFE desativado, outros filtros com permissões superiores, alterações feitas por administrador e ataques de kernel estão fora da garantia. A subcamada persistente fica instalada após recuperação, mas sem filtros; sua presença não bloqueia a rede.

Conexões existentes não são migradas. O launcher exige fechar processos selecionados antes de ativar; conexões antigas e comportamento de aplicações globais devem ser conferidos na VM. A consulta de um processo de teste não substitui captura de pacotes do app real. Não compare IPs por igualdade como única prova: a Webshare pode rotacionar a saída.

## Credenciais e logs

O perfil inteiro, incluindo usuário, senha, apps e configurações, usa DPAPI CurrentUser em `%APPDATA%\PKAproxyV2\session-v2.bin`. Não copia automaticamente configurações antigas. Pedidos entre a janela e o helper também usam DPAPI. Senha aparece mascarada na interface e não entra na linha de comando, repositório, pacote ou logs. O arquivo temporário `active.json` precisa conter credenciais enquanto o sing-box roda; fica em `%PROGRAMDATA%\PKAproxyV2\Tun`, acessível somente a Administradores e SYSTEM, e é removido ao encerrar normalmente. Uma queda brusca do helper pode deixar esse arquivo protegido no disco. Administradores, processos da mesma conta e dumps de memória não são isolados por DPAPI.

Logs do motor ficam em `%APPDATA%\PKAproxyV2\tun-diagnostic.log`, com redação de usuário/senha e limite de tamanho. Os eventos da interface são mantidos em memória. Sem exportação automática de diagnósticos. A verificação de atualização ocorre automaticamente na abertura; consulte ATUALIZACOES.md.

SOCKS5 com usuário/senha não cifra o enlace entre cliente e proxy; HTTPS cifra o conteúdo do destino, não a negociação SOCKS5. Não há fallback HTTP, direto ou para outro servidor.

## Compilar e testar

Em PowerShell, dentro desta pasta:

```powershell
.\build.ps1
.\test.ps1
.\test-update.ps1
```

O build usa o compilador C# do .NET Framework instalado no Windows e gera aplicação e helper x64. `core.gz` contém o sing-box fixado; sua integridade é conferida antes de execução. O processo de teste é incorporado ao binário. Não há instalador novo nem certificado Authenticode neste candidato. O executável tem assinatura RSA destacada verificável em PKA-Proxy.exe.sig; consulte SEGURANCA.md.

`test.ps1` não altera firewall ou rotas e não usa credenciais reais. Executa servidores SOCKS5 em loopback e verifica autenticação, respostas fragmentadas/inválidas, destino com DNS remoto, configuração de ambos os modos, rejeição de UDP/IPv6, cópia das seleções, perfil criptografado e estruturas nativas x64. Consulte `VALIDACAO-VM.md` para testes que ainda faltam.

## Componentes e referências

Motor: sing-box 1.14.2, revisão af6e64c3b69e6132ebaee0e1a3d24e93903f6709, reaproveitado do pacote anterior. SHA-256 descomprimido: `7BBEF1DEA9189EE12799AE834EA4B4658355DA25C47A21AD8804904C0CCD9410`.

Os arquivos drover.exe, version.dll e drover-packet.bin do exemplo anterior não são executados nem distribuídos neste projeto. Não se injeta DLL no jogo.

Fontes correspondentes do sing-box estão em `upstream/sing-box-v1.14.2-source.zip`; dependências e instruções de construção constam no projeto upstream. Licenças de terceiros estão em `licenses/` e `LICENSE-sing-box.txt`.

- [Regras por caminho do processo](https://sing-box.sagernet.org/configuration/route/rule/)
- [TUN e limites de strict_route no Windows](https://sing-box.sagernet.org/configuration/inbound/tun/)
- [Condições WFP e LUID de interface](https://learn.microsoft.com/en-us/windows/win32/fwp/filtering-condition-identifiers-)
- [Persistência dos filtros WFP](https://learn.microsoft.com/en-us/windows/win32/api/fwpmtypes/ns-fwpmtypes-fwpm_filter0)

# PKA Proxy — código modularizado

Abra PKA-Proxy.exe. Para recompilar, execute build.ps1 em uma sessão PowerShell com permissão para scripts locais. Requer o compilador .NET Framework do Windows; sem Python ou navegador.

## Fluxo de roteamento

Na janela principal, escolha o CLIENTE FINAL do jogo, isto é, o executável que mantém as conexões. Localize-o com o jogo aberto em Gerenciador de Tarefas > Detalhes > Abrir local do arquivo. O motor não herda regras entre pai e filho. Em Roteamento, é possível incluir separadamente o launcher .exe: os dois caminhos entram na mesma regra. Outros executáveis auxiliares precisam ser adicionados no motor. A comparação de caminho do motor é por substring, não por PID ou descendência.

Em Roteamento, selecione Global / Todos os aplicativos do Windows para gerar a regra catch-all appNames=[""]. Esse modo dispensa escolher o jogo. Ao mudar para global, UDP e IPv6 são marcados; podem ser desmarcados conforme suporte do proxy. TCP permanece habilitado. LAN bypass fica desativado. Este configurador é excluído pelo seu caminho para manter o Check direto fora da regra global. Mover o EXE depois de gerar o JSON exige gerar novamente. As exclusões do motor são por substring. VPNs/outros túneis podem precisar de exclusões adicionais no motor para evitar recursão.

Use Salvar configuração e aplique/reinicie no ProxiFyre instalado, por sua interface oficial. Salvar e Usar este modo alteram somente o estado/configuração, não ativam o serviço. Abra a interface do motor com Abrir motor instalado. Não há motor/driver próprio embutido, instalação automática ou alteração do proxy HTTP do Windows. Serviço elevado com Windows Packet Filter é necessário para atribuição abrangente de processos. Motor e dependências: https://github.com/wiresock/proxifyre/releases

Global é roteamento dos protocolos selecionados dos aplicativos identificados pelo motor, não garantia de todo pacote do sistema. ICMP não é SOCKS5; UDP depende do servidor; UDP IPv6 fragmentado pode passar direto; processos sem atribuição podem não ser capturados, especialmente sem elevação. IPv6 precisa funcionar na máquina e ser aceito pelo proxy. Selecionar apenas IPv4 bloqueia os destinos IPv6 correspondentes segundo o motor. Sem kill switch; parar o motor não mantém o bloqueio. Não foi validado com jogo/Webshare ou tráfego global real. Não promete DNS remoto exclusivo.

## Check

Consulta HTTPS ao ipify pela saída direta e por um túnel SOCKS5 explícito, valida TLS e compara IPs. Não há fallback direto no teste do proxy. Mesmo IP significa que o túnel respondeu com a mesma saída, não que foi ignorado. O Check não confirma roteamento do jogo/sistema. VPNs e regras externas ainda podem afetar a consulta direta. Credenciais são enviadas ao proxy; ao salvar, ficam em texto claro no JSON. Os binários Drover não foram executados nem incluídos.

## Arquivos

- src/MainForm.cs: janela principal, resumo do modo, Check e exportação.
- src/RoutingDialog.cs: janela dedicada aos modos, launcher opcional e protocolos.
- src/RoutingConfiguration.cs: modelo RoutingOptions, validações e serialização JSON; regra por executáveis ou catch-all; exclusão do configurador no modo global.
- src/ExecutablePicker.cs: seletor com atalhos e orientação para localizar o cliente final.
- src/UiTheme.cs: CTA com ícone de pasta, contraste, negrito, padding, hover e active.
- src/ProxyDiagnostics.cs: conexão direta, negociação SOCKS5, autenticação e consulta HTTPS.
- src/Program.cs: inicialização do app e renderização de prévias.
- build.ps1: recompila o executável.
- tests/: testes de configuração e protocolo.

Compilação e testes passaram; prévias das duas janelas foram inspecionadas. Testes de roteamento cobrem cliente, launcher, deduplicação, catch-all, exclusões, protocolos e seleção inválida; testes de SOCKS5 cobrem nove respostas/situações. Estados de hover/active são definidos no controle; não houve teste interativo completo com mouse. EXE sem assinatura digital.

Esquema e limites do motor consultados: https://github.com/wiresock/proxifyre/blob/main/docs/configuration.md

# PKAproxy 1.3.0.0 — correção de ativação

O modo Global anteriormente só gerava uma configuração. Agora Conectar / aplicar instala o ProxiFyre oficial 2.6.1 (SHA-256 fixado), seus pré-requisitos pelo instalador oficial e aplica a configuração no serviço elevado ProxiFyreService. Desconectar interrompe esse serviço. Não altera o proxy WinINET do Windows.

Na VM:
1. Instale PKAproxy-Setup.exe por cima da versão atual.
2. Preencha a proxy e abra Roteamento > Global.
3. Clique em Conectar / aplicar. Permita a elevação e conclua o instalador oficial do ProxiFyre. Se ele pedir reinicialização, reinicie a VM e clique em Conectar novamente.
4. Espere o teste automático ou clique em Verificar Global. Um processo independente consulta HTTPS e compara o IP com a saída SOCKS5 esperada.
5. Feche completamente e reabra o navegador. Conexões existentes precisam ser renovadas.
6. Desconectar restaura o tráfego direto ao parar o serviço. Fechar PKAproxy mantém o serviço ativo; use Desconectar antes de sair se desejar rede normal.

O PKAproxy permanece em %APPDATA%\PKAproxy. O motor e driver devem ser instalados em diretórios protegidos do Windows; não ficam em AppData. A senha de roteamento fica no app-config.json do motor, com acesso limitado a Administradores e SYSTEM. A sessão do aplicativo e o pedido entre processos usam DPAPI.

Check proxy confirma apenas uma conexão SOCKS5 explícita. Verificar Global usa PKArouteProbe.exe separado e confirma HTTPS/TCP; não valida todo o tráfego, UDP, DNS, IPv6 ou o jogo. O motor encaminha apenas os protocolos configurados e identificados. UDP exige suporte da proxy; fragmentos UDP IPv6 podem passar diretamente. Não há kill switch. A comparação por IP não garante exclusividade se sua rede direta e sua proxy tiverem a mesma saída.

Testes de configuração, SOCKS5, assinatura de atualização, instalação extraída e consulta independente foram feitos. Não instalamos o driver nem testamos roteamento real neste computador: o usuário pediu somente preparar para a VM. Esta versão precisa de validação na VM antes de declarar o Global operacional ali.

Fontes oficiais: https://github.com/wiresock/proxifyre/releases/tag/v2.6.1 e https://github.com/wiresock/proxifyre/blob/main/docs/configuration.md

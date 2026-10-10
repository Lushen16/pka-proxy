# Discord permanente — Litfix 2.0.17.0

Na aba Discord, informe IPv4, porta, usuário e senha de uma SOCKS5. O Discord.exe é detectado automaticamente; Localizar Discord permite escolher uma instalação diferente. Feche o Discord e desative/libere qualquer túnel geral antes de ativar.

Testar proxy verifica autenticação e HTTPS de saída pela proxy. Ativar permanente pede autorização de administrador do Windows. Depois da confirmação, a janela do Litfix pode ser fechada.

A ativação instala uma tarefa do Windows para a conta atual: início ao fazer login, execução em segundo plano e tentativa de recuperação após falhas. A cópia do motor e o perfil ficam em uma pasta protegida em ProgramData. As credenciais são cifradas com DPAPI. Uma cópia editável cifrada do perfil fica na pasta de configurações do usuário. Nenhuma senha de login do Windows é armazenada.

Este mecanismo encaminha o tráfego do Discord pelo Litfix; não altera arquivos ou configurações internas do Discord. Dashboard e Discord compartilham um único motor com saídas separadas. O Discord mantém sua própria SOCKS5, mesmo com Dashboard global ou HTTPS. Parar o Dashboard ou fechar sua janela retorna ao modo Discord sozinho. Para remover a configuração permanente, pare o Dashboard primeiro. Ao migrar de versões anteriores à 2.0.17, remova e reative a configuração para instalar o motor novo. A troca de configuração reinicia o motor e pode interromper conexões brevemente. Na aba Discord, TCP (login e mensagens) usa SOCKS5; UDP das chamadas IPv4 usa a conexão normal, pelo outbound direto do motor. A proxy não precisa aceitar UDP. IPv6 e demais tráfegos não suportados continuam bloqueados. O túnel geral mantém seu comportamento anterior.

O motor tenta reconectar a cada 60 segundos enquanto a configuração existe. Os filtros persistentes bloqueiam saída direta dos executáveis já registrados se o motor parar. Regras pelo nome Discord.exe cobrem mudanças de versão enquanto o túnel está ativo, e os filtros são atualizados para novos caminhos detectados a cada 15 segundos. Ainda há uma janela entre a criação de uma versão nova e o registro dos filtros: não foi demonstrada uma garantia de ausência de vazamento em todas as atualizações do Discord.

Remover configuração apaga a tarefa, encerra o motor permanente, remove seus filtros e apaga as credenciais salvas. A desinstalação chama a mesma limpeza e recusa continuar se ela falhar. Logs de diagnóstico podem permanecer. Para configurar outra proxy, remova a anterior e ative novamente.

É necessária uma conta com privilégios de administrador, elevando com a mesma conta que salvou o perfil; a elevação usando outra conta não consegue abrir as credenciais protegidas da primeira.

Validação feita: compilação x64, 69 verificações da suíte, testes de CONNECT e retorno UDP por relay local, criptografia e corrupção do perfil separado, descoberta de versões e validação da definição de tarefa pelo Agendador do Windows com TASK_VALIDATE_ONLY (sem registrar tarefa). O motor aceitou as configurações do Discord e dos protocolos. A nova aba foi renderizada e conferida.

Ainda não foram exercitados neste computador: instalar a tarefa de verdade, ativar WFP/TUN permanente, reiniciar Windows, fazer chamada com uma proxy real e desinstalar com a tarefa ativa. O instalador é uma compilação local de teste, sem assinatura RSA ou Authenticode.

Validação de integração recomendada em VM: instalar; testar proxy; ativar permanente; fechar a janela e verificar chamada/IP; reiniciar e fazer login; verificar retomada e bloqueio com proxy indisponível; simular nova versão do Discord; remover configuração e conferir retorno à rede normal; reativar e desinstalar, confirmando ausência da tarefa e dos filtros.

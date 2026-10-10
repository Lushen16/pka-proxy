# Validação pendente em VM Windows x64

Use uma VM com console local, snapshot e sem outra VPN/ProxiFyre ativo. Os testes abaixo são necessários antes de declarar proteção contra vazamento verificada. Não foram executados neste computador. Nenhuma credencial Webshare real foi fornecida nesta conversa.

1. Compilar e executar test.ps1: esperar 54 verificações aprovadas. Conferir as três telas em escala 100%, 125% e 150%.
2. Credenciais incorretas, porta indisponível e resposta SOCKS5 inválida: ativação deve falhar, estado deve informar erro e aplicativo selecionado não deve abrir. Nenhuma consulta de IP deve tentar sair diretamente como fallback.
3. Configuração correta no modo por aplicativo: cadastrar um cliente de teste e um aplicativo não marcado. Cliente marcado deve sair via proxy; não marcado deve permanecer direto. Repetir com executáveis de mesmo nome em caminhos diferentes.
4. Usar captura na interface física e no TUN. Para o app marcado, destinos públicos devem aparecer como conexões SOCKS5 do motor na interface física, nunca como sockets diretos do app. Rede local, sockets explicitamente vinculados à interface física, IPv6, UDP e ICMP do alvo devem falhar. Verificar processos filhos, auxiliares e serviço de DNS separadamente.
5. Conferir resolução DNS na porta 53 e tentativas DoH/DoT feitas pelo próprio alvo e por serviços compartilhados. No modo por aplicativo, DoH de outro processo não está coberto: não classificar como protegido. Documentar cada executável que o jogo utiliza.
6. Abrir automaticamente: testar uma seleção válida, arquivo removido, seleção vazia, cliente já aberto e falha de teste HTTPS após iniciar o TUN. Só deve iniciar o arquivo escolhido quando a ativação e o teste protegido tiverem passado.
7. Derrubar o servidor SOCKS5 durante transferência; tentar novos sockets. Nenhum socket do alvo deve usar a conexão direta. Testar também queda abrupta de sing-box.exe, helper e janela, e parada normal. Verificar que filtros continuam no BFE e que o status da janela detecta queda do motor.
8. Adicionar/trocar adaptador e sair/voltar de suspensão. Tentativas por interfaces novas devem ser bloqueadas. Reiniciar a VM e conferir restrições persistentes quando BFE está ativo. A fase de boot anterior ao BFE não é coberta.
9. Liberar rede direta apenas após parar o túnel. A operação deve exigir administrador, excluir somente GUIDs registrados pelo PKA V2 e restaurar acesso sem apagar regras existentes do Windows. Repetir pelo Recuperar-rede.cmd após fechar a janela e após uma queda brusca.
10. Tentar liberar enquanto outra instância ou outro usuário tem motor ativo: deve ser recusado pelo mutex global. Testar duas ativações simultâneas.
11. Conferir ACL da pasta administrativa e de active.json, perfil DPAPI, pedidos temporários e logs. Usar credenciais de teste fáceis de identificar, nunca imprimir credenciais reais em evidências. Depois de encerramento normal, active.json deve ser removido; depois de uma queda abrupta pode restar somente na pasta protegida.
12. No modo global, repetir IP/TCP de navegador e serviços, UDP/QUIC, IPv6, DNS, LAN, conexões preexistentes e queda do motor. A interface PKA e o núcleo são exceções explícitas. Não concluir sucesso apenas porque o IP do processo de teste mudou.

Resultado esperado deve ser registrado com versão do Windows, motor, aplicativos, adaptadores, evidências de captura e recuperação. Se qualquer conexão direta de um processo coberto for observada, este candidato não pode ser aprovado para uso com dados reais.

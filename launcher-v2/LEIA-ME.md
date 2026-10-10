# Litfix 2.0.14.0 — compilação local de teste

Proxy manual / automática nas abas Dashboard e Discord. No modo automático, escolha uma opção numerada para preencher IP, porta, usuário e senha. Cada aba lembra sua seleção; voltar ao modo manual restaura os dados digitados. Importar lista aceita IP:porta:usuário:senha, uma proxy por linha. O catálogo e as preferências são cifrados por usuário no computador e não são incluídos no instalador público ou no código-fonte. Não há rotação automática de IP durante uma sessão.

Corrigida a falha de abertura ao carregar aplicativos selecionados de um perfil salvo. O teste test-startup.ps1 reproduziu o erro na 2.0.9 e confirmou abertura e seleção na versão corrigida.

Nova aba Discord com proxy separada, execução em segundo plano, início ao entrar no Windows e limpeza na desinstalação. Leia DISCORD-PERMANENTE.md para funcionamento e limites de validação.

- Marque SOCKS5, HTTPS ou ambas. Ambas testa SOCKS5 primeiro na ativação e utiliza HTTPS se a primeira falhar, desde que UDP não esteja habilitado. O túnel usa um protocolo por sessão; não alterna durante a sessão.
- O botão Testar proxy verifica todos os protocolos marcados e mostra o IP retornado por cada um. A porta e as credenciais são compartilhadas entre os protocolos.
- HTTPS usa CONNECT. TLS até a proxy vem habilitado; informe o nome do certificado se o servidor exige um domínio. Para um endpoint HTTP CONNECT que aceita destinos HTTPS, desmarque TLS até a proxy. Isso mantém TLS com o destino, mas transmite a autenticação Basic sem TLS até a proxy.
- Na aba Discord, login e mensagens usam a proxy e as chamadas UDP usam a conexão normal. A ativação não exige UDP da proxy. No túnel geral, para chamadas Discord, selecione SOCKS5 e Chamadas / UDP. Adicionar Discord marca UDP automaticamente. A proxy precisa oferecer UDP ASSOCIATE e retorno real de UDP. O teste faz uma consulta DNS via relay antes da ativação. Sem UDP, uma troca de protocolo não corrige Sem rota. O programa explica o impedimento antes de abrir o túnel.
- A tabela Teste lista os aplicativos selecionados (ou cadastrados no modo global). Testar túnel consulta o IP usando um processo de diagnóstico protegido pela mesma rota. O resultado é o IP do túnel: não constitui medição individual do tráfego de cada aplicativo. No modo global a lista não é um inventário de todos os processos do Windows.
- O texto inferior da tela Teste foi removido. A fonte usa Segoe UI Variable Text, Bahnschrift ou Segoe UI, nessa ordem, conforme disponibilidade no Windows.
- Feche os aplicativos antes de ativar. Parar/fechar mantém a proteção persistente. Para voltar à conexão normal, pare o túnel e use Liberar rede direta.

Validação: compilação x64; 69 verificações de configuração, autenticação SOCKS5, armazenamento protegido e estruturas nativas; testes adicionais de CONNECT/autenticação, respostas malformadas e retorno real de UDP em relay local. As configurações SOCKS5/UDP e HTTPS foram aceitas pelo motor sing-box incluído. As duas telas foram renderizadas e conferidas. Chamadas reais do Discord, sua proxy e a ativação WFP/TUN não foram exercitadas nesta execução.

Este instalador local não possui assinatura RSA da distribuição nem Authenticode. Atualizações automáticas continuam exigindo a assinatura original; essa proteção não foi alterada.

Compilar: build.ps1. Instalador local: build-installer.ps1 -CompilerPath CAMINHO_ISCC -LocalUnsigned. Para releases assinados, omita LocalUnsigned depois de gerar as assinaturas correspondentes. Fontes/licenças do motor: upstream/ e licenses/.

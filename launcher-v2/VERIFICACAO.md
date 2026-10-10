# Verificação executada — 10/10/2026

- Compilação da aplicação Windows e helper x64: aprovada, sem avisos.
- Testes automatizados: 54 verificações aprovadas.
- SOCKS5 em TCP loopback: sem autenticação, usuário/senha, autenticação recusada, método recusado, versão inválida e respostas fragmentadas.
- Configuração: modo global, seleção por caminho, rejeição de seleção vazia/UDP/IPv6/host de bootstrap e cópia independente das seleções.
- DPAPI: perfil inteiro criptografado, recuperação de usuário/senha e rejeição de perfil adulterado.
- Interop: tamanhos x64 de FWP_VALUE0, condição e filtro; GUIDs estáveis de recuperação.
- sing-box 1.14.2: `check` aceitou as configurações global e por aplicativo.
- Dashboard, Aplicativos e Teste: renderizados e inspecionados em 100%.
- Alterações de rede no computador: nenhuma; testes não ativaram TUN ou filtros WFP.

Não executados: validação administrativa do WFP, queda do motor, captura de tráfego de executáveis reais, DNS/DoH compartilhado, reinício, outras escalas DPI e credenciais reais Webshare. Consulte VALIDACAO-VM.md.

GitHub: https://github.com/Lushen16/LIT-fix/pull/1 (rascunho).
Branch: codex/pka-launcher-v2. Commit inicial: 03bc363.

Atualização V2.0.1.0: 13 verificações do updater e teste completo de troca/reinício aprovados; manifesto real assinado e executável validados; consulta real ao latest do GitHub aprovada (v1.2.0.0, anterior à versão instalada).

Autenticidade: assinatura RSA/SHA-256 do executável verificada; alteração de um byte rejeitada; chave privada DPAPI usada na assinatura da release real; chave pública preservada. Sem certificado Authenticode.

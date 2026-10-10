# LIT-fix

Ferramentas para diagnosticar e corrigir problemas no Windows, em aplicativos e na conexão. Projeto em evolução.

O LIT-fix nasceu como um launcher de proxy e está ampliando sua proposta para reunir diagnósticos e correções em um só aplicativo. O objetivo é ajudar a identificar a causa de um problema, explicar a ação sugerida e aplicar correções específicas. Não promete resolver qualquer erro automaticamente.

## O que existe hoje

A versão de teste atual inclui:

- Interface com Dashboard, Aplicativos e Teste.
- Cadastro de executáveis e conexão por proxy SOCKS5, com modo global ou por aplicativo.
- Testes de conectividade, autenticação e IP público de saída, com diagnóstico e logs.
- Instalador Windows, atalhos e desinstalação.
- Atualização automática com validação de assinatura nas versões 2.0.3 e posteriores, incluindo versões de teste publicadas.
- Proteção das credenciais com DPAPI e assinatura RSA dos arquivos publicados.

Os recursos gerais de correção do Windows e de outros aplicativos ainda serão desenvolvidos. A mudança de proposta não significa que eles já estejam disponíveis.

## Baixar e instalar

Baixe o instalador na página de [versões publicadas](https://github.com/Lushen16/LIT-fix/releases). As versões atuais são de teste para Windows 10/11 x64 com .NET Framework 4.7.2 ou superior.

Antes de substituir uma instalação com proxy ativo, pare o túnel, clique em **Liberar rede direta** e feche o aplicativo. Versões anteriores à 2.0.3 precisam de uma instalação manual para receber a correção do atualizador.

## Estado do projeto

O módulo de proxy ainda exige validação de roteamento e proteção de rede em uma máquina virtual. O erro 12007 relatado no Shiba Launcher ainda não está confirmado como resolvido.

O módulo atual encaminha TCP/IPv4; UDP e IPv6 dos aplicativos protegidos são bloqueados. Parar ou fechar o aplicativo mantém o bloqueio de saída direta. Para retornar à conexão normal, use **Liberar rede direta** ou o procedimento de recuperação documentado.

A assinatura RSA detecta adulteração dos arquivos, mas não substitui um certificado Authenticode reconhecido pelo Windows. O instalador pode aparecer como editor desconhecido.

## Desenvolvimento

O código da versão atual está na branch [codex/pka-launcher-v2](https://github.com/Lushen16/LIT-fix/tree/codex/pka-launcher-v2), na pasta `launcher-v2`. O código legado da raiz corresponde à implementação anterior.

Consulte a [documentação do módulo atual](https://github.com/Lushen16/LIT-fix/blob/codex/pka-launcher-v2/launcher-v2/LEIA-ME.md), as [orientações de segurança](https://github.com/Lushen16/LIT-fix/blob/codex/pka-launcher-v2/launcher-v2/SEGURANCA.md) e os [testes de rede pendentes](https://github.com/Lushen16/LIT-fix/blob/codex/pka-launcher-v2/launcher-v2/VALIDACAO-VM.md).

Não adicione credenciais de proxy, chaves privadas ou certificados de assinatura ao repositório.

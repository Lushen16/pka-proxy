# PKAproxy

**PKA PROXY — conexão sob controle.** Aplicativo Windows para configurar SOCKS5 Webshare, verificar IP de saída e gerenciar regras de roteamento.

## Instalar

Baixe **PKAproxy-Setup.exe** em [Releases](https://github.com/Lushen16/pka-proxy/releases/latest). O instalador coloca PKAproxy.exe em `%APPDATA%\PKAproxy` e cria atalhos na Área de Trabalho e no menu Iniciar. Não precisa de Python ou navegador. Requer .NET Framework 4.7.2 ou superior.

A instalação do aplicativo é por usuário. O motor de redirecionamento ProxiFyre / Windows Packet Filter é uma dependência separada com componentes do Windows que exigem instalação administrativa. Esse driver não pode ficar inteiramente no AppData. O instalador PKAproxy não instala nem inclui o motor ou seus drivers. Obtenha-os na [distribuição oficial](https://github.com/wiresock/proxifyre/releases).

## Usar

1. Selecione o cliente final do jogo, informe host, porta e credenciais Webshare.
2. Use **Check proxy** para consultar o IP direto e o IP via SOCKS5.
3. Em **Roteamento**, escolha cliente + launcher opcional ou todos os aplicativos.
4. Salve a configuração e aplique-a no serviço do ProxiFyre. Salvar no PKAproxy não ativa o serviço.

Processos filhos não herdam automaticamente a regra do launcher: selecione explicitamente o cliente final. O modo global cobre os protocolos selecionados de aplicativos identificados pelo motor; não garante todo pacote do sistema. Não há kill switch. UDP, IPv6 e DNS dependem do motor, do proxy e do ambiente. O Check testa somente suas próprias conexões.

## Arquivos e atualizações

O aplicativo e seus dados ficam em `%APPDATA%\PKAproxy`. Os campos da sessão são preservados, com senha protegida pelo Windows (DPAPI). O JSON exportado ao ProxiFyre contém a senha em texto claro. A nova versão importa preferências antigas de `%LOCALAPPDATA%\PkaProxy` quando ainda não existem arquivos equivalentes na pasta nova.

O aplicativo consulta este repositório ao abrir e instala novas versões quando você clica em Atualizar agora. Cada atualização verifica assinatura RSA/SHA-256, hash, tamanho e versão antes de substituir o EXE, com backup. A assinatura de atualização é própria do aplicativo; executáveis ainda não possuem Authenticode reconhecido pelo Windows. A versão 1.1.0.0 pode receber esta atualização; para criar a instalação organizada com atalhos, execute o instalador.

`PKA-Proxy.exe` permanece como nome do arquivo remoto usado pelo atualizador para compatibilidade. O executável instalado se chama **PKAproxy.exe**.

## Desenvolvimento

Os arquivos C# são módulos de interface, configuração, diagnóstico, persistência e atualização. O código na raiz do repositório e a pasta src do pacote têm o mesmo conteúdo.

- **build.ps1**: compila o aplicativo e usa PKAproxy.ico.
- **build-installer.ps1**: compila o instalador com o aplicativo embutido; suporta Setup.cs/Integrity.cs na raiz ou na pasta installer.
- **prepare-release.ps1**: cria manifesto e assinatura usando a chave privada externa ao repositório.

Para publicar, aumente a versão em AssemblyInfo.cs, compile e assine os arquivos. Use a tag exata indicada pelo script. Publique PKA-Proxy.exe, update.json e update.sig, além do instalador. Nunca envie a chave privada ou credenciais para o repositório.

Compilação, testes de configuração/SOCKS5/assinatura e instalação/reinstalação passaram. O tráfego real do PokeAlliance/Webshare ainda precisa ser validado na VM.

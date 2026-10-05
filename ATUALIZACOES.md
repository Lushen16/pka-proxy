# Publicar atualizações

O repositório oficial é https://github.com/Lushen16/pka-proxy. A versão 1.2.0.0 instala o aplicativo em %APPDATA%\PKAproxy; clientes anteriores mantêm seus caminhos até executar o novo instalador.

1. Aumente AssemblyVersion e AssemblyFileVersion em AssemblyInfo.cs.
2. Execute build-installer.ps1 para compilar app e instalador.
3. Execute prepare-release.ps1 passando o caminho privado da chave de assinatura.
4. Publique PKA-Proxy.exe, update.json e update.sig com a tag exata indicada (exemplo: v1.2.0.0). Adicione PKAproxy-Setup.exe para novas instalações e o pacote de código-fonte, se desejado.

PKA-Proxy.exe é o nome remoto mantido para compatibilidade do atualizador. Dentro da pasta instalada o nome é PKAproxy.exe. A chave privada nunca integra o repositório ou pacotes públicos. Não edite update.json depois de assinar.

O aplicativo verifica a assinatura RSA/SHA-256 do manifesto e o hash/tamanho/versão do executável. O auxiliar cria backup e substitui apenas o EXE. A configuração da sessão e preferências de atualização ficam em %APPDATA%\PKAproxy, com senha da sessão protegida por DPAPI. O JSON exportado ao motor mantém senha em texto claro. ProxiFyre e seus drivers são atualizados separadamente.

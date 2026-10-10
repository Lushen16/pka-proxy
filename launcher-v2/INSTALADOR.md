Instalador Litfix 2.0.5.0
===================================

Baixe Litfix-Setup-2.0.5.0.exe na release oficial. Execute e siga o assistente em português. O atalho da Área de Trabalho é opcional; o menu Iniciar é criado automaticamente.

Instala em %LOCALAPPDATA%\Programs\Litfix para o usuário atual, permitindo que a atualização automática substitua o executável sem elevar o instalador. A ativação do motor continua exigindo administrador. Windows 10/11 x64 e .NET Framework 4.7.2+ são necessários. O instalador inclui o motor, licenças e fonte correspondente do sing-box, sem ativar o proxy.

Para desinstalar, pare o proxy, use Liberar rede direta e feche o Litfix. Depois remova em Configurações > Aplicativos. A desinstalação recusa motor ativo ou um registro de bloqueio de rede detectado; não remove silenciosamente filtros WFP. Configurações em %APPDATA%\PKAproxyV2 e dados protegidos em %PROGRAMDATA%\PKAproxyV2 são preservados. Se a rede continuar bloqueada, execute Recuperar-rede.cmd como administrador antes de remover o programa.

Verifique o instalador usando o script confiável da release:

    powershell -NoProfile -ExecutionPolicy Bypass -File .\Verificar-assinatura.ps1 -ExePath .\Litfix-Setup-2.0.5.0.exe

Mantenha Litfix-Setup-2.0.5.0.exe.sig ao lado do instalador. A assinatura RSA usa a mesma chave oficial do aplicativo. Não há certificado Authenticode; o Windows pode mostrar editor desconhecido.

Compilação: Inno Setup 6.5.3 oficial (assinatura Authenticode do compilador validada) e build-installer.ps1 -CompilerPath caminho\ISCC.exe. O script verifica a assinatura do aplicativo antes de empacotar. A assinatura privada do instalador é produzida separadamente fora do repositório.

Validação realizada: instalação silenciosa em pasta temporária, assinatura e hash do executável instalado, registro de desinstalação, destino do atalho, reinstalação preservando arquivo existente e desinstalação removendo componentes registrados e preservando arquivo do usuário. Proxy não foi ativado. Testes reais de WFP/TUN seguem pendentes em VM, conforme VALIDACAO-VM.md.

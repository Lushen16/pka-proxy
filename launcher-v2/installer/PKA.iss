[Setup]
AppId={{94CB85DC-2356-4C38-904B-38BC51226FB3}
AppName=PKA Proxy Launcher
AppVersion=2.0.1.0
AppPublisher=Lushen16
AppPublisherURL=https://github.com/Lushen16/pka-proxy
DefaultDirName={localappdata}\Programs\PKA Proxy Launcher
DefaultGroupName=PKA Proxy Launcher
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
OutputDir=..
OutputBaseFilename=PKA-Proxy-Setup-2.0.1.0
SetupIconFile=..\PKAproxy.ico
UninstallDisplayIcon={app}\PKA-Proxy.exe
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
DisableProgramGroupPage=yes
CloseApplications=no
RestartApplications=no
InfoBeforeFile=Instalacao.txt

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na Área de Trabalho"; GroupDescription: "Atalhos:"; Flags: unchecked

[Files]
Source: "..\PKA-Proxy.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\PKA-Proxy.exe.sig"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Verificar-assinatura.ps1"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\PUBLIC-KEY.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\PUBLIC-KEY-SHA256.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Recuperar-rede.cmd"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LEIA-ME.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\INSTALADOR.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\SEGURANCA.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\AVISOS.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LICENSE-sing-box.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\licenses\*"; DestDir: "{app}\licenses"; Flags: ignoreversion recursesubdirs
Source: "..\upstream\*"; DestDir: "{app}\upstream"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\PKA Proxy Launcher"; Filename: "{app}\PKA-Proxy.exe"
Name: "{group}\Recuperar rede (executar como administrador)"; Filename: "{app}\Recuperar-rede.cmd"
Name: "{group}\Desinstalar PKA Proxy Launcher"; Filename: "{uninstallexe}"
Name: "{autodesktop}\PKA Proxy Launcher"; Filename: "{app}\PKA-Proxy.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\PKA-Proxy.exe"; Description: "Abrir PKA Proxy Launcher"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
var Release: Cardinal;
begin
  Result := RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release) and (Release >= 461808);
  if not Result then MsgBox('Instale o .NET Framework 4.7.2 ou superior antes de continuar.', mbError, MB_OK);
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Result := '';
  if CheckForMutexes('Global\PKAproxy-TUN') then
    Result := 'Pare o proxy e feche o PKA antes de instalar. O motor de rede está ativo.';
end;

function InitializeUninstall(): Boolean;
begin
  Result := False;
  if CheckForMutexes('Global\PKAproxy-TUN') then begin
    MsgBox('Pare o proxy antes de desinstalar.', mbError, MB_OK);
    exit;
  end;
  if FileExists(ExpandConstant('{commonappdata}\PKAproxyV2\guard.keys')) then begin
    MsgBox('Antes de desinstalar, abra o PKA e use Liberar rede direta, ou execute Recuperar-rede.cmd como administrador. O bloqueio de rede ainda está registrado.', mbError, MB_OK);
    exit;
  end;
  Result := True;
end;

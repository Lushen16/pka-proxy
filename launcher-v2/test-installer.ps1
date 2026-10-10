param([Parameter(Mandatory=$true)][string]$TestDirectory)
$ErrorActionPreference='Stop'
$id='{94CB85DC-2356-4C38-904B-38BC51226FB3}_is1'
$registry='HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\'+$id
if(Test-Path $registry){throw 'Já existe uma instalação registrada. Use uma VM limpa para este teste.'}
$destination=[IO.Path]::GetFullPath($TestDirectory)
if(Test-Path $destination){throw 'O diretório de teste deve ser novo.'}
$setup=Join-Path $PSScriptRoot 'Litfix-Setup-2.0.5.0.exe'
$group='PKA Test '+[Guid]::NewGuid().ToString('N')
$arguments=@('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART','/TASKS=""',('/DIR="'+$destination+'"'),('/GROUP="'+$group+'"'))
function RunSetup { $p=Start-Process $setup -ArgumentList $arguments -WindowStyle Hidden -Wait -PassThru; if($p.ExitCode -ne 0){throw ('Instalação falhou: '+$p.ExitCode)} }
RunSetup
$target=Join-Path $destination 'PKA-Proxy.exe'
if((Get-FileHash $target).Hash -ne (Get-FileHash (Join-Path $PSScriptRoot 'PKA-Proxy.exe')).Hash){throw 'Executável instalado diferente'}
& (Join-Path $PSScriptRoot 'Verificar-assinatura.ps1') -ExePath $target
if(!(Test-Path $registry)){throw 'Desinstalação não registrada'}
$installedGroup=(Get-ItemProperty $registry).'Inno Setup: Icon Group'
$shortcut=Join-Path ([Environment]::GetFolderPath('Programs')) ($installedGroup+'\Litfix.lnk')
if(!(Test-Path $shortcut)){throw 'Atalho não criado'}
$shell=New-Object -ComObject WScript.Shell
if($shell.CreateShortcut($shortcut).TargetPath -ne $target){throw 'Destino do atalho incorreto'}
if($shell.CreateShortcut($shortcut).IconLocation -notlike ((Join-Path $destination 'Litfix-brand-2.0.5.ico')+'*')){throw 'Ícone do atalho incorreto'}
if((Get-FileHash (Join-Path $destination 'Litfix-brand-2.0.5.ico')).Hash -ne (Get-FileHash (Join-Path $PSScriptRoot 'PKAproxy.ico')).Hash){throw 'Ícone instalado diferente do L oficial'}
$sentinel=Join-Path $destination 'arquivo-do-usuario.txt'
[IO.File]::WriteAllText($sentinel,'preservar')
RunSetup
if([IO.File]::ReadAllText($sentinel) -ne 'preservar'){throw 'Reinstalação alterou arquivo do usuário'}
$p=Start-Process (Join-Path $destination 'unins000.exe') -ArgumentList @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART') -WindowStyle Hidden -Wait -PassThru
if($p.ExitCode -ne 0){throw 'Desinstalação falhou'}
if((Test-Path $target) -or (Test-Path $registry) -or (Test-Path $shortcut)){throw 'Desinstalação deixou componentes registrados'}
if([IO.File]::ReadAllText($sentinel) -ne 'preservar'){throw 'Desinstalação apagou arquivo do usuário'}
Write-Output 'PASS: instalação, integridade/assinatura, registro, atalho, reinstalação e desinstalação. Arquivo do usuário preservado. Nenhum proxy ativado.'

using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;
public class MainForm:Form
{
 readonly Color bg=Color.FromArgb(7,12,20),card=Color.FromArgb(16,27,43),blue=Color.FromArgb(37,119,255),light=Color.FromArgb(119,207,255),muted=Color.FromArgb(159,179,205);
 TextBox host=new TextBox(),user=new TextBox(),password=new TextBox(),logs=new TextBox(); NumericUpDown port=new NumericUpDown();
 CheckBox global=new CheckBox(),auto=new CheckBox();CheckedListBox apps=new CheckedListBox();ComboBox launch=new ComboBox();CheckBox socks=new CheckBox(),https=new CheckBox(),udp=new CheckBox(),proxyTls=new CheckBox();TextBox tlsName=new TextBox();ListView appResults=new ListView();string tunnelIp="",activeProtocol="";
 TextBox discordHost=new TextBox(),discordUser=new TextBox(),discordPassword=new TextBox(),discordPath=new TextBox();NumericUpDown discordPort=new NumericUpDown();Label discordState=new Label();Button discordEnable,discordRemove,discordTest,discordBrowse;bool permanentConfigured;
 Panel discordPage=new Panel(),dashboard=new Panel(),applicationPage=new Panel(),testPage=new Panel(),content=new Panel();
 Label state=new Label(),testState=new Label(),exitIp=new Label(),updateState=new Label();Button connect,stop,release,test;
 RoutingOptions route=new RoutingOptions();bool busy,active,preview,healthy=true,checkingHealth;int sessionVersion;DateTime lastHealth=DateTime.UtcNow;Timer monitor=new Timer();
 public MainForm()
 {
  preview=Array.IndexOf(Environment.GetCommandLineArgs(),"--preview")>=0;
  Text="Litfix "+UpdateService.Current;ClientSize=new Size(1080,760);MinimumSize=new Size(1096,799);StartPosition=FormStartPosition.CenterScreen;
  Icon=Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);BackColor=bg;ForeColor=Color.White;Font=new Font(UiFont(),10);AutoScaleMode=AutoScaleMode.Dpi;
  var sidebar=new Panel{Dock=DockStyle.Left,Width=205,BackColor=card};Controls.Add(sidebar);
  sidebar.Controls.Add(Label("Litfix",24,28,155,48,30,light));sidebar.Controls.Add(Label("PROXY LAUNCHER",25,82,165,25,10,Color.White));
  string[] names={"Dashboard","Discord","Aplicativos","Teste"};Panel[] pages={dashboard,discordPage,applicationPage,testPage};
  for(int i=0;i<4;i++){Panel page=pages[i];var b=Button(names[i],20,153+i*56,165,false);b.Click+=(s,e)=>ShowPage(page);sidebar.Controls.Add(b);}
  updateState=Label("Atualização automática\nLushen16/LIT-fix",24,410,165,100,9,light);sidebar.Controls.Add(updateState);
  sidebar.Controls.Add(Label("V"+UpdateService.Current+"  •  WINDOWS x64\nSOCKS5 / HTTPS\n\nTCP / UDP via SOCKS5\nIPv6 bloqueado",24,544,165,130,9,muted));
  content.SetBounds(225,20,835,720);content.Anchor=AnchorStyles.Top|AnchorStyles.Bottom|AnchorStyles.Left|AnchorStyles.Right;Controls.Add(content);
  foreach(Panel page in pages){page.Dock=DockStyle.Fill;page.AutoScroll=true;page.BackColor=bg;content.Controls.Add(page);}
  BuildDashboard();BuildApps();BuildTests();BuildDiscord();ShowPage(dashboard);host.TextChanged+=(s,e)=>InvalidateResults();port.ValueChanged+=(s,e)=>InvalidateResults();user.TextChanged+=(s,e)=>InvalidateResults();password.TextChanged+=(s,e)=>InvalidateResults();
  if(!preview){var dp=DiscordProxy.Load();discordHost.Text=dp.Host;discordPort.Value=dp.Port>0&&dp.Port<=65535?dp.Port:1080;discordUser.Text=dp.User;discordPassword.Text=dp.Password;discordPath.Text=dp.DiscordPath;if(!File.Exists(discordPath.Text)){var found=AppDiscovery.Find(true);if(found.Length>0)discordPath.Text=found[0];}permanentConfigured=DiscordProxy.HasConfiguration;var saved=SessionStore.Load();if(saved!=null){proxyTls.Checked=saved.ProxyTls??true;tlsName.Text=saved.TlsName??"";socks.Checked=saved.Protocol!="HTTPS";https.Checked=saved.Protocol=="HTTPS"||saved.Protocol=="BOTH";host.Text=saved.Host??"";port.Value=saved.Port>0&&saved.Port<=65535?saved.Port:1080;user.Text=saved.User??"";password.Text=saved.Password();route=saved.Route??new RoutingOptions();route.Ipv6=false;udp.Checked=route.Udp;if(route.Apps==null)route.Apps=new System.Collections.Generic.List<RegisteredApp>();if(route.Apps.Count==0&&!String.IsNullOrWhiteSpace(saved.Game)&&File.Exists(saved.Game))route.Apps.Add(new RegisteredApp{Path=saved.Game});}global.Checked=route.Global;auto.Checked=route.AutoLaunch;RefreshApps();}
  monitor.Interval=4000;monitor.Tick+=async(s,e)=>{if(!busy){RefreshState();if(active&&!checkingHealth&&(DateTime.UtcNow-lastHealth).TotalSeconds>=30)await CheckHealth();}};if(!preview)monitor.Start();
  FormClosing+=(s,e)=>{if(busy){e.Cancel=true;return;}if(!preview)Save();};FormClosed+=(s,e)=>monitor.Dispose();RefreshState();
  if(!preview)Shown+=async(s,e)=>await AutoUpdateAsync();
 }
 async Task AutoUpdateAsync()
 {
  if(Array.IndexOf(Environment.GetCommandLineArgs(),"--update-failed")>=0){updateState.Text="Atualização não concluída\nVersão anterior restaurada.";return;}
  if(active||permanentConfigured){updateState.Text="Atualização adiada\nHá um motor ativo.";Log("Atualização será verificada na próxima abertura sem túnel ativo.");return;}
  SetBusy(true);updateState.Text="Buscando atualização…";
  try{
   var update=await Task.Run(()=>UpdateService.Check("Lushen16/LIT-fix"));
   if(update==null){updateState.Text="Versão atualizada\nV"+UpdateService.Current;return;}
   updateState.Text="Baixando V"+update.Manifest.version+"…";Log("Nova versão assinada: "+update.Manifest.version);
   await Task.Run(()=>UpdateService.Fetch(update));
   if(EngineController.Status().StartsWith("Motor ativo")){updateState.Text="Atualização adiada\nHá um motor ativo.";return;}
   updateState.Text="Instalando atualização…";Save();
   await Task.Run(()=>UpdateInstaller.Start(update));
   SetBusy(false);Application.Exit();
  }catch(Exception){updateState.Text="Atualização indisponível\nVersão instalada mantida.";Log("Não foi possível atualizar. A versão instalada foi preservada; nova tentativa na próxima abertura.");}
  finally{if(!IsDisposed)SetBusy(false);}
 }
 void BuildDashboard()
 {
  dashboard.Controls.Add(Label("Sua conexão, sob controle.",0,4,810,48,24,Color.White));dashboard.Controls.Add(Label("Escolha o protocolo e ative a proteção antes de abrir seus aplicativos.",0,59,810,28,10,muted));
  state=Label("Desconectado",20,18,770,38,14,light);dashboard.Controls.Add(Card(0,100,815,70,state));
  var settings=new Panel{Left=0,Top=190,Width=815,Height=263,BackColor=card};dashboard.Controls.Add(settings);
  settings.Controls.Add(Label("Protocolo",20,15,105,25,10,light));socks.Text="SOCKS5";socks.SetBounds(130,12,110,32);socks.Checked=true;https.Text="HTTPS";https.SetBounds(245,12,100,32);settings.Controls.Add(socks);settings.Controls.Add(https);socks.CheckedChanged+=(s,e)=>InvalidateResults();https.CheckedChanged+=(s,e)=>InvalidateResults();udp.Text="Chamadas / UDP (SOCKS5)";udp.SetBounds(350,12,440,32);udp.CheckedChanged+=(s,e)=>{route.Udp=udp.Checked;InvalidateResults();};settings.Controls.Add(udp);Field(settings,"IPv4 do proxy",host,20,58,550);Field(settings,"Usuário",user,20,142,350);Field(settings,"Senha",password,400,142,390);password.UseSystemPasswordChar=true;proxyTls.Text="TLS até a proxy HTTPS";proxyTls.SetBounds(20,216,265,32);proxyTls.Checked=true;proxyTls.CheckedChanged+=(s,e)=>InvalidateResults();settings.Controls.Add(proxyTls);settings.Controls.Add(Label("Nome TLS / certificado (opcional)",300,202,485,22,9,muted));tlsName.SetBounds(300,227,485,28);Style(tlsName);tlsName.TextChanged+=(s,e)=>InvalidateResults();settings.Controls.Add(tlsName);
  settings.Controls.Add(Label("Porta",600,58,185,24,10,muted));port.SetBounds(600,87,185,32);port.Minimum=1;port.Maximum=65535;port.Value=1080;Style(port);settings.Controls.Add(port);
  global.Text="Modo global — todos os aplicativos";global.SetBounds(0,474,815,28);global.ForeColor=light;global.CheckedChanged+=(s,e)=>{route.Global=global.Checked;InvalidateResults();};dashboard.Controls.Add(global);
  auto.Text="Abrir o aplicativo escolhido após ativar e verificar o túnel";auto.SetBounds(0,511,815,28);auto.CheckedChanged+=(s,e)=>route.AutoLaunch=auto.Checked;dashboard.Controls.Add(auto);
  launch.SetBounds(0,550,815,32);launch.DropDownStyle=ComboBoxStyle.DropDownList;Style(launch);dashboard.Controls.Add(launch);
  connect=Button("Ativar proxy",0,602,252,true);connect.Click+=async(s,e)=>await ActivateProxy();dashboard.Controls.Add(connect);
  stop=Button("Parar túnel",277,602,252,false);stop.Click+=async(s,e)=>await Stop();dashboard.Controls.Add(stop);
  release=Button("Liberar rede direta",554,602,261,false);release.Click+=async(s,e)=>await Release();dashboard.Controls.Add(release);
  dashboard.Controls.Add(Label("Parar ou fechar mantém o bloqueio de saída direta. Para retornar à rede normal,\npare o túnel e clique em Liberar rede direta. O modo global também afeta serviços do Windows.",0,660,815,50,9,muted));
 }

 void BuildDiscord()
 {
  discordPage.Controls.Add(Label("Discord",0,4,810,48,24,Color.White));
  discordPage.Controls.Add(Label("Uma proxy exclusiva, mesmo com o Litfix fechado.",0,59,810,30,10,muted));
  discordState=Label("Sem proxy permanente",20,18,770,38,14,light);discordPage.Controls.Add(Card(0,100,815,70,discordState));
  var settings=new Panel{Left=0,Top=190,Width=815,Height=330,BackColor=card};discordPage.Controls.Add(settings);
  settings.Controls.Add(Label("SOCKS5  /  TCP + UDP",20,15,770,28,10,light));
  Field(settings,"IPv4 da proxy",discordHost,20,58,550);
  settings.Controls.Add(Label("Porta",600,58,185,24,10,muted));discordPort.SetBounds(600,87,185,32);discordPort.Minimum=1;discordPort.Maximum=65535;discordPort.Value=1080;Style(discordPort);settings.Controls.Add(discordPort);
  Field(settings,"Usuário",discordUser,20,142,350);Field(settings,"Senha",discordPassword,400,142,385);discordPassword.UseSystemPasswordChar=true;
  Field(settings,"Discord.exe",discordPath,20,224,550);discordPath.ReadOnly=true;
  discordBrowse=Button("Localizar Discord",600,253,185,false);discordBrowse.Click+=(sender,e)=>{using(var picker=new OpenFileDialog{Filter="Discord (Discord.exe)|Discord.exe",CheckFileExists=true})if(picker.ShowDialog(this)==DialogResult.OK)discordPath.Text=picker.FileName;};settings.Controls.Add(discordBrowse);
  discordTest=Button("Testar proxy e UDP",0,546,250,false);discordTest.Click+=async(sender,e)=>await TestDiscord();discordPage.Controls.Add(discordTest);
  discordEnable=Button("Ativar permanente",277,546,250,true);discordEnable.Click+=async(sender,e)=>await EnableDiscord();discordPage.Controls.Add(discordEnable);
  discordRemove=Button("Remover configuração",554,546,261,false);discordRemove.Click+=async(sender,e)=>await RemoveDiscord();discordPage.Controls.Add(discordRemove);
  discordPage.Controls.Add(Label("Inicia ao entrar no Windows e reconecta em segundo plano. Feche o Discord antes de ativar.\nA proxy precisa aceitar UDP para chamadas. A configuração permanece até removê-la\nou desinstalar o Litfix. O túnel geral e o modo permanente usam o mesmo motor.",0,612,815,92,10,muted));
 }
 DiscordProfile DiscordSettings(){return new DiscordProfile{Host=discordHost.Text.Trim(),Port=(int)discordPort.Value,User=discordUser.Text,Password=discordPassword.Text,DiscordPath=discordPath.Text};}
 async Task TestDiscord()
 {
  if(busy)return;SetBusy(true);try{var profile=DiscordSettings();DiscordProxy.Validate(profile);discordState.Text="Testando autenticação e UDP…";
   string ip=await Task.Run(()=>{string result=ProxyDiagnostics.ProxyIp(profile.Host,profile.Port,profile.User,profile.Password);ProxyDiagnostics.CheckUdp(profile.Host,profile.Port,profile.User,profile.Password,"SOCKS5");return result;});discordState.Text="TCP + UDP OK • IP de saída: "+ip;
  }catch(Exception ex){discordState.Text="Proxy não confirmou TCP / UDP";Error(ex);}finally{SetBusy(false);}
 }
 async Task EnableDiscord()
 {
  if(busy)return;SetBusy(true);try{if(active||NetworkGuard.ArmedState==true)throw new InvalidOperationException("Pare o túnel geral e use Liberar rede direta antes de ativar o Discord permanente.");
   var profile=DiscordSettings();DiscordProxy.Validate(profile);EnsureAppsClosed(new[]{profile.DiscordPath});DiscordProxy.Save(profile);discordState.Text="Configurando início automático… permita o aviso do Windows";
   await Task.Run(()=>DiscordProxy.Elevated(true));permanentConfigured=DiscordProxy.HasConfiguration;Log("Proxy permanente do Discord configurada. A janela pode ser fechada.");
  }catch(Exception ex){permanentConfigured=DiscordProxy.HasConfiguration;Error(ex);}finally{SetBusy(false);RefreshState();}
 }
 async Task RemoveDiscord()
 {
  if(busy)return;SetBusy(true);try{discordState.Text="Removendo configuração permanente…";await Task.Run(()=>DiscordProxy.Elevated(false));permanentConfigured=DiscordProxy.HasConfiguration;discordPassword.Clear();Log("Configuração permanente removida; rede do Discord liberada.");}
  catch(Exception ex){permanentConfigured=DiscordProxy.HasConfiguration;Error(ex);}finally{SetBusy(false);RefreshState();}
 }
 void BuildApps()
 {
  applicationPage.Controls.Add(Label("Aplicativos",0,4,800,48,24,Color.White));applicationPage.Controls.Add(Label("Cadastre os executáveis e marque os que devem usar a proxy.",0,59,800,30,10,muted));
  var discord=Button("Adicionar Discord",0,108,250,true);discord.Click+=(s,e)=>AddDetected(true);applicationPage.Controls.Add(discord);
  var pka=Button("Adicionar PKA",277,108,250,true);pka.Click+=(s,e)=>AddDetected(false);applicationPage.Controls.Add(pka);
  apps.SetBounds(0,170,815,290);apps.CheckOnClick=true;apps.BackColor=card;apps.ForeColor=Color.White;apps.BorderStyle=BorderStyle.None;apps.HorizontalScrollbar=true;applicationPage.Controls.Add(apps);
  apps.ItemCheck+=(s,e)=>{if(e.Index<route.Apps.Count)route.Apps[e.Index].Selected=e.NewValue==CheckState.Checked;BeginInvoke(new System.Action(()=>{InvalidateResults();RefreshLaunch();}));};
  var add=Button("Adicionar .exe",0,483,250,true);add.Click+=(s,e)=>{using(var picker=new OpenFileDialog{Filter="Executáveis Windows (*.exe)|*.exe",Multiselect=true,CheckFileExists=true})if(picker.ShowDialog(this)==DialogResult.OK){foreach(string p in picker.FileNames){string path=Path.GetFullPath(p);if(!route.Apps.Exists(a=>String.Equals(a.Path,path,StringComparison.OrdinalIgnoreCase)))route.Apps.Add(new RegisteredApp{Path=path});}RefreshApps();Save();}};applicationPage.Controls.Add(add);
  var remove=Button("Remover selecionado",277,483,250,false);remove.Click+=(s,e)=>{if(apps.SelectedIndex>=0){route.Apps.RemoveAt(apps.SelectedIndex);RefreshApps();Save();}};applicationPage.Controls.Add(remove);
  var open=Button("Abrir pelo túnel",554,483,261,false);open.Click+=(s,e)=>{try{if(!active||busy)throw new InvalidOperationException("Ative o túnel primeiro.");if(apps.SelectedIndex<0)throw new InvalidOperationException("Escolha um aplicativo na lista.");var a=route.Apps[apps.SelectedIndex];if(!route.Global&&!a.Selected)throw new InvalidOperationException("Marque este aplicativo e reconecte antes de abrir.");GameLauncher.Open(a.Path,"");Log("Aplicativo aberto: "+Path.GetFileName(a.Path));}catch(Exception ex){Error(ex);}};applicationPage.Controls.Add(open);
  applicationPage.Controls.Add(Label("Cadastre também os .exe auxiliares e o cliente final aberto por um launcher.\nFeche os apps antes de ativar: conexões já existentes não são migradas.\nPor aplicativo, os demais processos continuam diretos; DNS na porta 53 usa o túnel.\nA seleção não se estende automaticamente a processos filhos ou serviços compartilhados.",0,554,815,110,10,muted));
 }
 void AddDetected(bool discord)
 {
  try{
   if(active||busy)throw new InvalidOperationException("Pare o túnel antes de alterar os aplicativos protegidos.");
   string[] paths=AppDiscovery.Find(discord);
   if(paths.Length==0)throw new InvalidOperationException((discord?"Discord":"PokeAlliance")+" não encontrado nos caminhos padrão. Instale o aplicativo ou use Adicionar .exe.");
   foreach(string path in paths){var app=route.Apps.Find(a=>String.Equals(a.Path,path,StringComparison.OrdinalIgnoreCase));if(app==null)route.Apps.Add(new RegisteredApp{Path=path,Selected=true});else app.Selected=true;}
   if(discord)udp.Checked=true;RefreshApps();Save();Log((discord?"Discord":"PokeAlliance")+": "+paths.Length+" executável(is) localizado(s) e marcado(s). Feche o aplicativo e ative o túnel antes de abrir.");
  }catch(Exception ex){Error(ex);}
 }
 void BuildTests()
 {
  testPage.Controls.Add(Label("Teste e diagnóstico",0,4,810,48,24,Color.White));testPage.Controls.Add(Label("Resultados reais da negociação da proxy e de uma consulta HTTPS pelo túnel.",0,59,815,28,10,muted));
  testState=Label("Nenhum teste executado",20,18,760,45,13,light);exitIp=Label("IP público de saída: —",20,75,760,30,11,Color.White);testPage.Controls.Add(Card(0,114,815,128,testState,exitIp));
  test=Button("Testar proxy",0,264,250,true);test.Click+=async(s,e)=>await TestProxy();testPage.Controls.Add(test);
  var routed=Button("Testar túnel",277,264,250,false);routed.Click+=async(s,e)=>await TestTunnel();testPage.Controls.Add(routed);
  var diagnostic=Button("Abrir log do motor",554,264,261,false);diagnostic.Click+=(s,e)=>{string path=Path.Combine(permanentConfigured?DiscordProxy.PrivateRoot:AppPaths.Root,"tun-diagnostic.log");if(File.Exists(path))Process.Start(new ProcessStartInfo("notepad.exe","\""+path+"\""){UseShellExecute=true});else Log("O motor ainda não gerou um log.");};testPage.Controls.Add(diagnostic);
  appResults.SetBounds(0,328,815,210);appResults.View=View.Details;appResults.FullRowSelect=true;appResults.GridLines=false;appResults.BackColor=card;appResults.ForeColor=Color.White;appResults.BorderStyle=BorderStyle.None;appResults.Columns.Add("Aplicativo",190);appResults.Columns.Add("IP de saída do túnel",180);appResults.Columns.Add("Verificação",420);testPage.Controls.Add(appResults);logs.SetBounds(0,556,815,143);logs.Multiline=true;logs.ReadOnly=true;logs.ScrollBars=ScrollBars.Vertical;logs.BackColor=card;logs.ForeColor=light;logs.Font=new Font(UiFont(),10);testPage.Controls.Add(logs);

 }
 async Task ActivateProxy()
 {
  if(busy)return;SetBusy(true);try {
   if(permanentConfigured||DiscordProxy.HasConfiguration)throw new InvalidOperationException("Remova a proxy permanente na aba Discord antes de ativar o túnel geral.");
   if(active)throw new InvalidOperationException("Pare o túnel antes de alterar a configuração.");
   if(NetworkGuard.ArmedState==true)throw new InvalidOperationException("O bloqueio anterior permanece ativo. Libere a rede direta antes de criar uma nova sessão.");
   route.Global=global.Checked;route.AutoLaunch=auto.Checked;if(!route.Udp&&route.Apps.Exists(a=>a.Selected&&String.Equals(Path.GetFileName(a.Path),"Discord.exe",StringComparison.OrdinalIgnoreCase)))throw new InvalidOperationException("Marque Chamadas / UDP e use SOCKS5 com UDP para proteger o Discord. HTTPS CONNECT não transporta chamadas.");
   var request=new EngineRequest{Protocol=await ChooseProtocol(),ProxyTls=proxyTls.Checked,TlsName=tlsName.Text.Trim(),Route=route.Copy(),Host=host.Text.Trim(),Port=(int)port.Value,User=user.Text,Password=password.Text,OwnerPath=Process.GetCurrentProcess().MainModule.FileName};
   TunConfiguration.Build(request);EnsureAppsClosed(request.Route.SelectedPaths());
   string chosen=launch.SelectedItem==null?null:((RegisteredApp)launch.SelectedItem).Path;
   if(route.AutoLaunch&&chosen==null)throw new InvalidOperationException("Escolha um aplicativo marcado para abrir automaticamente.");
   Save();state.Text="Ativando e verificando… permita o aviso do Windows";activeProtocol=request.Protocol;tunnelIp="";RefreshAppResults();Log("Validando "+request.Protocol+" e instalando proteção persistente.");
   await Task.Run(()=>EngineController.RunElevated(request,false));active=true;healthy=true;sessionVersion++;lastHealth=DateTime.UtcNow;Log("Túnel ativo; teste HTTPS do processo protegido concluído.");
   if(request.Route.AutoLaunch){GameLauncher.Open(chosen,"");Log("Aplicativo aberto: "+Path.GetFileName(chosen));}
  }catch(Exception ex){Error(ex);}finally{SetBusy(false);RefreshState();}
 }
 async Task Stop(){if(busy)return;SetBusy(true);try{sessionVersion++;tunnelIp="";RefreshAppResults();await Task.Run(()=>EngineController.RunElevated(null,true));Log("Túnel parado. Bloqueio de saída direta mantido.");}catch(Exception ex){Error(ex);}finally{SetBusy(false);RefreshState();}}
 async Task Release(){if(busy)return;SetBusy(true);try{if(active)throw new InvalidOperationException("Pare o túnel antes de liberar a rede direta.");await Task.Run(()=>EngineController.ReleaseGuard());tunnelIp="";RefreshAppResults();Log("Bloqueio removido. A rede direta está liberada.");}catch(Exception ex){Error(ex);}finally{SetBusy(false);RefreshState();}}
 string Selection(){if(!socks.Checked&&!https.Checked)throw new ArgumentException("Marque SOCKS5, HTTPS ou ambas.");return socks.Checked?(https.Checked?"BOTH":"SOCKS5"):"HTTPS";}
 async Task<string> ChooseProtocol()
 {
  string selection=Selection();if(selection!="BOTH")return selection;
  string h=host.Text.Trim(),u=user.Text,p=password.Text;int n=(int)port.Value;bool needsUdp=udp.Checked,secure=proxyTls.Checked;string certificate=tlsName.Text.Trim();
  try{await Task.Run(()=>{ProxyDiagnostics.ProxyIp(h,n,u,p,"SOCKS5");if(needsUdp)ProxyDiagnostics.CheckUdp(h,n,u,p,"SOCKS5");});Log("Ambas selecionadas: SOCKS5 validada e escolhida.");return "SOCKS5";}
  catch(Exception ex){Log("SOCKS5 indisponível: "+ex.Message);if(needsUdp)throw new InvalidOperationException("A SOCKS5 não confirmou UDP. HTTPS não pode substituí-la para chamadas Discord.");}
  await Task.Run(()=>ProxyDiagnostics.ProxyIp(h,n,u,p,"HTTPS",secure,certificate));Log("Ambas selecionadas: HTTPS validada e escolhida.");return "HTTPS";
 }
 async Task TestProxy()
 {
  if(busy)return;SetBusy(true);testState.Text="Testando protocolos selecionados…";
  try{
   string selection=Selection(),h=host.Text.Trim(),u=user.Text,p=password.Text;int n=(int)port.Value;bool needsUdp=udp.Checked,secure=proxyTls.Checked;string certificate=tlsName.Text.Trim();int ok=0,total=selection=="BOTH"?2:1;
   string summary="";RoutingConfiguration.ValidateProxy(h,n,u,p);
   foreach(string kind in selection=="BOTH"?new[]{"SOCKS5","HTTPS"}:new[]{selection}){
    try{string ip=await Task.Run(()=>ProxyDiagnostics.ProxyIp(h,n,u,p,kind,secure,certificate));string status="TCP OK";
     if(needsUdp&&kind=="SOCKS5"){await Task.Run(()=>ProxyDiagnostics.CheckUdp(h,n,u,p,kind));status="TCP + UDP OK";}
     if(needsUdp&&kind=="HTTPS")status="TCP OK; sem suporte a chamadas UDP";
     summary+=(summary.Length==0?"":"  |  ")+kind+": "+ip;Log(kind+": "+status+" / IP "+ip);ok++;
    }catch(Exception ex){Log(kind+": "+ex.Message);}
   }
   testState.Text=ok+" de "+total+" protocolos verificados";exitIp.Text=summary.Length==0?"IP de saída: indisponível":summary;RefreshAppResults();
  }catch(Exception ex){testState.Text="Falha na proxy";exitIp.Text="IP de saída: indisponível";Error(ex);}finally{SetBusy(false);}
 }
 void InvalidateResults(){if(active)return;tunnelIp="";if(appResults!=null)RefreshAppResults();if(exitIp!=null)exitIp.Text="IP público de saída: —";}
 void RefreshAppResults()
 {
  appResults.Items.Clear();if(permanentConfigured){var row=new ListViewItem("Discord");row.SubItems.Add(tunnelIp.Length>0?tunnelIp:"—");row.SubItems.Add(tunnelIp.Length>0?"Túnel Discord OK; app não medido":"Aguardando teste do túnel Discord");row.ToolTipText="IP do processo de diagnóstico na rota permanente do Discord.";appResults.Items.Add(row);appResults.ShowItemToolTips=true;return;}foreach(var a in route.Apps){if(!route.Global&&!a.Selected)continue;
   string status=tunnelIp.Length>0?"Túnel OK; app não medido / "+activeProtocol:"Aguardando teste do túnel";
   var item=new ListViewItem(Path.GetFileNameWithoutExtension(a.Path));item.SubItems.Add(tunnelIp.Length>0?tunnelIp:"—");item.SubItems.Add(status);item.ToolTipText="IP medido pelo processo de diagnóstico na mesma rota. O tráfego deste aplicativo não foi medido individualmente.";appResults.Items.Add(item);
  }
  appResults.ShowItemToolTips=true;
 }

 async Task TestTunnel(){if(busy)return;SetBusy(true);try{if(!active)throw new InvalidOperationException("Ative o túnel primeiro.");string ip=await Task.Run(()=>EngineController.RoutedIp());healthy=true;tunnelIp=ip;RefreshAppResults();testState.Text="IP do túnel verificado para a rota selecionada";exitIp.Text="IP público pelo túnel: "+ip;Log(testState.Text+" / "+ip);}catch(Exception ex){healthy=false;tunnelIp="";RefreshAppResults();testState.Text="Túnel não confirmado";exitIp.Text="IP público pelo túnel: indisponível";Error(ex);}finally{SetBusy(false);}}
 async Task CheckHealth(){checkingHealth=true;lastHealth=DateTime.UtcNow;int version=sessionVersion;bool ok=false;try{await Task.Run(()=>EngineController.RoutedIp());ok=true;}catch{}finally{checkingHealth=false;}if(!IsDisposed&&active&&version==sessionVersion){if(healthy!=ok)Log(ok?"Conectividade do processo protegido restabelecida.":"Erro de conectividade do processo protegido. Sem fallback direto.");healthy=ok;RefreshState();}}
 void EnsureAppsClosed(string[] paths){foreach(string path in paths)foreach(Process p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(path))){using(p){try{if(String.Equals(p.MainModule.FileName,path,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Feche "+Path.GetFileName(path)+" antes de ativar.");}catch(System.ComponentModel.Win32Exception){throw new InvalidOperationException("Não foi possível verificar um processo aberto. Feche "+Path.GetFileName(path)+" antes de ativar.");}}}}
 void SetBusy(bool value){busy=value;connect.Enabled=stop.Enabled=release.Enabled=!value&&!permanentConfigured;test.Enabled=!value;discordEnable.Enabled=!value&&!permanentConfigured;discordRemove.Enabled=!value&&permanentConfigured;discordTest.Enabled=!value;discordHost.Enabled=discordUser.Enabled=discordPassword.Enabled=discordPath.Enabled=discordPort.Enabled=discordBrowse.Enabled=!value&&!permanentConfigured;proxyTls.Enabled=tlsName.Enabled=socks.Enabled=https.Enabled=udp.Enabled=global.Enabled=auto.Enabled=host.Enabled=port.Enabled=user.Enabled=password.Enabled=launch.Enabled=!value&&!active;applicationPage.Enabled=!value;apps.Enabled=!value&&!active;foreach(Control c in applicationPage.Controls)if(c is Button&&c.Text!="Abrir pelo túnel")c.Enabled=!value&&!active;}
 void RefreshState(){bool wasActive=active;active=!preview&&EngineController.Status().StartsWith("Motor ativo");bool? guarded=preview?(bool?)false:NetworkGuard.ArmedState;state.Text=active?(healthy?"Conectado • TCP / IPv4 • monitoramento HTTPS ativo":"Erro de conectividade • motor ativo • saída direta bloqueada"):!guarded.HasValue?"Proteção não consultada • ativação exigirá administrador":guarded.Value?"Túnel parado • saída direta bloqueada":"Desconectado • rede direta liberada";if(active&&EngineController.Mode()=="DISCORD")state.Text="Discord • proxy permanente ativa em segundo plano";discordState.Text=permanentConfigured?(active?"Proxy permanente configurada • motor ativo":"Proxy permanente configurada • aguardando reconexão"):"Sem proxy permanente";if(wasActive&&!active){tunnelIp="";RefreshAppResults();}if(wasActive&&!active)Log("Motor desconectado. A proteção persistente permanece ativa.");SetBusy(busy);}
 void RefreshApps(){tunnelIp="";RefreshAppResults();apps.Items.Clear();foreach(var a in route.Apps)apps.Items.Add(a,a.Selected);RefreshLaunch();}
 void RefreshLaunch(){string previous=launch.SelectedItem==null?null:((RegisteredApp)launch.SelectedItem).Path;launch.Items.Clear();foreach(var a in route.Apps)if(a.Selected)launch.Items.Add(a);for(int i=0;i<launch.Items.Count;i++)if(((RegisteredApp)launch.Items[i]).Path==previous)launch.SelectedIndex=i;if(launch.SelectedIndex<0&&launch.Items.Count>0)launch.SelectedIndex=0;}
 void Save(){if(preview)return;try{SessionStore.Save("",host.Text.Trim(),(int)port.Value,user.Text,password.Text,route,Selection(),proxyTls.Checked,tlsName.Text.Trim());}catch{Log("Não foi possível salvar o perfil protegido.");}}
 void Error(Exception ex){string text=ex is AggregateException?"Falha de conexão TCP com o proxy.":ex.Message;Log("ERRO: "+text);MessageBox.Show(this,text,"Litfix",MessageBoxButtons.OK,MessageBoxIcon.Error);}
 void Log(string value){if(discordUser.Text.Length>0)value=value.Replace(discordUser.Text,"[usuario-discord]");if(discordPassword.Text.Length>0)value=value.Replace(discordPassword.Text,"[senha-discord]");if(user.Text.Length>0)value=value.Replace(user.Text,"[usuario]");if(password.Text.Length>0)value=value.Replace(password.Text,"[senha]");if(logs.TextLength>24000)logs.Clear();logs.AppendText(DateTime.Now.ToString("HH:mm:ss")+"  "+value+Environment.NewLine);}
 void ShowPage(Panel page){foreach(Control child in content.Controls)child.Visible=child==page;page.BringToFront();}
 public void PreviewPage(string name){if(name=="tests"){route.Apps.Add(new RegisteredApp{Path=@"C:\Apps\Discord.exe"});route.Apps.Add(new RegisteredApp{Path=@"C:\Apps\PokeAlliance.exe"});RefreshAppResults();}ShowPage(name=="discord"?discordPage:name=="apps"?applicationPage:name=="tests"?testPage:dashboard);}
 static string UiFont(){foreach(var f in FontFamily.Families)if(f.Name=="Segoe UI Variable Text")return f.Name;foreach(var f in FontFamily.Families)if(f.Name=="Bahnschrift")return f.Name;return "Segoe UI";}
 Label Label(string t,int x,int y,int w,int h,int size,Color color){return new Label{Text=t,Left=x,Top=y,Width=w,Height=h,ForeColor=color,Font=new Font(UiFont(),size)};}
 Panel Card(int x,int y,int w,int h,params Control[] children){var p=new Panel{Left=x,Top=y,Width=w,Height=h,BackColor=card};p.Controls.AddRange(children);return p;}
 Button Button(string text,int x,int y,int w,bool primary){var b=new Button{Text=text,Left=x,Top=y,Width=w,Height=42,BackColor=primary?blue:card,ForeColor=Color.White,FlatStyle=FlatStyle.Flat,Cursor=Cursors.Hand,Font=new Font(UiFont(),10,FontStyle.Bold)};b.FlatAppearance.BorderColor=blue;b.FlatAppearance.BorderSize=primary?0:1;return b;}
 void Style(Control c){c.BackColor=bg;c.ForeColor=Color.White;c.Font=new Font(UiFont(),11);}
 void Field(Panel p,string title,TextBox input,int x,int y,int width){p.Controls.Add(Label(title,x,y,width,25,10,muted));input.SetBounds(x,y+29,width,32);Style(input);p.Controls.Add(input);}
}

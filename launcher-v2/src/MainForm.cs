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
 CheckBox global=new CheckBox(),auto=new CheckBox();CheckedListBox apps=new CheckedListBox();ComboBox launch=new ComboBox();
 Panel dashboard=new Panel(),applicationPage=new Panel(),testPage=new Panel(),content=new Panel();
 Label state=new Label(),testState=new Label(),exitIp=new Label(),updateState=new Label();Button connect,stop,release,test;
 RoutingOptions route=new RoutingOptions();bool busy,active,preview,healthy=true,checkingHealth;int sessionVersion;DateTime lastHealth=DateTime.UtcNow;Timer monitor=new Timer();
 public MainForm()
 {
  preview=Array.IndexOf(Environment.GetCommandLineArgs(),"--preview")>=0;
  Text="Litfix "+UpdateService.Current;ClientSize=new Size(1080,760);MinimumSize=new Size(1096,799);StartPosition=FormStartPosition.CenterScreen;
  Icon=Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);BackColor=bg;ForeColor=Color.White;Font=new Font("Segoe UI",10);AutoScaleMode=AutoScaleMode.Dpi;
  var sidebar=new Panel{Dock=DockStyle.Left,Width=205,BackColor=card};Controls.Add(sidebar);
  sidebar.Controls.Add(Label("Litfix",24,28,155,48,30,light));sidebar.Controls.Add(Label("PROXY LAUNCHER",25,82,165,25,10,Color.White));
  string[] names={"Dashboard","Aplicativos","Teste"};Panel[] pages={dashboard,applicationPage,testPage};
  for(int i=0;i<3;i++){Panel page=pages[i];var b=Button(names[i],20,153+i*56,165,false);b.Click+=(s,e)=>ShowPage(page);sidebar.Controls.Add(b);}
  updateState=Label("Atualização automática\nLushen16/pka-proxy",24,370,165,115,9,light);sidebar.Controls.Add(updateState);
  sidebar.Controls.Add(Label("V"+UpdateService.Current+"  •  WINDOWS x64\nSOCKS5 / WEBSHARE\n\nTCP protegido\nUDP e IPv6 bloqueados",24,544,165,130,9,muted));
  content.SetBounds(225,20,835,720);content.Anchor=AnchorStyles.Top|AnchorStyles.Bottom|AnchorStyles.Left|AnchorStyles.Right;Controls.Add(content);
  foreach(Panel page in pages){page.Dock=DockStyle.Fill;page.AutoScroll=true;page.BackColor=bg;content.Controls.Add(page);}
  BuildDashboard();BuildApps();BuildTests();ShowPage(dashboard);
  if(!preview){var saved=SessionStore.Load();if(saved!=null){host.Text=saved.Host??"";port.Value=saved.Port>0&&saved.Port<=65535?saved.Port:1080;user.Text=saved.User??"";password.Text=saved.Password();route=saved.Route??new RoutingOptions();route.Udp=route.Ipv6=false;if(route.Apps==null)route.Apps=new System.Collections.Generic.List<RegisteredApp>();if(route.Apps.Count==0&&!String.IsNullOrWhiteSpace(saved.Game)&&File.Exists(saved.Game))route.Apps.Add(new RegisteredApp{Path=saved.Game});}global.Checked=route.Global;auto.Checked=route.AutoLaunch;RefreshApps();}
  monitor.Interval=4000;monitor.Tick+=async(s,e)=>{if(!busy){RefreshState();if(active&&!checkingHealth&&(DateTime.UtcNow-lastHealth).TotalSeconds>=30)await CheckHealth();}};if(!preview)monitor.Start();
  FormClosing+=(s,e)=>{if(busy){e.Cancel=true;return;}if(!preview)Save();};FormClosed+=(s,e)=>monitor.Dispose();RefreshState();
  if(!preview)Shown+=async(s,e)=>await AutoUpdateAsync();
 }
 async Task AutoUpdateAsync()
 {
  if(Array.IndexOf(Environment.GetCommandLineArgs(),"--update-failed")>=0){updateState.Text="Atualização não concluída\nVersão anterior restaurada.";return;}
  if(active){updateState.Text="Atualização adiada\nHá um motor ativo.";Log("Atualização será verificada na próxima abertura sem túnel ativo.");return;}
  SetBusy(true);updateState.Text="Buscando atualização…";
  try{
   var update=await Task.Run(()=>UpdateService.Check("Lushen16/pka-proxy"));
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
  dashboard.Controls.Add(Label("Sua conexão, sob controle.",0,4,810,48,24,Color.White));dashboard.Controls.Add(Label("Configure o SOCKS5 e ative a proteção antes de abrir seus aplicativos.",0,59,810,28,10,muted));
  state=Label("Desconectado",20,18,770,38,14,light);dashboard.Controls.Add(Card(0,100,815,70,state));
  var settings=new Panel{Left=0,Top=190,Width=815,Height=263,BackColor=card};dashboard.Controls.Add(settings);
  settings.Controls.Add(Label("WEBSHARE  /  SOCKS5",20,15,750,25,10,light));Field(settings,"IPv4 do proxy",host,20,58,550);Field(settings,"Usuário",user,20,142,350);Field(settings,"Senha",password,400,142,390);password.UseSystemPasswordChar=true;
  settings.Controls.Add(Label("Porta",600,58,185,24,10,muted));port.SetBounds(600,87,185,32);port.Minimum=1;port.Maximum=65535;port.Value=1080;Style(port);settings.Controls.Add(port);
  global.Text="Modo global — todos os aplicativos";global.SetBounds(0,474,815,28);global.ForeColor=light;global.CheckedChanged+=(s,e)=>{route.Global=global.Checked;};dashboard.Controls.Add(global);
  auto.Text="Abrir o aplicativo escolhido após ativar e verificar o túnel";auto.SetBounds(0,511,815,28);auto.CheckedChanged+=(s,e)=>route.AutoLaunch=auto.Checked;dashboard.Controls.Add(auto);
  launch.SetBounds(0,550,815,32);launch.DropDownStyle=ComboBoxStyle.DropDownList;Style(launch);dashboard.Controls.Add(launch);
  connect=Button("Ativar proxy",0,602,252,true);connect.Click+=async(s,e)=>await ActivateProxy();dashboard.Controls.Add(connect);
  stop=Button("Parar túnel",277,602,252,false);stop.Click+=async(s,e)=>await Stop();dashboard.Controls.Add(stop);
  release=Button("Liberar rede direta",554,602,261,false);release.Click+=async(s,e)=>await Release();dashboard.Controls.Add(release);
  dashboard.Controls.Add(Label("Parar ou fechar mantém o bloqueio de saída direta. Para retornar à rede normal,\npare o túnel e clique em Liberar rede direta. O modo global também afeta serviços do Windows.",0,660,815,50,9,muted));
 }
 void BuildApps()
 {
  applicationPage.Controls.Add(Label("Aplicativos",0,4,800,48,24,Color.White));applicationPage.Controls.Add(Label("Cadastre os executáveis e marque os que devem usar a proxy.",0,59,800,30,10,muted));
  apps.SetBounds(0,115,815,345);apps.CheckOnClick=true;apps.BackColor=card;apps.ForeColor=Color.White;apps.BorderStyle=BorderStyle.None;apps.HorizontalScrollbar=true;applicationPage.Controls.Add(apps);
  apps.ItemCheck+=(s,e)=>{if(e.Index<route.Apps.Count)route.Apps[e.Index].Selected=e.NewValue==CheckState.Checked;BeginInvoke(new System.Action(RefreshLaunch));};
  var add=Button("Adicionar .exe",0,483,250,true);add.Click+=(s,e)=>{using(var picker=new OpenFileDialog{Filter="Executáveis Windows (*.exe)|*.exe",Multiselect=true,CheckFileExists=true})if(picker.ShowDialog(this)==DialogResult.OK){foreach(string p in picker.FileNames){string path=Path.GetFullPath(p);if(!route.Apps.Exists(a=>String.Equals(a.Path,path,StringComparison.OrdinalIgnoreCase)))route.Apps.Add(new RegisteredApp{Path=path});}RefreshApps();Save();}};applicationPage.Controls.Add(add);
  var remove=Button("Remover selecionado",277,483,250,false);remove.Click+=(s,e)=>{if(apps.SelectedIndex>=0){route.Apps.RemoveAt(apps.SelectedIndex);RefreshApps();Save();}};applicationPage.Controls.Add(remove);
  var open=Button("Abrir pelo túnel",554,483,261,false);open.Click+=(s,e)=>{try{if(!active||busy)throw new InvalidOperationException("Ative o túnel primeiro.");if(apps.SelectedIndex<0)throw new InvalidOperationException("Escolha um aplicativo na lista.");var a=route.Apps[apps.SelectedIndex];if(!route.Global&&!a.Selected)throw new InvalidOperationException("Marque este aplicativo e reconecte antes de abrir.");GameLauncher.Open(a.Path,"");Log("Aplicativo aberto: "+Path.GetFileName(a.Path));}catch(Exception ex){Error(ex);}};applicationPage.Controls.Add(open);
  applicationPage.Controls.Add(Label("Cadastre também os .exe auxiliares e o cliente final aberto por um launcher.\nFeche os apps antes de ativar: conexões já existentes não são migradas.\nPor aplicativo, os demais processos continuam diretos; DNS na porta 53 usa o túnel.\nA seleção não se estende automaticamente a processos filhos ou serviços compartilhados.",0,554,815,110,10,muted));
 }
 void BuildTests()
 {
  testPage.Controls.Add(Label("Teste e diagnóstico",0,4,810,48,24,Color.White));testPage.Controls.Add(Label("Resultados reais da negociação SOCKS5 e de uma consulta HTTPS pelo túnel.",0,59,815,28,10,muted));
  testState=Label("Nenhum teste executado",20,18,760,45,13,light);exitIp=Label("IP público de saída: —",20,75,760,30,11,Color.White);testPage.Controls.Add(Card(0,114,815,128,testState,exitIp));
  test=Button("Testar SOCKS5",0,264,250,true);test.Click+=async(s,e)=>await TestProxy();testPage.Controls.Add(test);
  var routed=Button("Testar túnel",277,264,250,false);routed.Click+=async(s,e)=>await TestTunnel();testPage.Controls.Add(routed);
  var diagnostic=Button("Abrir log do motor",554,264,261,false);diagnostic.Click+=(s,e)=>{string path=Path.Combine(AppPaths.Root,"tun-diagnostic.log");if(File.Exists(path))Process.Start(new ProcessStartInfo("notepad.exe","\""+path+"\""){UseShellExecute=true});else Log("O motor ainda não gerou um log.");};testPage.Controls.Add(diagnostic);
  logs.SetBounds(0,328,815,252);logs.Multiline=true;logs.ReadOnly=true;logs.ScrollBars=ScrollBars.Vertical;logs.BackColor=card;logs.ForeColor=light;logs.Font=new Font("Consolas",10);testPage.Controls.Add(logs);
  testPage.Controls.Add(Label("Teste SOCKS5: TCP, método de autenticação, CONNECT, TLS e IP de saída.\nTeste túnel: outro executável protegido realiza HTTPS. Não comprova o tráfego do jogo.\nUDP/QUIC e IPv6 são bloqueados; proxy SOCKS5 não cifra a autenticação no enlace.\nNão há tentativa automática de usar a conexão direta quando um teste falha.",0,608,815,104,10,muted));
 }
 async Task ActivateProxy()
 {
  if(busy)return;SetBusy(true);try {
   if(active)throw new InvalidOperationException("Pare o túnel antes de alterar a configuração.");
   if(NetworkGuard.ArmedState==true)throw new InvalidOperationException("O bloqueio anterior permanece ativo. Libere a rede direta antes de criar uma nova sessão.");
   route.Global=global.Checked;route.AutoLaunch=auto.Checked;
   var request=new EngineRequest{Route=route.Copy(),Host=host.Text.Trim(),Port=(int)port.Value,User=user.Text,Password=password.Text,OwnerPath=Process.GetCurrentProcess().MainModule.FileName};
   TunConfiguration.Build(request);EnsureAppsClosed(request.Route.SelectedPaths());
   string chosen=launch.SelectedItem==null?null:((RegisteredApp)launch.SelectedItem).Path;
   if(route.AutoLaunch&&chosen==null)throw new InvalidOperationException("Escolha um aplicativo marcado para abrir automaticamente.");
   Save();state.Text="Ativando e verificando… permita o aviso do Windows";Log("Validando SOCKS5 e instalando proteção persistente.");
   await Task.Run(()=>EngineController.RunElevated(request,false));active=true;healthy=true;sessionVersion++;lastHealth=DateTime.UtcNow;Log("Túnel ativo; teste HTTPS do processo protegido concluído.");
   if(request.Route.AutoLaunch){GameLauncher.Open(chosen,"");Log("Aplicativo aberto: "+Path.GetFileName(chosen));}
  }catch(Exception ex){Error(ex);}finally{SetBusy(false);RefreshState();}
 }
 async Task Stop(){if(busy)return;SetBusy(true);try{sessionVersion++;await Task.Run(()=>EngineController.RunElevated(null,true));Log("Túnel parado. Bloqueio de saída direta mantido.");}catch(Exception ex){Error(ex);}finally{SetBusy(false);RefreshState();}}
 async Task Release(){if(busy)return;SetBusy(true);try{if(active)throw new InvalidOperationException("Pare o túnel antes de liberar a rede direta.");await Task.Run(()=>EngineController.ReleaseGuard());Log("Bloqueio removido. A rede direta está liberada.");}catch(Exception ex){Error(ex);}finally{SetBusy(false);RefreshState();}}
 async Task TestProxy(){if(busy)return;SetBusy(true);testState.Text="Testando TCP / autenticação / HTTPS…";try{string h=host.Text.Trim(),u=user.Text,p=password.Text;int n=(int)port.Value;RoutingConfiguration.ValidateProxy(h,n,u,p);System.Net.IPAddress address;if(!System.Net.IPAddress.TryParse(h,out address)||address.AddressFamily!=System.Net.Sockets.AddressFamily.InterNetwork)throw new ArgumentException("Informe o IPv4 da proxy.");string ip=await Task.Run(()=>ProxyDiagnostics.ProxyIp(h,n,u,p));testState.Text="SOCKS5, autenticação e HTTPS: OK";exitIp.Text="IP público de saída: "+ip;Log(testState.Text+" / "+ip);}catch(Exception ex){testState.Text="Falha SOCKS5";exitIp.Text="IP público de saída: indisponível";Error(ex);}finally{SetBusy(false);}}
 async Task TestTunnel(){if(busy)return;SetBusy(true);try{if(!active)throw new InvalidOperationException("Ative o túnel primeiro.");string ip=await Task.Run(()=>EngineController.RoutedIp());healthy=true;testState.Text="HTTPS do processo de teste protegido: OK";exitIp.Text="IP público pelo túnel: "+ip;Log(testState.Text+" / "+ip);}catch(Exception ex){healthy=false;testState.Text="Túnel não confirmado";exitIp.Text="IP público pelo túnel: indisponível";Error(ex);}finally{SetBusy(false);}}
 async Task CheckHealth(){checkingHealth=true;lastHealth=DateTime.UtcNow;int version=sessionVersion;bool ok=false;try{await Task.Run(()=>EngineController.RoutedIp());ok=true;}catch{}finally{checkingHealth=false;}if(!IsDisposed&&active&&version==sessionVersion){if(healthy!=ok)Log(ok?"Conectividade do processo protegido restabelecida.":"Erro de conectividade do processo protegido. Sem fallback direto.");healthy=ok;RefreshState();}}
 void EnsureAppsClosed(string[] paths){foreach(string path in paths)foreach(Process p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(path))){using(p){try{if(String.Equals(p.MainModule.FileName,path,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Feche "+Path.GetFileName(path)+" antes de ativar.");}catch(System.ComponentModel.Win32Exception){throw new InvalidOperationException("Não foi possível verificar um processo aberto. Feche "+Path.GetFileName(path)+" antes de ativar.");}}}}
 void SetBusy(bool value){busy=value;connect.Enabled=stop.Enabled=release.Enabled=test.Enabled=!value;global.Enabled=auto.Enabled=host.Enabled=port.Enabled=user.Enabled=password.Enabled=launch.Enabled=!value&&!active;applicationPage.Enabled=!value;apps.Enabled=!value&&!active;foreach(Control c in applicationPage.Controls)if(c is Button&&c.Text!="Abrir pelo túnel")c.Enabled=!value&&!active;}
 void RefreshState(){bool wasActive=active;active=!preview&&EngineController.Status().StartsWith("Motor ativo");bool? guarded=preview?(bool?)false:NetworkGuard.ArmedState;state.Text=active?(healthy?"Conectado • TCP / IPv4 • monitoramento HTTPS ativo":"Erro de conectividade • motor ativo • saída direta bloqueada"):!guarded.HasValue?"Proteção não consultada • ativação exigirá administrador":guarded.Value?"Túnel parado • saída direta bloqueada":"Desconectado • rede direta liberada";if(wasActive&&!active)Log("Motor desconectado. A proteção persistente permanece ativa.");SetBusy(busy);}
 void RefreshApps(){apps.Items.Clear();foreach(var a in route.Apps)apps.Items.Add(a,a.Selected);RefreshLaunch();}
 void RefreshLaunch(){string previous=launch.SelectedItem==null?null:((RegisteredApp)launch.SelectedItem).Path;launch.Items.Clear();foreach(var a in route.Apps)if(a.Selected)launch.Items.Add(a);for(int i=0;i<launch.Items.Count;i++)if(((RegisteredApp)launch.Items[i]).Path==previous)launch.SelectedIndex=i;if(launch.SelectedIndex<0&&launch.Items.Count>0)launch.SelectedIndex=0;}
 void Save(){if(preview)return;try{SessionStore.Save("",host.Text.Trim(),(int)port.Value,user.Text,password.Text,route);}catch{Log("Não foi possível salvar o perfil protegido.");}}
 void Error(Exception ex){string text=ex is AggregateException?"Falha de conexão TCP com o proxy.":ex.Message;Log("ERRO: "+text);MessageBox.Show(this,text,"Litfix",MessageBoxButtons.OK,MessageBoxIcon.Error);}
 void Log(string value){if(user.Text.Length>0)value=value.Replace(user.Text,"[usuario]");if(password.Text.Length>0)value=value.Replace(password.Text,"[senha]");if(logs.TextLength>24000)logs.Clear();logs.AppendText(DateTime.Now.ToString("HH:mm:ss")+"  "+value+Environment.NewLine);}
 void ShowPage(Panel page){foreach(Control child in content.Controls)child.Visible=child==page;page.BringToFront();}
 public void PreviewPage(string name){ShowPage(name=="apps"?applicationPage:name=="tests"?testPage:dashboard);}
 Label Label(string t,int x,int y,int w,int h,int size,Color color){return new Label{Text=t,Left=x,Top=y,Width=w,Height=h,ForeColor=color,Font=new Font("Segoe UI",size)};}
 Panel Card(int x,int y,int w,int h,params Control[] children){var p=new Panel{Left=x,Top=y,Width=w,Height=h,BackColor=card};p.Controls.AddRange(children);return p;}
 Button Button(string text,int x,int y,int w,bool primary){var b=new Button{Text=text,Left=x,Top=y,Width=w,Height=42,BackColor=primary?blue:card,ForeColor=Color.White,FlatStyle=FlatStyle.Flat,Cursor=Cursors.Hand,Font=new Font("Segoe UI",10,FontStyle.Bold)};b.FlatAppearance.BorderColor=blue;b.FlatAppearance.BorderSize=primary?0:1;return b;}
 void Style(Control c){c.BackColor=bg;c.ForeColor=Color.White;c.Font=new Font("Segoe UI",11);}
 void Field(Panel p,string title,TextBox input,int x,int y,int width){p.Controls.Add(Label(title,x,y,width,25,10,muted));input.SetBounds(x,y+29,width,32);Style(input);p.Controls.Add(input);}
}

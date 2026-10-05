using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Threading.Tasks;

public class PkaProxy : Form {
 TextBox game=new TextBox(),host=new TextBox(),user=new TextBox(),pass=new TextBox();
 NumericUpDown port=new NumericUpDown();
 RoutingOptions route=new RoutingOptions();
 Button check; Label status,directIp,proxyIp,routeSummary;
 Color bg=Color.FromArgb(17,24,39),card=Color.FromArgb(31,41,55),muted=Color.FromArgb(156,163,175),accent=Color.FromArgb(59,130,246);
 public PkaProxy(){
  Text="PKA Proxy";ClientSize=new Size(780,730);BackColor=bg;ForeColor=Color.White;FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;StartPosition=FormStartPosition.CenterScreen;Font=new Font("Segoe UI",10);AutoScaleMode=AutoScaleMode.Dpi;
  LabelAt("PKA Proxy",28,22,650,40,24,Color.White);
  LabelAt("PokeAlliance + Webshare • SOCKS5",30,67,650,24,10,muted);
  var updates=ActionButton("Atualizações",585,28,167,false);updates.Click+=(o,e)=>OpenUpdates(null);Controls.Add(updates);
  Shown+=async(o,e)=>{if(Environment.GetCommandLineArgs().Length==1)await CheckStartup();};
  Panel setup=new Panel{Left=28,Top=112,Width=724,Height=285,BackColor=card};Controls.Add(setup);
  AddField(setup,"CLIENTE FINAL DO JOGO (.EXE)",game,20);game.Width=395;
  var choose=UiTheme.PrimaryButton("Escolher",550,36,150);choose.Click+=(o,e)=>{string selected=ExecutablePicker.Select(this);if(selected!=null)game.Text=selected;};setup.Controls.Add(choose);game.Width=505;

  AddField(setup,"IP OU HOST DO PROXY",host,85);host.Width=395;
  var pl=new Label{Text="PORTA",Left=584,Top=85,Width=110,ForeColor=muted,Font=new Font("Segoe UI",9)};setup.Controls.Add(pl);port.SetBounds(584,107,116,28);port.Minimum=1;port.Maximum=65535;port.Value=1080;setup.Controls.Add(port);StyleInput(port);
  AddField(setup,"USUÁRIO",user,150);user.Width=325;
  var ul=new Label{Text="SENHA",Left=375,Top=150,Width=300,ForeColor=muted,Font=new Font("Segoe UI",9)};setup.Controls.Add(ul);pass.SetBounds(375,172,325,28);pass.UseSystemPasswordChar=true;setup.Controls.Add(pass);StyleInput(pass);
  setup.Controls.Add(new Label{Text="Selecione o cliente que o launcher abre. Filhos não herdam a regra.\nPara incluir o launcher ou usar todos os apps, abra Roteamento.",Left=24,Top=217,Width=680,Height=54,ForeColor=muted,Font=new Font("Segoe UI",9)});
  Panel result=new Panel{Left=28,Top=416,Width=724,Height=146,BackColor=card};Controls.Add(result);
  status=new Label{Text="Pronto para verificar",Left=24,Top=17,Width=530,Height=26,Font=new Font("Segoe UI",12,FontStyle.Bold)};result.Controls.Add(status);
  directIp=new Label{Text="Rede direta: —",Left=24,Top=54,Width=490,Height=25};proxyIp=new Label{Text="Via SOCKS5: —",Left=24,Top=82,Width=490,Height=25};result.Controls.Add(directIp);result.Controls.Add(proxyIp);
  check=ActionButton("Check proxy",554,48,146,true);check.Click+=async(o,e)=>await Check();result.Controls.Add(check);
  result.Controls.Add(new Label{Text="Consulta HTTPS ao ipify. Verifica este teste, não o tráfego do jogo.",Left=24,Top=115,Width=680,Height=23,ForeColor=muted,Font=new Font("Segoe UI",9)});
  var save=ActionButton("Salvar configuração",28,582,210,true);save.Click+=(o,e)=>Save();Controls.Add(save);
  var manager=ActionButton("Abrir motor instalado",252,582,210,false);manager.Click+=(o,e)=>{try{using(var d=new OpenFileDialog{Title="Selecione a interface oficial ProxiFyre instalada",Filter="Executável ProxiFyre|*.exe"})if(d.ShowDialog()==DialogResult.OK)System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(d.FileName){UseShellExecute=true,WorkingDirectory=Path.GetDirectoryName(d.FileName)});}catch(Exception){MessageBox.Show("Não foi possível abrir o motor selecionado.");}};Controls.Add(manager);
  var routing=ActionButton("Roteamento…",476,582,276,false);routing.Click+=(o,e)=>{route.ClientPath=game.Text;using(var dialog=new RoutingDialog(route)){if(dialog.ShowDialog(this)==DialogResult.OK){route=dialog.Options;game.Enabled=!route.Global;choose.Enabled=!route.Global;UpdateRouteSummary();}}};Controls.Add(routing);
  routeSummary=new Label{Left=30,Top=636,Width=720,Height=24,ForeColor=muted};Controls.Add(routeSummary);UpdateRouteSummary();
  LabelAt("Salvar não ativa o proxy. Aplique no ProxiFyre / Windows Packet Filter.\nA senha fica no JSON. Check não comprova o tráfego do jogo ou do sistema.",30,669,720,45,9,muted);
  var saved=SessionStore.Load();if(saved!=null){game.Text=saved.Game??"";host.Text=saved.Host??"";port.Value=saved.Port>=1&&saved.Port<=65535?saved.Port:1080;user.Text=saved.User??"";pass.Text=saved.Password();route=saved.Route??new RoutingOptions();game.Enabled=choose.Enabled=!route.Global;UpdateRouteSummary();}
  FormClosing+=(o,e)=>{try{SessionStore.Save(game.Text,host.Text,(int)port.Value,user.Text,pass.Text,route);}catch{/* App stays usable when profile storage is unavailable. */}};

 }
 async Task CheckStartup(){
  var preferences=UpdatePreferences.Load();if(!preferences.CheckOnStartup||String.IsNullOrWhiteSpace(preferences.Repository))return;
  try{var available=await Task.Run(()=>UpdateService.Check(preferences.Repository));if(!IsDisposed&&available!=null)OpenUpdates(available);}catch{/* Keep startup usable offline. */}
 }
 void OpenUpdates(PendingUpdate available){using(var dialog=new UpdateDialog(available)){dialog.ShowDialog(this);if(dialog.ExitForUpdate)Application.Exit();}}
 void UpdateRouteSummary(){routeSummary.Text=(route.Global?"Global / todos os apps":"Cliente final"+(route.LauncherPath.Length>0?" + launcher":""))+" • TCP"+(route.Udp?" + UDP":"")+" • "+(route.Ipv6?"IPv4 + IPv6":"IPv4");}
 void LabelAt(string t,int x,int y,int w,int h,int size,Color color){Controls.Add(new Label{Text=t,Left=x,Top=y,Width=w,Height=h,ForeColor=color,Font=new Font("Segoe UI",size)});}
 void StyleInput(Control c){c.BackColor=bg;c.ForeColor=Color.White;c.Font=new Font("Segoe UI",11);if(c is TextBox)((TextBox)c).BorderStyle=BorderStyle.FixedSingle;}
 void AddField(Panel panel,string name,Control c,int y){panel.Controls.Add(new Label{Text=name,Left=24,Top=y,Width=500,ForeColor=muted,Font=new Font("Segoe UI",9)});c.SetBounds(24,y+22,676,28);StyleInput(c);panel.Controls.Add(c);}
 Button ActionButton(string t,int x,int y,int w,bool primary){var b=new Button{Text=t,Left=x,Top=y,Width=w,Height=38,FlatStyle=FlatStyle.Flat,BackColor=primary?accent:card,ForeColor=Color.White,Cursor=Cursors.Hand};b.FlatAppearance.BorderSize=0;return b;}
 async Task Check(){
  string h=host.Text.Trim(),u=user.Text,pw=pass.Text;int p=(int)port.Value;
  try{RoutingConfiguration.ValidateProxy(h,p,u,pw);}catch(Exception ex){MessageBox.Show(ex.Message);return;}
  check.Enabled=false;status.Text="Verificando conexão…";status.ForeColor=Color.White;directIp.Text="Rede direta: consultando…";proxyIp.Text="Via SOCKS5: consultando…";
  try{
   var directTask=Task.Run(()=>ProxyDiagnostics.Attempt(()=>ProxyDiagnostics.DirectIp()));var proxyTask=Task.Run(()=>ProxyDiagnostics.Attempt(()=>ProxyDiagnostics.ProxyIp(h,p,u,pw)));
   await Task.WhenAll(directTask,proxyTask);if(IsDisposed)return;
   var d=directTask.Result;var r=proxyTask.Result;
   directIp.Text="Rede direta: "+d;proxyIp.Text="Via SOCKS5: "+r;
   IPAddress dip,rip;bool dok=IPAddress.TryParse(d,out dip),rok=IPAddress.TryParse(r,out rip);
   if(!rok){status.Text="Proxy não confirmado • sem fallback direto";status.ForeColor=Color.FromArgb(248,113,113);}
   else if(!dok){status.Text="SOCKS5 funciona • comparação indisponível";status.ForeColor=Color.FromArgb(251,191,36);}
   else if(dip.Equals(rip)){status.Text="SOCKS5 respondeu • mesmo IP de saída";status.ForeColor=Color.FromArgb(251,191,36);}
   else {status.Text="Proxy funcionando • IP de saída diferente";status.ForeColor=Color.FromArgb(52,211,153);}
  }finally{if(!IsDisposed)check.Enabled=true;}
 }
 void Save(){try{
  route.ClientPath=game.Text;
  string json=RoutingConfiguration.Build(route,host.Text,(int)port.Value,user.Text,pass.Text,true);
  using(var d=new SaveFileDialog{Filter="Configuração JSON|*.json",FileName="app-config.json",OverwritePrompt=true})if(d.ShowDialog()==DialogResult.OK){File.WriteAllText(d.FileName,json,new UTF8Encoding(false));MessageBox.Show("Configuração salva: "+(route.Global?"Global / todos os aplicativos":"Cliente do jogo")+". Aplique e reinicie o serviço elevado no ProxiFyre. Salvar não ativa o roteamento.");}
 }catch(Exception ex){MessageBox.Show(ex.Message,"Confira os campos");}}
}

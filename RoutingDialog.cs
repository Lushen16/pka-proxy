using System;using System.Drawing;using System.Windows.Forms;
public sealed class RoutingDialog:Form {
 public RoutingOptions Options{get;private set;}
 RadioButton selected,global;TextBox launcher;CheckBox udp,ipv6;Label help;
 public RoutingDialog(RoutingOptions current){
  Options=current.Copy();Text="Roteamento de aplicativos";ClientSize=new Size(700,505);FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;MinimizeBox=false;StartPosition=FormStartPosition.CenterParent;BackColor=Color.FromArgb(17,24,39);ForeColor=Color.White;Font=new Font("Segoe UI",10);AutoScaleMode=AutoScaleMode.Dpi;
  Controls.Add(new Label{Text="Quem deve usar o proxy?",Left=24,Top=20,Width=650,Height=38,Font=new Font("Segoe UI",18,FontStyle.Bold)});
  selected=new RadioButton{Text="Cliente final do jogo + launcher opcional",Left=26,Top=72,Width=645,Height=28,Checked=!current.Global};
  global=new RadioButton{Text="Global / Todos os aplicativos do Windows",Left=26,Top=110,Width=645,Height=28,Checked=current.Global};Controls.Add(selected);Controls.Add(global);
  Controls.Add(new Label{Text="LAUNCHER OPCIONAL (.EXE)",Left=26,Top=157,Width=640,Height=24,ForeColor=Color.LightGray});
  launcher=new TextBox{Left=26,Top=185,Width=478,Text=current.LauncherPath,BackColor=Color.FromArgb(31,41,55),ForeColor=Color.White,BorderStyle=BorderStyle.FixedSingle};Controls.Add(launcher);
  var pick=UiTheme.PrimaryButton("Escolher",518,177,155);pick.Click+=(s,e)=>{string path=ExecutablePicker.Select(this);if(path!=null)launcher.Text=path;};Controls.Add(pick);
  udp=new CheckBox{Text="Incluir UDP (requer suporte do proxy)",Left=26,Top=238,Width=645,Height=28,Checked=current.Udp};ipv6=new CheckBox{Text="Incluir destinos IPv6 (requer suporte do proxy)",Left=26,Top=274,Width=645,Height=28,Checked=current.Ipv6};Controls.Add(udp);Controls.Add(ipv6);
  help=new Label{Left=26,Top=320,Width=647,Height=118,ForeColor=Color.FromArgb(156,163,175)};Controls.Add(help);
  Action refresh=()=>{launcher.Enabled=pick.Enabled=!global.Checked;help.Text=global.Checked?
   "Global cria uma regra para todos os apps identificados pelo serviço elevado. Este configurador fica excluído para permitir o Check direto. Outros túneis podem precisar de exclusões. Não é uma VPN completa: ICMP, UDP IPv6 fragmentado e processos não identificados podem ficar fora da cobertura. Sem kill switch.":
   "Selecione o cliente final na janela principal. O launcher pode ser incluído aqui, mas seus filhos não herdam regras. Identifique o .exe do cliente em Gerenciador de Tarefas > Detalhes > Abrir local do arquivo. Adicione explicitamente outros executáveis necessários no motor.";};
  global.CheckedChanged+=(s,e)=>{if(global.Checked){udp.Checked=true;ipv6.Checked=true;}refresh();};selected.CheckedChanged+=(s,e)=>refresh();refresh();
  var confirm=UiTheme.PrimaryButton("Usar este modo",26,447,220);confirm.Image=null;confirm.Click+=(s,e)=>{Options.Global=global.Checked;Options.Udp=udp.Checked;Options.Ipv6=ipv6.Checked;Options.LauncherPath=launcher.Text.Trim();DialogResult=DialogResult.OK;};Controls.Add(confirm);
  var cancel=new Button{Text="Cancelar",Left=260,Top=447,Width=130,Height=42,DialogResult=DialogResult.Cancel,FlatStyle=FlatStyle.Flat};Controls.Add(cancel);CancelButton=cancel;
 }
}

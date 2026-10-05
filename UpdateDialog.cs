using System;using System.IO;using System.Drawing;using System.Windows.Forms;using System.Threading.Tasks;
public sealed class UpdateDialog:Form {
 TextBox repo;CheckBox automatic;Button check,install;Label info;PendingUpdate pending;
 public bool ExitForUpdate{get;private set;}
 public UpdateDialog(PendingUpdate available){
  pending=available;var pref=UpdatePreferences.Load();Text="Atualizações — PKA Proxy";ClientSize=new Size(680,350);BackColor=Color.FromArgb(17,24,39);ForeColor=Color.White;Font=new Font("Segoe UI",10);StartPosition=FormStartPosition.CenterParent;FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;
  Controls.Add(new Label{Text="Versão instalada: "+UpdateService.Current,Left=24,Top=22,Width=625,Height=32,Font=new Font("Segoe UI",16,FontStyle.Bold)});
  Controls.Add(new Label{Text="Repositório público GitHub (link ou usuario/repositorio)",Left=24,Top=72,Width=625,Height=26});
  repo=new TextBox{Left=24,Top=102,Width=630,Text=pref.Repository??"",BackColor=Color.FromArgb(31,41,55),ForeColor=Color.White};Controls.Add(repo);
  automatic=new CheckBox{Text="Verificar novas versões ao abrir (consulta o GitHub)",Left=24,Top=146,Width=630,Height=27,Checked=pref.CheckOnStartup};Controls.Add(automatic);
  info=new Label{Left=24,Top=192,Width=630,Height=68,ForeColor=Color.LightGray,Text=available==null?"Configure o repositório para receber atualizações assinadas.\nNenhum token ou senha GitHub é necessário.":"Nova versão disponível: "+available.Manifest.version};Controls.Add(info);
  check=UiTheme.PrimaryButton("Salvar e verificar",24,278,220);check.Image=null;check.Click+=async(s,e)=>await Check();Controls.Add(check);
  install=UiTheme.PrimaryButton("Atualizar agora",264,278,220);install.Image=null;install.Enabled=available!=null;install.Click+=async(s,e)=>await Install();Controls.Add(install);
  repo.TextChanged+=(s,e)=>{pending=null;install.Enabled=false;};
 }
 async Task Check(){
  string repository;try{repository=UpdateService.Repository(repo.Text);new UpdatePreferences{Repository=repository,CheckOnStartup=automatic.Checked}.Save();}catch(Exception ex){info.Text=ex.Message;return;}
  check.Enabled=install.Enabled=false;repo.Enabled=automatic.Enabled=false;info.Text="Consultando versão e verificando assinatura…";
  try{pending=await Task.Run(()=>UpdateService.Check(repository));if(IsDisposed)return;info.Text=pending==null?"Você já usa a versão mais recente publicada.":"Nova versão assinada: "+pending.Manifest.version+"\nClique em Atualizar agora para baixar e reiniciar.";}
  catch(Exception){if(!IsDisposed)info.Text="Não foi possível verificar. Confira o repositório, a internet e os arquivos\nupdate.json / update.sig da release. Sua versão continua funcionando.";pending=null;}
  finally{if(!IsDisposed){check.Enabled=repo.Enabled=automatic.Enabled=true;install.Enabled=pending!=null;}}
 }
 async Task Install(){
  if(pending==null)return;
  check.Enabled=install.Enabled=repo.Enabled=automatic.Enabled=false;ControlBox=false;info.Text="Baixando atualização e verificando integridade…";
  try{
   await Task.Run(()=>UpdateService.Fetch(pending));if(IsDisposed)return;
   UpdateInstaller.Start(pending);
   string ready=Path.Combine(pending.Folder,"ready");bool started=false;
   for(int i=0;i<100;i++){if(File.Exists(ready)){started=true;break;}await Task.Delay(100);}
   if(!started)throw new IOException("Atualizador não iniciou.");
   ExitForUpdate=true;DialogResult=DialogResult.OK;
  }catch(Exception){if(!IsDisposed)info.Text="Atualização não instalada. Confira a conexão e as permissões da pasta.\nA versão atual e suas configurações foram preservadas.";}
  finally{if(!IsDisposed){check.Enabled=repo.Enabled=automatic.Enabled=ControlBox=true;install.Enabled=true;}}
 }
}

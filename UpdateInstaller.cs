using System;using System.IO;using System.Diagnostics;using System.Windows.Forms;
public static class UpdateInstaller {
 public static void Start(PendingUpdate update){
  string target=System.Reflection.Assembly.GetExecutingAssembly().Location;
  string helper=Path.Combine(update.Folder,"PKA-Update-Helper.exe");File.Copy(target,helper,true);
  string args="--apply-update "+Process.GetCurrentProcess().Id+" "+Quote(target)+" "+Quote(update.Folder);
  Process.Start(new ProcessStartInfo(helper,args){UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=update.Folder});
 }
 static string Quote(string value){if(value.Contains("\""))throw new ArgumentException();return "\""+value+"\"";}
 public static int Apply(string[] args){
  try{
   if(args.Length!=4)throw new ArgumentException("Argumentos do atualizador inválidos.");
   string target=Path.GetFullPath(args[2]),folder=Path.GetFullPath(args[3]);
   if(!Path.GetFileName(target).Equals("PKA-Proxy.exe",StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Destino inválido.");
   var m=UpdateService.VerifyManifest(File.ReadAllBytes(Path.Combine(folder,"update.json")),File.ReadAllBytes(Path.Combine(folder,"update.sig")),UpdateTrust.PublicKey);
   string source=Path.Combine(folder,"PKA-Proxy.exe");UpdateService.VerifyFile(source,m);
   if(new Version(m.version)<=UpdateService.Current)throw new InvalidOperationException("Atualização não é mais recente.");
   var parent=Process.GetProcessById(Int32.Parse(args[1]));
   if(!String.Equals(Path.GetFullPath(parent.MainModule.FileName),target,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Processo de destino diferente do aplicativo.");
   string ready=Path.Combine(folder,"ready");File.WriteAllText(ready,"ready");
   if(!parent.WaitForExit(30000))throw new InvalidOperationException("Feche o aplicativo para atualizar.");
   string backup=ReplaceBinary(source,target);
   try{Process.Start(new ProcessStartInfo(target){UseShellExecute=true,WorkingDirectory=Path.GetDirectoryName(target)});}
   catch{File.Copy(backup,target,true);throw;}
   return 0;
  }catch(Exception ex){MessageBox.Show("Atualização não concluída. A configuração foi preservada.\n"+ex.Message,"PKA Proxy — atualização");return 1;}
 }
 public static string ReplaceBinary(string source,string target){
  // Replace only the binary, never app-config.json or other user files.
  string next=target+".new-"+Guid.NewGuid().ToString("N"),backup=target+".backup-"+Guid.NewGuid().ToString("N");
  File.Copy(source,next,false);
  try{File.Replace(next,target,backup,true);return backup;}finally{if(File.Exists(next))File.Delete(next);}
 }
}

using System;using System.Text;using System.Text.RegularExpressions;
public sealed class RegisteredApp { public string Path=""; public bool Selected=true; public override string ToString(){return System.IO.Path.GetFileNameWithoutExtension(Path)+"  |  "+Path;} }
public sealed class RoutingOptions {
 public bool Global,Udp,Ipv6,AutoLaunch,DirectDiscordCalls;public string ClientPath="",LauncherPath="";
 public System.Collections.Generic.List<RegisteredApp> Apps=new System.Collections.Generic.List<RegisteredApp>();
 public RoutingOptions Copy(){var r=(RoutingOptions)MemberwiseClone();r.Apps=new System.Collections.Generic.List<RegisteredApp>();foreach(var a in Apps)r.Apps.Add(new RegisteredApp{Path=a.Path,Selected=a.Selected});return r;}
 public string[] SelectedPaths(){var p=new System.Collections.Generic.List<string>();foreach(var a in Apps)if(a.Selected){string path=System.IO.Path.GetFullPath(a.Path);if(!System.IO.File.Exists(path)||!path.EndsWith(".exe",StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Executável não encontrado: "+path);if(!p.Contains(path))p.Add(path);}if(!Global&&p.Count==0)throw new ArgumentException("Selecione ao menos um aplicativo.");return p.ToArray();}
}
public static class RoutingConfiguration
{
 public static void ValidateProxy(string h,int p,string u,string pw)
 {
  if(String.IsNullOrWhiteSpace(h)||!Regex.IsMatch(h.Trim(),@"\A[A-Za-z0-9][A-Za-z0-9.-]*\z"))throw new ArgumentException("Informe o IP da proxy sem protocolo nem porta.");
  if(p<1||p>65535)throw new ArgumentException("Porta inválida.");
  if(u==null||pw==null||String.IsNullOrEmpty(u)!=String.IsNullOrEmpty(pw))throw new ArgumentException("Preencha usuário e senha juntos.");
  if(Encoding.UTF8.GetByteCount(u)>255||Encoding.UTF8.GetByteCount(pw)>255)throw new ArgumentException("Credenciais excedem 255 bytes.");
 }
}

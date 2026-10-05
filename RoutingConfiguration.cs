using System;using System.IO;using System.Text;using System.Text.RegularExpressions;using System.Collections.Generic;using System.Web.Script.Serialization;
public sealed class RoutingOptions {
 public bool Global,Udp,Ipv6;
 public string ClientPath="",LauncherPath="";
 public RoutingOptions Copy(){return (RoutingOptions)MemberwiseClone();}
}
public static class RoutingConfiguration {
 public static void ValidateProxy(string h,int p,string u,string pw){
  if(!Regex.IsMatch(h.Trim(),@"\A[A-Za-z0-9][A-Za-z0-9.-]*\z"))throw new ArgumentException("Informe host ou IPv4 sem protocolo ou porta.");
  if(p<1||p>65535)throw new ArgumentException("Porta inválida.");
  if(String.IsNullOrEmpty(u)!=String.IsNullOrEmpty(pw))throw new ArgumentException("Preencha usuário e senha juntos.");
  if(Encoding.UTF8.GetByteCount(u)>255||Encoding.UTF8.GetByteCount(pw)>255)throw new ArgumentException("Credenciais excedem 255 bytes.");
 }
 static string Executable(string path,bool checkFile){
  path=path.Trim();if(!Path.IsPathRooted(path)||!Path.GetExtension(path).Equals(".exe",StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Selecione o caminho completo do cliente final .exe.");
  path=Path.GetFullPath(path);if(checkFile&&!File.Exists(path))throw new ArgumentException("Executável não encontrado: "+path);return path;
 }
 public static string Build(RoutingOptions o,string host,int port,string user,string pass,bool checkFiles){
  ValidateProxy(host,port,user,pass);
  var apps=new List<string>();
  if(o.Global)apps.Add("");else {
   apps.Add(Executable(o.ClientPath,checkFiles));
   if(!String.IsNullOrWhiteSpace(o.LauncherPath)){string launcher=Executable(o.LauncherPath,checkFiles);if(!String.Equals(apps[0],launcher,StringComparison.OrdinalIgnoreCase))apps.Add(launcher);}
  }
  var rule=new Dictionary<string,object>{{"appNames",apps.ToArray()},{"socks5ProxyEndpoint",host.Trim()+":"+port},{"socks5Transport","TCP"},{"supportedProtocols",o.Udp?new[]{"TCP","UDP"}:new[]{"TCP"}},{"supportedAddressFamilies",o.Ipv6?new[]{"IPv4","IPv6"}:new[]{"IPv4"}}};
  if(user.Length>0){rule["username"]=user;rule["password"]=pass;}
  // Exclude the diagnostic app so the direct-IP baseline stays outside our catch-all.
  // Other VPN/tunnel carrier processes need exclusions in the engine if present.
  var excludes=o.Global?new[]{Path.GetFullPath(System.Reflection.Assembly.GetExecutingAssembly().Location)}:new string[0];
  return new JavaScriptSerializer().Serialize(new Dictionary<string,object>{{"logLevel","Error"},{"bypassLan",false},{"proxies",new[]{rule}},{"excludes",excludes}});
 }
}

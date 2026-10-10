using System;using System.Collections.Generic;using System.Net;using System.Web.Script.Serialization;
public static class TunConfiguration
{
 static Dictionary<string,object> Map(params object[] p){var d=new Dictionary<string,object>();for(int i=0;i<p.Length;i+=2)d[(string)p[i]]=p[i+1];return d;}
 public static string Build(EngineRequest r)
 {
  RoutingConfiguration.ValidateProxy(r.Host,r.Port,r.User,r.Password);
  IPAddress ip;if(!IPAddress.TryParse(r.Host,out ip)||ip.AddressFamily!=System.Net.Sockets.AddressFamily.InterNetwork)throw new ArgumentException("Informe o IPv4 do SOCKS5 Webshare; hostnames não são usados para evitar DNS de bootstrap direto.");
  if(r.Route.Udp||r.Route.Ipv6)throw new ArgumentException("Esta versão protege apenas TCP/IPv4. UDP e IPv6 são bloqueados.");
  var targets=new List<string>(r.Route.SelectedPaths());targets.Add(System.IO.Path.Combine(AppPaths.Root,"probe","PKArouteProbe.exe"));
  var rules=new List<object>();
  rules.Add(Map("port",53,"action","hijack-dns"));
  rules.Add(Map("process_path",new[]{r.OwnerPath??System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName},"action","route","outbound","direct"));
  if(!r.Route.Global)rules.Add(Map("type","logical","mode","and","rules",new[]{Map("process_path",targets.ToArray()),Map("network","tcp"),Map("ip_version",4)},"action","route","outbound","proxy"));
  if(!r.Route.Global)rules.Add(Map("process_path",targets.ToArray(),"action","reject"));
  if(r.Route.Global){rules.Add(Map("ip_version",6,"action","reject"));rules.Add(Map("network",new[]{"udp","icmp"},"action","reject"));}
  var proxy=Map("type","socks","tag","proxy","server",r.Host,"server_port",r.Port,"version","5");if(r.User.Length>0){proxy["username"]=r.User;proxy["password"]=r.Password;}
  return new JavaScriptSerializer().Serialize(Map(
   "log",Map("level","warn","timestamp",true),
   "dns",Map("servers",new[]{Map("type","https","tag","secure-dns","server","1.1.1.1","path","/dns-query","detour","proxy","tls",Map("enabled",true,"server_name","cloudflare-dns.com"))},"final","secure-dns","strategy","ipv4_only"),
   "inbounds",new[]{Map("type","tun","tag","pka-tun","interface_name","PKAproxy-TUN","address",new[]{"172.27.254.1/30","fd5a:504b:4100::1/126"},"mtu",1400,"auto_route",true,"strict_route",true)},
   "outbounds",new[]{proxy,Map("type","direct","tag","direct")},
   "route",Map("auto_detect_interface",true,"find_process",true,"final",r.Route.Global?"proxy":"direct","rules",rules.ToArray())));
 }
}

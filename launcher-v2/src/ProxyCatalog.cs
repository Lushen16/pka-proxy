using System;using System.IO;using System.Text;using System.Collections.Generic;using System.Security.Cryptography;using System.Web.Script.Serialization;
public sealed class ProxyPreset
{
 public string Host="",User="",Password="",Location="";public int Port=1080;
}
public sealed class ProxySelectionState
{
 public bool GeneralAutomatic,DiscordAutomatic;public int GeneralIndex,DiscordIndex;public ProxyPreset GeneralManual,DiscordManual;
}
public static class ProxyCatalog
{
 static void Write(string name,object value){Directory.CreateDirectory(AppPaths.Root);byte[] bytes=Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(value));File.WriteAllBytes(AppPaths.SettingsFile(name),ProtectedData.Protect(bytes,null,DataProtectionScope.CurrentUser));}
 static T Read<T>(string name) where T:new(){try{return new JavaScriptSerializer().Deserialize<T>(Encoding.UTF8.GetString(ProtectedData.Unprotect(File.ReadAllBytes(AppPaths.SettingsFile(name)),null,DataProtectionScope.CurrentUser)));}catch{return new T();}}
 public static List<ProxyPreset> Load(){return Read<List<ProxyPreset>>("proxy-catalog.bin")??new List<ProxyPreset>();}
 public static void Save(List<ProxyPreset> presets){if(presets==null||presets.Count==0||presets.Count>1000)throw new ArgumentException("Importe de 1 a 1000 proxies.");foreach(var p in presets)Validate(p);Write("proxy-catalog.bin",presets);}
 public static ProxySelectionState Selection(){return Read<ProxySelectionState>("proxy-selection.bin")??new ProxySelectionState();}
 public static void SaveSelection(ProxySelectionState state){Write("proxy-selection.bin",state);}
 public static void Validate(ProxyPreset p){if(p==null)throw new ArgumentException("Proxy inválida.");RoutingConfiguration.ValidateProxy(p.Host,p.Port,p.User,p.Password);System.Net.IPAddress ip;if(!System.Net.IPAddress.TryParse(p.Host,out ip)||ip.AddressFamily!=System.Net.Sockets.AddressFamily.InterNetwork)throw new ArgumentException("A lista deve conter endereços IPv4.");}
 public static List<ProxyPreset> Parse(string text)
 {
  var list=new List<ProxyPreset>();int line=0;foreach(string raw in text.Split(new[]{'\r','\n'},StringSplitOptions.RemoveEmptyEntries)){line++;string value=raw.Trim();if(value.Length==0||value.StartsWith("#"))continue;string[] parts=value.Split(new[]{':'},4);int port;if(parts.Length!=4||!Int32.TryParse(parts[1],out port))throw new ArgumentException("Linha "+line+": use IP:porta:usuário:senha.");var p=new ProxyPreset{Host=parts[0].Trim(),Port=port,User=parts[2],Password=parts[3]};try{Validate(p);}catch(ArgumentException){throw new ArgumentException("Proxy inválida na linha "+line+".");}list.Add(p);}if(list.Count==0)throw new ArgumentException("A lista está vazia.");return list;
 }
}

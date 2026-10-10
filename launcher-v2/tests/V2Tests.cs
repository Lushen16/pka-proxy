using System;using System.IO;using System.Text;using System.Net;using System.Net.Sockets;using System.Threading.Tasks;using System.Collections.Generic;using System.Web.Script.Serialization;using System.Runtime.InteropServices;
public static class AppPaths {public static string Root;public static string SettingsFile(string name){return Path.Combine(Root,name);}}
public class EngineRequest {public RoutingOptions Route;public string Host,User,Password,OwnerPath;public int Port;}
class Tests
{
 static int count;
 static void Assert(bool ok,string message){if(!ok)throw new Exception(message);count++;}
 static void Reject(System.Action action,string message){bool rejected=false;try{action();}catch(ArgumentException){rejected=true;}Assert(rejected,message);}
 static Dictionary<string,object> Parse(string json){return (Dictionary<string,object>)new JavaScriptSerializer().DeserializeObject(json);}
 static byte[] Read(Stream s,int length){var b=new byte[length];int n=0;while(n<length){int got=s.Read(b,n,length-n);if(got==0)throw new IOException();n+=got;}return b;}
 static void Reply(Stream s,params byte[] b){foreach(byte x in b){s.WriteByte(x);s.Flush();}}
 static void Socks(bool credentials,bool failAuth,bool refuseMethod,bool badVersion,bool domain)
 {
  var listener=new TcpListener(IPAddress.Loopback,0);listener.Start();int port=((IPEndPoint)listener.LocalEndpoint).Port;
  var server=Task.Run(()=>{using(var client=listener.AcceptTcpClient())using(var s=client.GetStream()){
   s.ReadTimeout=s.WriteTimeout=3000;byte method=(byte)(credentials?2:0);var greeting=Read(s,3);Assert(greeting[0]==5&&greeting[1]==1&&greeting[2]==method,"offers exactly intended method");
   Reply(s,5,(byte)(refuseMethod?255:method));if(refuseMethod)return;
   if(credentials){Assert(Read(s,1)[0]==1,"RFC1929 version");int n=Read(s,1)[0];Assert(Encoding.UTF8.GetString(Read(s,n))=="fixture-user","username encoded");n=Read(s,1)[0];Assert(Encoding.UTF8.GetString(Read(s,n))=="fixture-secret","password encoded");Reply(s,1,(byte)(failAuth?1:0));if(failAuth)return;}
   var request=Read(s,5);Assert(request[0]==5&&request[1]==1&&request[2]==0&&request[3]==3,"CONNECT uses remote DNS");Assert(Encoding.ASCII.GetString(Read(s,request[4]))=="api.ipify.org","expected IP endpoint");var targetPort=Read(s,2);Assert(targetPort[0]==1&&targetPort[1]==187,"HTTPS port 443");
   if(badVersion){Reply(s,4,0,0,1);return;}if(domain)Reply(s,5,0,0,3,3,97,98,99,1,2);else Reply(s,5,0,0,1,127,0,0,1,1,2);
  }});
  bool failed=false;try{using(var client=new TcpClient()){client.Connect(IPAddress.Loopback,port);client.ReceiveTimeout=3000;using(var s=client.GetStream()){s.ReadTimeout=s.WriteTimeout=3000;try{ProxyDiagnostics.Handshake(s,credentials?"fixture-user":"",credentials?"fixture-secret":"");}catch(InvalidOperationException){failed=true;}}}}finally{listener.Stop();}
  Assert(server.Wait(5000),"fixture completes");server.GetAwaiter().GetResult();Assert(failed==(failAuth||refuseMethod||badVersion),"fail closed on malformed/authentication response");
 }
 static int Main(string[] args)
 {
  AppPaths.Root=Path.GetFullPath(args[0]);Directory.CreateDirectory(AppPaths.Root);string app=Path.Combine(AppPaths.Root,"fixture.exe");File.WriteAllText(app,"fixture, never executed");
  var r=new EngineRequest{Route=new RoutingOptions{Global=true},Host="192.0.2.10",Port=1080,User="fixture-user",Password="fixture-secret",OwnerPath=app};
  var config=Parse(TunConfiguration.Build(r));var route=(Dictionary<string,object>)config["route"];Assert((string)route["final"]=="proxy","global uses proxy");var rules=(object[])route["rules"];Assert(rules.Length==4,"global DNS, UI, IPv6, UDP/ICMP rules");Assert((string)((Dictionary<string,object>)rules[2])["action"]=="reject","global rejects IPv6");Assert((string)((Dictionary<string,object>)rules[3])["action"]=="reject","global rejects UDP/ICMP");File.WriteAllText(Path.Combine(AppPaths.Root,"global.json"),TunConfiguration.Build(r));
  r.Route.Global=false;Reject(()=>TunConfiguration.Build(r),"empty selection rejected");r.Route.Apps.Add(new RegisteredApp{Path=app,Selected=true});r.OwnerPath=Path.Combine(Environment.SystemDirectory,"notepad.exe");config=Parse(TunConfiguration.Build(r));route=(Dictionary<string,object>)config["route"];Assert((string)route["final"]=="direct","unselected apps remain direct");rules=(object[])route["rules"];var selected=(Dictionary<string,object>)rules[2];Assert((string)selected["type"]=="logical"&&(string)selected["mode"]=="and","target AND TCP AND IPv4");Assert((string)((Dictionary<string,object>)rules[3])["action"]=="reject","selected unsupported traffic rejected");Assert(!TunConfiguration.Build(r).Contains("ip_is_private"),"no LAN bypass for targets");File.WriteAllText(Path.Combine(AppPaths.Root,"applications.json"),TunConfiguration.Build(r));
  var copy=r.Route.Copy();copy.Apps[0].Selected=false;Assert(r.Route.Apps[0].Selected,"request selection copied deeply");r.Route.Udp=true;Reject(()=>TunConfiguration.Build(r),"UDP unsupported rejected");r.Route.Udp=false;r.Route.Ipv6=true;Reject(()=>TunConfiguration.Build(r),"IPv6 unsupported rejected");r.Route.Ipv6=false;r.Host="proxy.example.com";Reject(()=>TunConfiguration.Build(r),"hostname bootstrap rejected");
  Reject(()=>RoutingConfiguration.ValidateProxy("192.0.2.10",0,"",""),"bad port rejected");Reject(()=>RoutingConfiguration.ValidateProxy("192.0.2.10",1080,"u",""),"partial credentials rejected");Reject(()=>RoutingConfiguration.ValidateProxy("192.0.2.10",1080,new string('u',256),"p"),"oversized credentials rejected");
  SessionStore.Save("","192.0.2.10",1080,"fixture-user","fixture-secret",r.Route);byte[] stored=File.ReadAllBytes(AppPaths.SettingsFile("session-v2.bin"));string raw=Encoding.UTF8.GetString(stored);Assert(!raw.Contains("fixture-secret")&&!raw.Contains("fixture-user"),"whole profile encrypted");var session=SessionStore.Load();Assert(session.User=="fixture-user"&&session.Password()=="fixture-secret","DPAPI roundtrip");stored[stored.Length-1]^=0xff;File.WriteAllBytes(AppPaths.SettingsFile("session-v2.bin"),stored);Assert(SessionStore.Load()==null,"tampered profile rejected");
  Assert(IntPtr.Size==8,"native tests x64");Assert(Marshal.SizeOf(typeof(NetworkGuard.Value))==16,"FWP value size");Assert(Marshal.SizeOf(typeof(NetworkGuard.Condition))==40,"condition size");Assert(Marshal.SizeOf(typeof(NetworkGuard.Filter))==200,"filter size");Assert(NetworkGuard.Key("x")==NetworkGuard.Key("x")&&NetworkGuard.Key("x")!=NetworkGuard.Key("y"),"stable recovery keys");
  Socks(false,false,false,false,false);Socks(true,false,false,false,true);Socks(true,true,false,false,false);Socks(false,false,true,false,false);Socks(false,false,false,true,false);
  Console.WriteLine("PASS: "+count+" assertions; real loopback SOCKS5 fixtures, configuration, DPAPI and native layouts. WFP activation and real Webshare not exercised.");return 0;
 }
}

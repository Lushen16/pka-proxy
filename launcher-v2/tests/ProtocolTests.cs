using System;using System.IO;using System.Net;using System.Net.Sockets;using System.Text;using System.Threading.Tasks;
public static class ProtocolTests
{
 sealed class Duplex:MemoryStream
 {
  public MemoryStream Sent=new MemoryStream();public Duplex(string response):base(Encoding.ASCII.GetBytes(response)){}
  public override void Write(byte[] b,int o,int n){Sent.Write(b,o,n);}
 }
 public static void Run()
 {
  using(var s=new Duplex("HTTP/1.1 200 Connection established\r\n\r\nX")){
   ProxyDiagnostics.HttpConnect(s,"user","secret");
   string request=Encoding.ASCII.GetString(s.Sent.ToArray());
   if(!request.Contains("CONNECT api.ipify.org:443")||!request.Contains("Basic dXNlcjpzZWNyZXQ=")||s.ReadByte()!=88)throw new Exception("CONNECT/auth or tunnel boundary failed");
  }
  foreach(string response in new[]{"HTTP/1.1 407 Authentication required\r\n\r\n","HTTP/1.1 200 OK\r\n"}){
   bool failed=false;using(var s=new Duplex(response)){try{ProxyDiagnostics.HttpConnect(s,"","");}catch(IOException){failed=true;}catch(InvalidOperationException){failed=true;}}
   if(!failed)throw new Exception("CONNECT must reject malformed/auth response");
  }
  var tcp=new TcpListener(IPAddress.Loopback,0);tcp.Start();
  using(var udp=new UdpClient(new IPEndPoint(IPAddress.Loopback,0))){
   udp.Client.ReceiveTimeout=5000;int udpPort=((IPEndPoint)udp.Client.LocalEndPoint).Port;
   var server=Task.Run(()=>{using(var c=tcp.AcceptTcpClient())using(var s=c.GetStream()){
    s.ReadTimeout=5000;Read(s,3);s.Write(new byte[]{5,0},0,2);var associate=Read(s,10);if(associate[1]!=3)throw new Exception("Not UDP ASSOCIATE");
    byte[] reply={5,0,0,1,127,0,0,1,(byte)(udpPort>>8),(byte)udpPort};s.Write(reply,0,reply.Length);
    IPEndPoint sender=null;var packet=udp.Receive(ref sender);packet[12]|=128;udp.Send(packet,packet.Length,sender);
    s.ReadByte();
   }});
   try{ProxyDiagnostics.CheckUdp("127.0.0.1",((IPEndPoint)tcp.LocalEndpoint).Port,"","","SOCKS5");if(!server.Wait(6000))throw new Exception("UDP fixture timeout");server.GetAwaiter().GetResult();}finally{tcp.Stop();}
  }
  bool refused=false;try{ProxyDiagnostics.CheckUdp("127.0.0.1",1,"","","HTTPS");}catch(ArgumentException){refused=true;}if(!refused)throw new Exception("HTTPS UDP accepted");
  Console.WriteLine("PASS: CONNECT/auth, malformed responses, real SOCKS5 UDP relay roundtrip, HTTPS UDP rejection.");
 }
 static byte[] Read(Stream s,int n){byte[] b=new byte[n];for(int i=0;i<n;i++){int v=s.ReadByte();if(v<0)throw new IOException();b[i]=(byte)v;}return b;}
}

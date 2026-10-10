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
public static class ProxyDiagnostics
{

    public static void HttpConnect(Stream stream,string user,string password)
    {
        string auth=user.Length==0?"":"Proxy-Authorization: Basic "+Convert.ToBase64String(Encoding.UTF8.GetBytes(user+":"+password))+"\r\n";
        byte[] request=Encoding.ASCII.GetBytes("CONNECT api.ipify.org:443 HTTP/1.1\r\nHost: api.ipify.org:443\r\n"+auth+"\r\n");
        stream.Write(request,0,request.Length);
        var bytes=new List<byte>();
        while(bytes.Count<16384){int b=stream.ReadByte();if(b<0)throw new IOException("Resposta CONNECT incompleta.");bytes.Add((byte)b);int n=bytes.Count;if(n>=4&&bytes[n-4]==13&&bytes[n-3]==10&&bytes[n-2]==13&&bytes[n-1]==10){string first=Encoding.ASCII.GetString(bytes.ToArray()).Split('\r')[0];if(!Regex.IsMatch(first,@"^HTTP/1\.[01] 200(?: |$)"))throw new InvalidOperationException("HTTPS CONNECT recusado. Confira protocolo, porta e credenciais.");return;}}
        throw new IOException("Cabeçalhos CONNECT excedem o limite.");
    }
    public static void CheckUdp(string host,int port,string user,string password,string protocol)
    {
        if(protocol=="HTTPS")throw new ArgumentException("Chamadas Discord exigem UDP; HTTPS CONNECT não suporta UDP.");
        try{
        using(var client=new TcpClient(AddressFamily.InterNetwork)){
            if(!client.ConnectAsync(host,port).Wait(10000))throw new IOException("Timeout UDP.");
            using(var stream=client.GetStream()){
                stream.ReadTimeout=stream.WriteTimeout=5000;Authenticate(stream,user,password);
                byte[] req={5,3,0,1,0,0,0,0,0,0};stream.Write(req,0,req.Length);
                var reply=ReadExact(stream,4);if(reply[0]!=5||reply[1]!=0||reply[2]!=0)throw new IOException("UDP ASSOCIATE recusado.");
                if(reply[3]!=1)throw new IOException("Relay UDP precisa retornar IPv4.");
                var address=ReadExact(stream,4);var ports=ReadExact(stream,2);int relayPort=(ports[0]<<8)|ports[1];
                var relay=new IPAddress(address);if(relay.Equals(IPAddress.Any))relay=IPAddress.Parse(host);
                if(relayPort==0)throw new IOException("Porta do relay inválida.");
                using(var udp=new UdpClient(AddressFamily.InterNetwork)){
                    udp.Client.ReceiveTimeout=5000;udp.Connect(relay,relayPort);
                    byte[] id=new byte[2];using(var random=System.Security.Cryptography.RandomNumberGenerator.Create())random.GetBytes(id);
                    // A DNS query through the relay proves that UDP returns, beyond negotiation.
                    byte[] packet={0,0,0,1,1,1,1,1,0,53,id[0],id[1],1,0,0,1,0,0,0,0,0,0,3,97,112,105,5,105,112,105,102,121,3,111,114,103,0,0,1,0,1};
                    udp.Send(packet,packet.Length);IPEndPoint sender=null;var response=udp.Receive(ref sender);
                    int offset=response.Length>=4&&response[3]==1?10:response.Length>=4&&response[3]==4?22:response.Length>=5&&response[3]==3?7+response[4]:-1;
                    if(offset<0||response.Length<offset+12||response[0]!=0||response[1]!=0||response[2]!=0||response[offset]!=id[0]||response[offset+1]!=id[1]||(response[offset+2]&128)==0||(response[offset+3]&15)!=0)throw new IOException("Relay não retornou UDP válido.");
                }
            }
        }
        }catch(Exception ex){throw new InvalidOperationException("Esta SOCKS5 não confirmou tráfego UDP. As chamadas Discord podem ficar em Sem rota. Use uma proxy/VPN com UDP ou desmarque o Discord dos aplicativos protegidos. Detalhe: "+ex.Message);}
    }

    public static string Attempt(Func<string> f)
    {
        try
        {
            return f();
        }
        catch(AuthenticationException)
        {
            return "falha TLS";
        }
        catch(WebException)
        {
            return "falha na consulta HTTPS";
        }
        catch(SocketException)
        {
            return "conexão indisponível";
        }
        catch(IOException)
        {
            return "conexão interrompida / tempo esgotado";
        }
        catch(Exception ex)
        {
            return ex is InvalidOperationException?ex.Message:"não foi possível concluir";
        }
    }
    static string ValidIp(string value)
    {
        IPAddress ip;
        if(!IPAddress.TryParse(value.Trim(),out ip))throw new InvalidOperationException("resposta de IP inválida");
        return ip.ToString();
    }
    public static string DirectIp()
    {
        var req=(HttpWebRequest)WebRequest.Create("https://api.ipify.org/");
        req.Proxy=null;
        req.Timeout=10000;
        req.ReadWriteTimeout=10000;
        req.AllowAutoRedirect=false;
        using(var res=(HttpWebResponse)req.GetResponse())
        {
            if(res.StatusCode!=HttpStatusCode.OK)throw new IOException();
            using(var reader=new StreamReader(res.GetResponseStream()))
            {
                char[] chars=new char[128];
                int n=reader.Read(chars,0,chars.Length);
                return ValidIp(new string(chars,0,n));
            }
        }
    }
    static byte[] ReadExact(Stream s,int n)
    {
        byte[] b=new byte[n];
        int i=0;
        while(i<n)
        {
            int got=s.Read(b,i,n-i);
            if(got==0)throw new IOException("Resposta incompleta");
            i+=got;
        }
        return b;
    }
    static void Authenticate(Stream s,string u,string pw)
    {
        RoutingConfiguration.ValidateProxy("127.0.0.1",1080,u,pw);
        byte method=(byte)(u.Length>0?2:0);
        byte[] greeting=
        {
            5,1,method
        };
        s.Write(greeting,0,greeting.Length);
        byte[] reply=ReadExact(s,2);
        if(reply[0]!=5||reply[1]!=method)throw new InvalidOperationException("método SOCKS5 recusado");
        if(method==2)
        {
            byte[] ub=Encoding.UTF8.GetBytes(u),pb=Encoding.UTF8.GetBytes(pw);
            var a=new List<byte>
            {
                1,(byte)ub.Length
            };
            a.AddRange(ub);
            a.Add((byte)pb.Length);
            a.AddRange(pb);
            s.Write(a.ToArray(),0,a.Count);
            reply=ReadExact(s,2);
            if(reply[0]!=1||reply[1]!=0)throw new InvalidOperationException("autenticação recusada");
        }
        }
    public static void Handshake(Stream s,string u,string pw)
    {
        Authenticate(s,u,pw);
        byte[] reply;
        byte[] dest=Encoding.ASCII.GetBytes("api.ipify.org");
        var request=new List<byte>
        {
            5,1,0,3,(byte)dest.Length
        };
        request.AddRange(dest);
        request.Add(1);
        request.Add(187);
        s.Write(request.ToArray(),0,request.Count);
        reply=ReadExact(s,4);
        if(reply[0]!=5||reply[2]!=0)throw new InvalidOperationException("resposta SOCKS5 inválida");
        if(reply[1]!=0)throw new InvalidOperationException("proxy recusou o destino ("+reply[1]+")");
        int size=reply[3]==1?4:reply[3]==4?16:reply[3]==3?ReadExact(s,1)[0]:-1;
        if(size<1)throw new InvalidOperationException("endereço SOCKS5 inválido");
        ReadExact(s,size+2);
    }
    public static string ProxyIp(string h,int p,string u,string pw)
    {
        return ProxyIp(h,p,u,pw,"SOCKS5");
    }
    public static string ProxyIp(string h,int p,string u,string pw,string protocol)
    {
        return ProxyIp(h,p,u,pw,protocol,true,"");
    }
    public static string ProxyIp(string h,int p,string u,string pw,string protocol,bool proxyTls,string tlsName)
    {
        RoutingConfiguration.ValidateProxy(h,p,u,pw);
        if(protocol!="SOCKS5"&&protocol!="HTTPS"&& !String.IsNullOrEmpty(protocol))throw new ArgumentException("Protocolo inválido.");
        using(var client=new TcpClient(AddressFamily.InterNetwork))
        {
            var connect=client.ConnectAsync(h,p);
            if(!connect.Wait(10000))throw new IOException("Timeout");
            client.ReceiveTimeout=10000;
            client.SendTimeout=10000;
            using(var stream=client.GetStream())
            {
                stream.ReadTimeout=10000;
                stream.WriteTimeout=10000;
                Stream tunnel=stream;
                if(protocol=="HTTPS"){if(proxyTls){var outer=new SslStream(stream,false);tunnel=outer;outer.ReadTimeout=outer.WriteTimeout=10000;outer.AuthenticateAsClient(String.IsNullOrWhiteSpace(tlsName)?h:tlsName,null,SslProtocols.Tls12,true);}HttpConnect(tunnel,u,pw);}
                else Handshake(stream,u,pw);
                using(tunnel)
                using(var tls=new SslStream(tunnel,false))
                {
                    tls.ReadTimeout=10000;
                    tls.WriteTimeout=10000;
                    tls.AuthenticateAsClient("api.ipify.org",null,SslProtocols.Tls12,true);
                    byte[] request=Encoding.ASCII.GetBytes("GET / HTTP/1.0\r\nHost: api.ipify.org\r\nConnection: close\r\n\r\n");
                    tls.Write(request,0,request.Length);
                    using(var reader=new StreamReader(tls,Encoding.ASCII))
                    {
                        string first=reader.ReadLine();
                        if(first==null||!Regex.IsMatch(first,@"^HTTP/1\.[01] 200(?: |$)"))throw new InvalidOperationException("consulta de IP recusada");
                        int total=first.Length;
                        string line;
                        while((line=reader.ReadLine())!="")
                        {
                            if(line==null||(total+=line.Length)>16384)throw new IOException();
                            if(line.StartsWith("Transfer-Encoding:",StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("resposta HTTP não suportada");
                        }
                        char[] body=new char[128];
                        int n=reader.Read(body,0,body.Length);
                        return ValidIp(new string(body,0,n));
                    }
                }
            }
        }
    }
}

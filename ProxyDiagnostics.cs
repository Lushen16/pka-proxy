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
    public static void Handshake(Stream s,string u,string pw)
    {
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
                Handshake(stream,u,pw);
                using(var tls=new SslStream(stream,false))
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

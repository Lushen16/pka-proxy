using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Security.AccessControl;
using System.ServiceProcess;
using System.Text;
using System.Web.Script.Serialization;
using Microsoft.Win32;

public sealed class EngineRequest
{
    public RoutingOptions Route;
    public string Host, User, Password;
    public int Port;
}
public static class EngineController
{
    const string ServiceName="ProxiFyreService";
    public static string Status()
    {
        try { using(var s=new ServiceController(ServiceName)) return s.Status==ServiceControllerStatus.Running?"Motor ativo • roteamento ainda deve ser verificado":"Motor instalado • desconectado"; }
        catch { return "Motor não instalado • rede normal"; }
    }
    public static void RunElevated(EngineRequest request, bool stop)
    {
        Directory.CreateDirectory(AppPaths.Root);
        string path=Path.Combine(AppPaths.Root,"engine-"+Guid.NewGuid().ToString("N")+".bin");
        try
        {
            if(!stop) File.WriteAllBytes(path,ProtectedData.Protect(Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(request)),null,DataProtectionScope.CurrentUser));
            using(var p=Process.Start(new ProcessStartInfo(System.Reflection.Assembly.GetExecutingAssembly().Location,
                stop?"--engine-stop":"--engine-apply \""+path+"\"") {UseShellExecute=true,Verb="runas"}))
            {
                p.WaitForExit();
                if(p.ExitCode!=0)throw new InvalidOperationException("A operação não foi concluída. Confira a mensagem do motor; se pediu reinicialização, reinicie o Windows.");
            }
        }
        finally { if(File.Exists(path))File.Delete(path); }
    }
    static string EnginePath()
    {
        using(var k=Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\"+ServiceName))
        {
            if(k==null)return null;
            string raw=Convert.ToString(k.GetValue("ImagePath"));
            string exe=raw.StartsWith("\"")?raw.Split('"')[1]:raw.Split(' ')[0];
            exe=Path.GetFullPath(Environment.ExpandEnvironmentVariables(exe));
            string expected=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"ProxiFyre","ProxiFyre.exe");
            if(!String.Equals(exe,expected,StringComparison.OrdinalIgnoreCase)||!File.Exists(exe))
                throw new InvalidOperationException("Há uma instalação antiga ou em outro local. Instale o ProxiFyre oficial em Program Files antes de continuar.");
            return exe;
        }
    }
    static void Install()
    {
        string arch=Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
        string token=String.Equals(arch,"ARM64",StringComparison.OrdinalIgnoreCase)?"arm64":Environment.Is64BitOperatingSystem?"x64":"x86";
        string hash=token=="x64"?"c08cbb5c15acd04d77d7c330712ae366d2a9a8e2a290586e8fe9aa73c78a1908":token=="arm64"?"d7b949c27c9462af5d0d27e2a162f18ac14d21cb7b8cca4300af4e2c084c1dc8":"6e4e3b6fd0cad53ad8767a60bef0fabc2d3b0f4c57d660623344ce9f8c47b9a5";
        // Download into the elevated user's temporary directory and verify again before execution.
        string folder=Path.Combine(Path.GetTempPath(),"PKA-engine-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var acl=new DirectorySecurity();acl.SetAccessRuleProtection(true,false);
        foreach(string sid in new[]{"S-1-5-18","S-1-5-32-544"})acl.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(sid),FileSystemRights.FullControl,InheritanceFlags.ContainerInherit|InheritanceFlags.ObjectInherit,PropagationFlags.None,AccessControlType.Allow));
        Directory.SetAccessControl(folder,acl);
        string setup=Path.Combine(folder,"setup.exe");
        try
        {
            using(var web=new WebClient()) { web.Proxy=null; web.DownloadFile("https://github.com/wiresock/proxifyre/releases/download/v2.6.1/ProxiFyre-2.6.1-win-"+token+"-setup.exe",setup); }
            using(var sha=SHA256.Create())using(var f=File.OpenRead(setup))
                if(BitConverter.ToString(sha.ComputeHash(f)).Replace("-","").ToLowerInvariant()!=hash)throw new InvalidOperationException("O instalador do motor falhou na verificação SHA-256.");
            using(var p=Process.Start(new ProcessStartInfo(setup) {UseShellExecute=true}))
            {
                p.WaitForExit();
                if(p.ExitCode==3010||p.ExitCode==1641)throw new InvalidOperationException("Reinicie o Windows para concluir a instalação do driver e clique em Conectar novamente.");
                if(p.ExitCode!=0)throw new InvalidOperationException("Instalação do motor cancelada ou falhou ("+p.ExitCode+").");
            }
        }
        finally { try {File.Delete(setup);Directory.Delete(folder);}catch{} }
    }
    public static int Apply(string[] args)
    {
        try
        {
            if(!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))throw new InvalidOperationException("É necessário permitir a elevação do Windows.");
            bool stop=args[0]=="--engine-stop";
            if(stop)
            {
                if(EnginePath()==null)return 0;
                using(var s=new ServiceController(ServiceName)) { if(s.Status!=ServiceControllerStatus.Stopped) {s.Stop();s.WaitForStatus(ServiceControllerStatus.Stopped,TimeSpan.FromSeconds(30));} }
                return 0;
            }
            if(args.Length!=2)throw new ArgumentException("Pedido inválido.");
            var request=new JavaScriptSerializer().Deserialize<EngineRequest>(Encoding.UTF8.GetString(ProtectedData.Unprotect(File.ReadAllBytes(args[1]),null,DataProtectionScope.CurrentUser)));
            string json=RoutingConfiguration.Build(request.Route,request.Host,request.Port,request.User,request.Password,true);
            // Check credentials before changing any running routing rule.
            ProxyDiagnostics.ProxyIp(request.Host,request.Port,request.User,request.Password);
            string exe=EnginePath();
            if(exe==null) { Install();exe=EnginePath(); }
            if(exe==null)throw new InvalidOperationException("Motor não instalado. Conclua o instalador e tente novamente.");
            string config=Path.Combine(Path.GetDirectoryName(exe),"app-config.json");
            byte[] previous=File.Exists(config)?File.ReadAllBytes(config):null;
            using(var s=new ServiceController(ServiceName))
            {
                bool wasRunning=s.Status==ServiceControllerStatus.Running;
                if(s.Status!=ServiceControllerStatus.Stopped) {s.Stop();s.WaitForStatus(ServiceControllerStatus.Stopped,TimeSpan.FromSeconds(30));}
                try
                {
                    File.WriteAllText(config,json,new UTF8Encoding(false));
                    var permissions=new FileSecurity();permissions.SetAccessRuleProtection(true,false);
                    foreach(string sid in new[]{"S-1-5-18","S-1-5-32-544"})permissions.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(sid),FileSystemRights.FullControl,AccessControlType.Allow));
                    File.SetAccessControl(config,permissions);
                    s.Start();s.WaitForStatus(ServiceControllerStatus.Running,TimeSpan.FromSeconds(30));
                    System.Threading.Thread.Sleep(2000);s.Refresh();
                    if(s.Status!=ServiceControllerStatus.Running)throw new InvalidOperationException("O motor parou ao iniciar. Verifique seus logs.");
                }
                catch
                {
                    s.Refresh();if(s.Status!=ServiceControllerStatus.Stopped){s.Stop();s.WaitForStatus(ServiceControllerStatus.Stopped,TimeSpan.FromSeconds(30));}
                    if(previous!=null)File.WriteAllBytes(config,previous);else File.Delete(config);
                    if(wasRunning)s.Start();
                    throw;
                }
            }
            return 0;
        }
        catch(Exception e) {System.Windows.Forms.MessageBox.Show(e.Message,"PKAproxy • motor de rede");return 1;}
    }
    public static string RoutedIp()
    {
        string root=Path.Combine(AppPaths.Root,"probe");Directory.CreateDirectory(root);
        string exe=Path.Combine(root,"PKArouteProbe.exe"),result=Path.Combine(root,Guid.NewGuid().ToString("N")+".txt");
        using(var source=System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("PKArouteProbe.exe"))
        using(var target=File.Create(exe))source.CopyTo(target);
        try
        {
            using(var p=Process.Start(new ProcessStartInfo(exe,"\""+result+"\""){UseShellExecute=false,CreateNoWindow=true}))
            { if(!p.WaitForExit(20000)){p.Kill();throw new IOException("A consulta de roteamento expirou.");}if(p.ExitCode!=0)throw new IOException("Não foi possível consultar o IP do processo de teste."); }
            return File.ReadAllText(result).Trim();
        }
        finally {if(File.Exists(result))File.Delete(result);}
    }
}

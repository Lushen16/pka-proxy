using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
public static class UpdateInstaller
{
    public static void Start(PendingUpdate update)
    {
        string target=System.Reflection.Assembly.GetExecutingAssembly().Location;
        UpdateService.VerifyFile(Path.Combine(update.Folder,"PKA-Proxy.exe"),update.Manifest);
        string writable=target+".write-test-"+Guid.NewGuid().ToString("N");
        using(File.Create(writable)){}File.Delete(writable);
        string helper=Path.Combine(update.Folder,"PKA-Update-Helper.exe");
        File.Copy(target,helper,true);
        string ready=Path.Combine(update.Folder,"ready"),cancel=Path.Combine(update.Folder,"cancel");
        if(File.Exists(ready))File.Delete(ready);if(File.Exists(cancel))File.Delete(cancel);
        string args="--apply-update "+Process.GetCurrentProcess().Id+" "+Process.GetCurrentProcess().StartTime.ToUniversalTime().Ticks+" "+Quote(target)+" "+Quote(update.Folder);
        using(var child=Process.Start(new ProcessStartInfo(helper,args)
        {
            UseShellExecute=false,CreateNoWindow=true,WorkingDirectory=update.Folder
        })){
            var timer=Stopwatch.StartNew();
            while(timer.ElapsedMilliseconds<10000){if(File.Exists(ready))return;if(child.HasExited)throw new IOException("O helper recusou a atualização.");Thread.Sleep(100);}
            File.WriteAllText(cancel,"cancel");
            throw new IOException("O helper não confirmou a atualização.");
        }
    }
    static string Quote(string value)
    {
        if(value.Contains("\""))throw new ArgumentException();
        return "\""+value+"\"";
    }
    public static int Apply(string[] args)
    {
        try
        {
            if(args.Length!=5)throw new ArgumentException("Argumentos do atualizador inválidos.");
            string target=Path.GetFullPath(args[3]),folder=Path.GetFullPath(args[4]);
            if(!Path.GetFileName(target).Equals("PKA-Proxy.exe",StringComparison.OrdinalIgnoreCase) && !Path.GetFileName(target).Equals("PKAproxy.exe",StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Destino inválido.");
            var m=UpdateService.VerifyManifest(File.ReadAllBytes(Path.Combine(folder,"update.json")),File.ReadAllBytes(Path.Combine(folder,"update.sig")),UpdateTrust.PublicKey);
            string source=Path.Combine(folder,"PKA-Proxy.exe");
            UpdateService.VerifyFile(source,m);
            if(new Version(m.version)<=UpdateService.Current)throw new InvalidOperationException("Atualização não é mais recente.");
            var parent=Process.GetProcessById(Int32.Parse(args[1]));
            if(parent.StartTime.ToUniversalTime().Ticks!=Int64.Parse(args[2]))throw new InvalidOperationException("Processo de origem expirado.");
            if(!String.Equals(Path.GetFullPath(parent.MainModule.FileName),target,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Processo de destino diferente do aplicativo.");
            string ready=Path.Combine(folder,"ready");
            File.WriteAllText(ready,"ready");
            if(!parent.WaitForExit(30000))throw new InvalidOperationException("Feche o aplicativo para atualizar.");
            if(File.Exists(Path.Combine(folder,"cancel")))throw new InvalidOperationException("Atualização cancelada.");
            string backup=ReplaceBinary(source,target);
            try
            {
                Process.Start(new ProcessStartInfo(target)
                {
                    UseShellExecute=true,WorkingDirectory=Path.GetDirectoryName(target)
                });
            }
            catch
            {
                ReplaceBinary(backup,target);
                Process.Start(new ProcessStartInfo(target,"--update-failed"){UseShellExecute=true,WorkingDirectory=Path.GetDirectoryName(target)});
                throw;
            }
            // The replacement is already running; keep no second launcher in the install folder.
            // Cleanup is best effort and never turns a successful restart into a rollback.
            try {File.Delete(backup);}catch{}
            try {File.Delete(source);}catch{}
            return 0;
        }
        catch(Exception ex)
        {
            MessageBox.Show("Atualização não concluída. A configuração foi preservada.\n"+ex.Message,"Litfix — Atualização");
            return 1;
        }
    }
    public static string ReplaceBinary(string source,string target)
    {
        // Replace only the binary, never app-config.json or other user files.
        string next=target+".new-"+Guid.NewGuid().ToString("N"),backup=target+".backup-"+Guid.NewGuid().ToString("N");
        File.Copy(source,next,false);
        try
        {
            File.Replace(next,target,backup,true);
            return backup;
        }
        finally
        {
            if(File.Exists(next))File.Delete(next);
        }
    }
}

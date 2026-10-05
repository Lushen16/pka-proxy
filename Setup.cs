using System;using System.IO;using System.Reflection;using System.Security.Cryptography;using System.Diagnostics;using System.Drawing;using System.Windows.Forms;
[assembly: AssemblyVersion("1.2.0.0")]
[assembly: AssemblyProduct("PKAproxy Setup")]
public static class InstallerCore
{
    public static string Install(string destination, bool shortcuts)
    {
        using(var net=Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full")) { if(net==null || Convert.ToInt32(net.GetValue("Release",0))<461808) throw new InvalidOperationException("Instale o .NET Framework 4.7.2 ou superior antes de continuar."); }
        destination=Path.GetFullPath(destination);
        if(!Path.GetFileName(destination).Equals("PKAproxy",StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("A pasta de instalação deve se chamar PKAproxy.");
        string target=Path.Combine(destination,"PKAproxy.exe");
        Directory.CreateDirectory(destination);
        foreach(var process in Process.GetProcessesByName("PKAproxy"))
        {
            try { if(String.Equals(process.MainModule.FileName,target,StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Feche o PKAproxy antes de instalar."); }
            catch(System.ComponentModel.Win32Exception) { }
            finally { process.Dispose(); }
        }
        byte[] payload;
        using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("PKAproxy.Payload"))
        using(var memory=new MemoryStream()) { stream.CopyTo(memory); payload=memory.ToArray(); }
        using(var sha=SHA256.Create())
        {
            string actual=BitConverter.ToString(sha.ComputeHash(payload)).Replace("-","");
            if(!actual.Equals(InstallerIntegrity.PayloadHash,StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("O pacote de instalação está danificado.");
        }
        string pending=Path.Combine(destination,"PKAproxy.installing");
        File.WriteAllBytes(pending,payload);
        try
        {
            if(File.Exists(target)) File.Replace(pending,target,target+".previous",true);
            else File.Move(pending,target);
        }
        finally { if(File.Exists(pending)) File.Delete(pending); }
        if(shortcuts)
        {
            string programs=Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            CreateShortcut(Path.Combine(programs,"PKAproxy.lnk"),target,destination);
            CreateShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),"PKAproxy.lnk"),target,destination);
        }
        return target;
    }
    static void CreateShortcut(string path,string target,string directory)
    {
        Type shellType=Type.GetTypeFromProgID("WScript.Shell");
        object shell=Activator.CreateInstance(shellType);
        object shortcut=shellType.InvokeMember("CreateShortcut",BindingFlags.InvokeMethod,null,shell,new object[]{path});
        try
        {
            Type t=shortcut.GetType();
            t.InvokeMember("TargetPath",BindingFlags.SetProperty,null,shortcut,new object[]{target});
            t.InvokeMember("WorkingDirectory",BindingFlags.SetProperty,null,shortcut,new object[]{directory});
            t.InvokeMember("Description",BindingFlags.SetProperty,null,shortcut,new object[]{"PKA PROXY"});
            t.InvokeMember("IconLocation",BindingFlags.SetProperty,null,shortcut,new object[]{target+",0"});
            t.InvokeMember("Save",BindingFlags.InvokeMethod,null,shortcut,null);
        }
        finally { System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell); }
    }
}
public sealed class SetupForm:Form
{
    Button install; Label message; CheckBox shortcuts,launch;
    public SetupForm()
    {
        Text="Instalar PKAproxy";ClientSize=new Size(650,425);BackColor=Color.FromArgb(15,17,23);ForeColor=Color.White;Font=new Font("Segoe UI",10);StartPosition=FormStartPosition.CenterScreen;FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;
        Controls.Add(new Label{Text="PKA PROXY",Left=26,Top=25,Width=600,Height=48,Font=new Font("Segoe UI",26,FontStyle.Bold),ForeColor=Color.FromArgb(250,190,55)});
        Controls.Add(new Label{Text="Instalação para seu usuário Windows",Left=29,Top=82,Width=590,Height=25,ForeColor=Color.LightGray});
        Controls.Add(new Label{Text="PASTA DO APLICATIVO",Left=29,Top=133,Width=590,Height=24,ForeColor=Color.LightGray});
        Controls.Add(new TextBox{Text=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"PKAproxy"),Left=29,Top=163,Width=591,ReadOnly=true,BackColor=Color.FromArgb(27,30,39),ForeColor=Color.White});
        shortcuts=new CheckBox{Text="Criar atalhos na Área de Trabalho e no menu Iniciar",Left=29,Top=207,Width=590,Height=28,Checked=true};Controls.Add(shortcuts);
        launch=new CheckBox{Text="Abrir PKAproxy ao terminar",Left=29,Top=242,Width=590,Height=28,Checked=true};Controls.Add(launch);
        message=new Label{Text="O Windows precisa ter .NET Framework 4.7.2 ou superior.\nO roteamento requer ProxiFyre / Windows Packet Filter instalados separadamente.\nEste instalador não instala o motor ou driver e não altera o proxy do Windows.",Left=29,Top=286,Width=591,Height=69,ForeColor=Color.LightGray,Font=new Font("Segoe UI",9)};Controls.Add(message);
        install=new Button{Text="Instalar PKAproxy",Left=29,Top=367,Width=230,Height=40,FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(250,190,55),ForeColor=Color.FromArgb(20,22,28),Font=new Font("Segoe UI",11,FontStyle.Bold)};install.FlatAppearance.BorderSize=0;install.Click+=(s,e)=>Install();Controls.Add(install);
    }
    void Install()
    {
        try
        {
            install.Enabled=false;
            string target=InstallerCore.Install(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"PKAproxy"),shortcuts.Checked);
            if(launch.Checked)Process.Start(new ProcessStartInfo(target){UseShellExecute=true,WorkingDirectory=Path.GetDirectoryName(target)});
            MessageBox.Show(this,"PKAproxy instalado. Suas configurações anteriores foram preservadas.","Instalação concluída");Close();
        }
        catch(Exception ex){message.Text="Instalação não concluída: "+ex.Message;install.Enabled=true;}
    }
    [STAThread] static int Main(string[] args)
    {
        if(args.Length==2&&args[0]=="--test-extract") { try { InstallerCore.Install(args[1],false);return 0; } catch { return 1; } }
        if(args.Length==1&&args[0]=="--install-no-start") { try { InstallerCore.Install(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"PKAproxy"),true);return 0; } catch { return 1; } }
        Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
        if(args.Length==2&&args[0]=="--preview") { using(var f=new SetupForm()){f.Show();f.Update();using(var b=new Bitmap(f.Width,f.Height)){f.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height));b.Save(args[1]);}}return 0; }
        Application.Run(new SetupForm());return 0;
    }
}


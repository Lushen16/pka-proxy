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
public class MainForm : Form
{
    TextBox clientPathBox=new TextBox(),proxyHostBox=new TextBox(),usernameBox=new TextBox(),passwordBox=new TextBox();
    NumericUpDown proxyPortBox=new NumericUpDown();
    RoutingOptions routingOptions=new RoutingOptions();
    Button checkProxyButton,connectButton,disconnectButton;
    Label connectionStatusLabel,directIpLabel,proxyIpLabel,routingStatusLabel;
    Color bg=Color.FromArgb(15,17,23),card=Color.FromArgb(27,30,39),muted=Color.FromArgb(167,173,188),accent=Color.FromArgb(250,190,55);
    public MainForm()
    {
        Text = "PKAproxy";
        ClientSize = new Size(800, 850);
        BackColor = bg;
        ForeColor = Color.White;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        var mark = new PictureBox { Left=28, Top=28, Width=58, Height=58, Image=UiTheme.BrandMark(58), SizeMode=PictureBoxSizeMode.Zoom };
        Controls.Add(mark);
        AddLabel("PKA", 102, 20, 110, 45, 28, accent);
        AddLabel("PROXY", 213, 20, 240, 45, 28, Color.White);
        AddLabel("CONEXÃO SOB CONTROLE", 105, 70, 410, 25, 9, muted);
        var updates = CreateButton("Atualizações", 590, 30, 182, false);
        updates.Click += (sender, e) => ShowUpdates(null);
        Controls.Add(updates);
        AddLabel("SOCKS5  /  WEBSHARE  /  WINDOWS", 30, 112, 620, 24, 9, muted);
        Shown += async (sender, e) => { if (Environment.GetCommandLineArgs().Length == 1) await CheckForUpdatesAsync(); };

        var setup = new Panel { Left=28, Top=148, Width=744, Height=306, BackColor=card };
        Controls.Add(setup);
        setup.Controls.Add(new Label { Text="01   CONFIGURAR CONEXÃO", Left=24, Top=15, Width=690, Height=23, ForeColor=accent, Font=new Font("Segoe UI", 10, FontStyle.Bold) });
        CreateField(setup, "CLIENTE FINAL DO JOGO", clientPathBox, 44);
        clientPathBox.Width = 505;
        var choose = UiTheme.PrimaryButton("Escolher", 568, 60, 152);
        choose.Click += (sender, e) => { string path = ExecutablePicker.Select(this); if (path != null) clientPathBox.Text = path; };
        setup.Controls.Add(choose);
        CreateField(setup, "IP OU HOST DO PROXY", proxyHostBox, 106);
        proxyHostBox.Width = 505;
        setup.Controls.Add(new Label { Text="PORTA", Left=568, Top=106, Width=140, ForeColor=muted, Font=new Font("Segoe UI",9) });
        proxyPortBox.SetBounds(568, 128, 152, 28);
        proxyPortBox.Minimum=1; proxyPortBox.Maximum=65535; proxyPortBox.Value=1080;
        setup.Controls.Add(proxyPortBox); ApplyInputStyle(proxyPortBox);
        CreateField(setup, "USUÁRIO", usernameBox, 168); usernameBox.Width=333;
        setup.Controls.Add(new Label { Text="SENHA", Left=383, Top=168, Width=335, ForeColor=muted, Font=new Font("Segoe UI",9) });
        passwordBox.SetBounds(383,190,337,28); passwordBox.UseSystemPasswordChar=true;
        setup.Controls.Add(passwordBox); ApplyInputStyle(passwordBox);
        setup.Controls.Add(new Label { Text="Escolha o cliente que o launcher abre. Filhos não herdam a regra.\nInclua o launcher ou configure todos os apps em Roteamento.", Left=24, Top=242, Width=695, Height=45, ForeColor=muted, Font=new Font("Segoe UI",9) });

        var result = new Panel { Left=28, Top=472, Width=744, Height=152, BackColor=card };
        Controls.Add(result);
        connectionStatusLabel = new Label { Text="02   VERIFICAR PROXY", Left=24, Top=16, Width=530, Height=28, ForeColor=accent, Font=new Font("Segoe UI",12,FontStyle.Bold) };
        directIpLabel = new Label { Text="Rede direta: —", Left=24, Top=54, Width=510, Height=25 };
        proxyIpLabel = new Label { Text="Via SOCKS5: —", Left=24, Top=83, Width=510, Height=25 };
        result.Controls.Add(connectionStatusLabel); result.Controls.Add(directIpLabel); result.Controls.Add(proxyIpLabel);
        checkProxyButton = CreateButton("Check proxy",568,56,152,true);
        checkProxyButton.Click += async (sender,e) => await CheckProxyAsync();
        result.Controls.Add(checkProxyButton);
        result.Controls.Add(new Label { Text="O Check verifica esta conexão, não o tráfego do jogo.",Left=24,Top=121,Width=695,Height=24,ForeColor=muted,Font=new Font("Segoe UI",9) });

        var save = CreateButton("Salvar configuração",28,645,235,true);
        save.Click += (sender,e) => SaveProxyConfig(); Controls.Add(save);
        var manager = CreateButton("Motor de rede",282,645,235,false);
        manager.Text="Verificar Global"; manager.Click += async (sender,e) => await VerifyRoutingAsync(); Controls.Add(manager);
        var routing = CreateButton("Roteamento",537,645,235,false);
        routing.Click += (sender,e) =>
        {
            routingOptions.ClientPath = clientPathBox.Text;
            using (var dialog = new RoutingDialog(routingOptions))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    routingOptions=dialog.Options; clientPathBox.Enabled=choose.Enabled=!routingOptions.Global; UpdateRoutingStatus();
                }
            }
        };
        Controls.Add(routing);
        connectButton=CreateButton("Conectar / aplicar",28,699,355,true); connectButton.Click += async (sender,e) => await ConnectAsync(false); Controls.Add(connectButton);
        disconnectButton=CreateButton("Desconectar",402,699,370,false); disconnectButton.Click += async (sender,e) => await ConnectAsync(true); Controls.Add(disconnectButton);
        routingStatusLabel=new Label { Left=30,Top=749,Width=740,Height=25,ForeColor=muted };
        Controls.Add(routingStatusLabel);
        AddLabel("Conectar instala o motor oficial e aplica a regra (permissão de administrador).\nReabra o navegador após conectar. PKAproxy " + UpdateService.Current,30,790,740,43,9,muted);
        var saved = SessionStore.Load();
        if(saved != null)
        {
            clientPathBox.Text=saved.Game??""; proxyHostBox.Text=saved.Host??"";
            proxyPortBox.Value=saved.Port>=1&&saved.Port<=65535?saved.Port:1080;
            usernameBox.Text=saved.User??""; passwordBox.Text=saved.Password();
            routingOptions=saved.Route??new RoutingOptions(); clientPathBox.Enabled=choose.Enabled=!routingOptions.Global;
        }
        UpdateRoutingStatus();
        var monitor=new System.Windows.Forms.Timer {Interval=3000}; monitor.Tick+=(s,e)=>UpdateRoutingStatus(); monitor.Start(); FormClosed+=(s,e)=>monitor.Dispose();
        FormClosing += (sender,e) =>
        {
            try { SessionStore.Save(clientPathBox.Text,proxyHostBox.Text,(int)proxyPortBox.Value,usernameBox.Text,passwordBox.Text,routingOptions); }
            catch { /* Keep the app usable if profile storage is unavailable. */ }
        };
    }
    async Task ConnectAsync(bool stop)
    {
        var request=new EngineRequest { Route=routingOptions.Copy(),Host=proxyHostBox.Text.Trim(),Port=(int)proxyPortBox.Value,User=usernameBox.Text,Password=passwordBox.Text };
        request.Route.ClientPath=clientPathBox.Text;
        try
        {
            if(!stop)RoutingConfiguration.Build(request.Route,request.Host,request.Port,request.User,request.Password,true);
            connectButton.Enabled=disconnectButton.Enabled=false;
            routingStatusLabel.Text=stop?"Desconectando…":"Instalando / ativando motor… conclua os avisos do Windows";
            await Task.Run(()=>EngineController.RunElevated(request,stop));
            UpdateRoutingStatus();
            if(!stop&&request.Route.Global)await VerifyRoutingAsync();
            else if(!stop)MessageBox.Show("Motor ativo. Reabra o cliente do jogo para aplicar às novas conexões.");
        }
        catch(Exception e) {MessageBox.Show(e.Message,"PKAproxy");UpdateRoutingStatus();}
        finally {connectButton.Enabled=disconnectButton.Enabled=true;}
    }
    async Task VerifyRoutingAsync()
    {
        if(!routingOptions.Global){MessageBox.Show("Esta consulta verifica o modo Global. No modo jogo, o processo de teste continua na rede normal.");return;}
        checkProxyButton.Enabled=false;
        connectionStatusLabel.Text="Verificando outro processo…";
        try
        {
            string host=proxyHostBox.Text.Trim(),user=usernameBox.Text,password=passwordBox.Text;int port=(int)proxyPortBox.Value;
            var expectedTask=Task.Run(()=>ProxyDiagnostics.ProxyIp(host,port,user,password));
            var routedTask=Task.Run(()=>EngineController.RoutedIp());
            await Task.WhenAll(expectedTask,routedTask);
            proxyIpLabel.Text="Processo no Global: "+routedTask.Result;
            directIpLabel.Text="Saída esperada SOCKS5: "+expectedTask.Result;
            bool running=EngineController.Status().StartsWith("Motor ativo");
            bool matches=running&&routedTask.Result==expectedTask.Result;
            connectionStatusLabel.Text=matches?"Global verificado neste processo":"Global não confirmado • confira o motor";
            connectionStatusLabel.ForeColor=matches?Color.FromArgb(52,211,153):Color.FromArgb(248,113,113);
            MessageBox.Show(matches?"O processo de teste saiu pelo IP da proxy. Feche completamente e reabra o navegador para testar conexões novas. Este teste confirma HTTPS/TCP, não todos os protocolos.":"O IP do processo de teste não confirmou a proxy. Não considere o Global conectado.","Teste de roteamento");
        }
        catch(Exception e){connectionStatusLabel.Text="Global não confirmado";connectionStatusLabel.ForeColor=Color.FromArgb(248,113,113);MessageBox.Show(e.Message);}
        finally{checkProxyButton.Enabled=true;}
    }
    void OpenEngine()
    {
        try
        {
            using(var picker=new OpenFileDialog { Title="Selecione o ProxiFyre instalado",Filter="Executável ProxiFyre|*.exe" })
            {
                if(picker.ShowDialog(this)==DialogResult.OK)
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(picker.FileName) { UseShellExecute=true,WorkingDirectory=Path.GetDirectoryName(picker.FileName) });
            }
        }
        catch { MessageBox.Show("Não foi possível abrir o motor selecionado."); }
    }
    async Task CheckForUpdatesAsync()
    {
        var preferences=UpdatePreferences.Load();
        if(!preferences.CheckOnStartup||String.IsNullOrWhiteSpace(preferences.Repository))return;
        try
        {
            var available=await Task.Run(()=>UpdateService.Check(preferences.Repository));
            if(!IsDisposed&&available!=null)ShowUpdates(available);
        }
        catch
        {
            /* Keep startup usable offline. */
        }
    }
    void ShowUpdates(PendingUpdate available)
    {
        using(var dialog=new UpdateDialog(available))
        {
            dialog.ShowDialog(this);
            if(dialog.ExitForUpdate)Application.Exit();
        }
    }
    void UpdateRoutingStatus()
    {
        routingStatusLabel.Text=EngineController.Status()+" • "+(routingOptions.Global?"Global / todos os apps":"Cliente final"+(routingOptions.LauncherPath.Length>0?" + launcher":""))+" • TCP"+(routingOptions.Udp?" + UDP":"")+" • "+(routingOptions.Ipv6?"IPv4 + IPv6":"IPv4");
    }
    void AddLabel(string t,int x,int y,int w,int h,int size,Color color)
    {
        Controls.Add(new Label
        {
            Text=t,Left=x,Top=y,Width=w,Height=h,ForeColor=color,Font=new Font("Segoe UI",size)
        });
    }
    void ApplyInputStyle(Control c)
    {
        c.BackColor=bg;
        c.ForeColor=Color.White;
        c.Font=new Font("Segoe UI",11);
        if(c is TextBox)((TextBox)c).BorderStyle=BorderStyle.FixedSingle;
    }
    void CreateField(Panel panel,string name,Control c,int y)
    {
        panel.Controls.Add(new Label
        {
            Text=name,Left=24,Top=y,Width=500,ForeColor=muted,Font=new Font("Segoe UI",9)
        });
        c.SetBounds(24,y+22,676,28);
        ApplyInputStyle(c);
        panel.Controls.Add(c);
    }
    Button CreateButton(string t,int x,int y,int w,bool primary)
    {
        var b=new Button
        {
            Text=t,Left=x,Top=y,Width=w,Height=38,FlatStyle=FlatStyle.Flat,BackColor=primary?accent:card,ForeColor=primary?Color.FromArgb(20,22,28):Color.White,Cursor=Cursors.Hand,Font=new Font("Segoe UI",10,FontStyle.Bold)
        };
        b.FlatAppearance.BorderSize=0;
        return b;
    }
    async Task CheckProxyAsync()
    {
        string h=proxyHostBox.Text.Trim(),u=usernameBox.Text,pw=passwordBox.Text;
        int p=(int)proxyPortBox.Value;
        try
        {
            RoutingConfiguration.ValidateProxy(h,p,u,pw);
        }
        catch(Exception ex)
        {
            MessageBox.Show(ex.Message);
            return;
        }
        checkProxyButton.Enabled=false;
        connectionStatusLabel.Text="Verificando conexão…";
        connectionStatusLabel.ForeColor=Color.White;
        directIpLabel.Text="Rede direta: consultando…";
        proxyIpLabel.Text="Via SOCKS5: consultando…";
        try
        {
            var directTask=Task.Run(()=>ProxyDiagnostics.Attempt(()=>ProxyDiagnostics.DirectIp()));
            var proxyTask=Task.Run(()=>ProxyDiagnostics.Attempt(()=>ProxyDiagnostics.ProxyIp(h,p,u,pw)));
            await Task.WhenAll(directTask,proxyTask);
            if(IsDisposed)return;
            var d=directTask.Result;
            var r=proxyTask.Result;
            directIpLabel.Text="Rede direta: "+d;
            proxyIpLabel.Text="Via SOCKS5: "+r;
            IPAddress dip,rip;
            bool dok=IPAddress.TryParse(d,out dip),rok=IPAddress.TryParse(r,out rip);
            if(!rok)
            {
                connectionStatusLabel.Text="Proxy não confirmado • sem fallback direto";
                connectionStatusLabel.ForeColor=Color.FromArgb(248,113,113);
            }
            else if(!dok)
            {
                connectionStatusLabel.Text="SOCKS5 funciona • comparação indisponível";
                connectionStatusLabel.ForeColor=Color.FromArgb(251,191,36);
            }
            else if(dip.Equals(rip))
            {
                connectionStatusLabel.Text="SOCKS5 respondeu • mesmo IP de saída";
                connectionStatusLabel.ForeColor=Color.FromArgb(251,191,36);
            }
            else
            {
                connectionStatusLabel.Text="SOCKS5 funciona • roteamento não verificado";
                connectionStatusLabel.ForeColor=Color.FromArgb(52,211,153);
            }
        }
        finally
        {
            if(!IsDisposed)checkProxyButton.Enabled=true;
        }
    }
    void SaveProxyConfig()
    {
        try
        {
            routingOptions.ClientPath=clientPathBox.Text;
            string json=RoutingConfiguration.Build(routingOptions,proxyHostBox.Text,(int)proxyPortBox.Value,usernameBox.Text,passwordBox.Text,true);
            using(var d=new SaveFileDialog
            {
                Filter="Configuração JSON|*.json",FileName="app-config.json",OverwritePrompt=true,InitialDirectory=AppPaths.Root
            })if(d.ShowDialog()==DialogResult.OK)
            {
                File.WriteAllText(d.FileName,json,new UTF8Encoding(false));
                MessageBox.Show("Configuração salva: "+(routingOptions.Global?"Global / todos os aplicativos":"Cliente do jogo")+". Aplique e reinicie o serviço elevado no ProxiFyre. Salvar não ativa o roteamento.");
            }
        }
        catch(Exception ex)
        {
            MessageBox.Show(ex.Message,"Confira os campos");
        }
    }
}


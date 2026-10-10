using System;using System.IO;using System.Reflection;using System.Windows.Forms;
class StartupTests
{
 [STAThread]static int Main(string[] args)
 {
  bool expectFailure=args.Length>1&&args[1]=="--expect-failure";
  try{
   AppPaths.ProtectedRoot=Path.GetFullPath(args[0]);Directory.CreateDirectory(AppPaths.Root);
   string app=Path.Combine(AppPaths.Root,"Discord.exe"),other=Path.Combine(AppPaths.Root,"Game.exe");File.WriteAllText(app,"fixture, never executed");File.WriteAllText(other,"fixture, never executed");
   var route=new RoutingOptions();route.Apps.Add(new RegisteredApp{Path=app,Selected=true});route.Apps.Add(new RegisteredApp{Path=other,Selected=false});
   SessionStore.Save("","192.0.2.10",1080,"","",route,"SOCKS5");
   var presets=ProxyCatalog.Parse("192.0.2.11:6001:fixture-user:fixture-secret\n192.0.2.12:6002:fixture-user:secret:with:colons");
   ProxyCatalog.Save(presets);var stored=File.ReadAllBytes(AppPaths.SettingsFile("proxy-catalog.bin"));if(System.Text.Encoding.UTF8.GetString(stored).Contains("fixture-secret"))throw new Exception("Catalog credentials stored unencrypted.");
   if(ProxyCatalog.Load().Count!=2||ProxyCatalog.Load()[1].Password!="secret:with:colons")throw new Exception("Catalog import/roundtrip failed.");
   bool rejected=false;try{ProxyCatalog.Parse("192.0.2.11:0:user:secret");}catch(ArgumentException){rejected=true;}if(!rejected)throw new Exception("Invalid preset accepted.");
   ProxyCatalog.SaveSelection(new ProxySelectionState());
   Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
   using(var form=new MainForm()){
    if(expectFailure)throw new Exception("Baseline unexpectedly opened.");
    var flags=BindingFlags.Instance|BindingFlags.NonPublic;
    var apps=(CheckedListBox)typeof(MainForm).GetField("apps",flags).GetValue(form);
    var launch=(ComboBox)typeof(MainForm).GetField("launch",flags).GetValue(form);
    if(apps.Items.Count!=2||!apps.GetItemChecked(0)||apps.GetItemChecked(1)||launch.Items.Count!=1)throw new Exception("Saved selection changed during startup.");
    apps.SetItemChecked(1,true);if(launch.Items.Count!=2)throw new Exception("Selection refresh before handle creation failed.");
    IntPtr handle=form.Handle;apps.SetItemChecked(1,false);Application.DoEvents();if(launch.Items.Count!=1)throw new Exception("Selection refresh after handle creation failed.");
    var general=(ProxyPicker)typeof(MainForm).GetField("generalPicker",flags).GetValue(form);var discord=(ProxyPicker)typeof(MainForm).GetField("discordPicker",flags).GetValue(form);
    var host=(TextBox)typeof(MainForm).GetField("host",flags).GetValue(form);var discordHost=(TextBox)typeof(MainForm).GetField("discordHost",flags).GetValue(form);
    general.Automatic.Checked=true;general.Options.SelectedIndex=1;discord.Automatic.Checked=true;discord.Options.SelectedIndex=0;
    if(host.Text!="192.0.2.12"||discordHost.Text!="192.0.2.11"||!host.ReadOnly||!discordHost.ReadOnly)throw new Exception("Independent picker fill/lock failed.");
    general.Manual.Checked=true;if(host.Text!="192.0.2.10"||host.ReadOnly)throw new Exception("Manual fields not restored.");general.Automatic.Checked=true;
    var selected=ProxyCatalog.Selection();if(!selected.GeneralAutomatic||!selected.DiscordAutomatic||selected.GeneralIndex!=1||selected.DiscordIndex!=0)throw new Exception("Independent picker selection not persisted.");
   }
   using(var reopened=new MainForm()){
    var flags=BindingFlags.Instance|BindingFlags.NonPublic;var general=(ProxyPicker)typeof(MainForm).GetField("generalPicker",flags).GetValue(reopened);var host=(TextBox)typeof(MainForm).GetField("host",flags).GetValue(reopened);
    if(!general.Automatic.Checked||general.Options.SelectedIndex!=1||host.Text!="192.0.2.12")throw new Exception("Automatic choice not restored on reopen.");general.Manual.Checked=true;if(host.Text!="192.0.2.10")throw new Exception("Manual snapshot not preserved across reopen.");
   }
   Console.WriteLine("PASS: startup with saved selected apps, restored selection, and selection refresh before/after window handle creation. No tunnel activated.");return 0;
  }catch(InvalidOperationException ex){if(expectFailure&&ex.StackTrace.Contains("MarshaledInvoke")){Console.WriteLine("PASS: reproduced original startup crash before window handle creation.");return 0;}Console.Error.WriteLine(ex);return 1;}
  catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
 }
}

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
   Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
   using(var form=new MainForm()){
    if(expectFailure)throw new Exception("Baseline unexpectedly opened.");
    var flags=BindingFlags.Instance|BindingFlags.NonPublic;
    var apps=(CheckedListBox)typeof(MainForm).GetField("apps",flags).GetValue(form);
    var launch=(ComboBox)typeof(MainForm).GetField("launch",flags).GetValue(form);
    if(apps.Items.Count!=2||!apps.GetItemChecked(0)||apps.GetItemChecked(1)||launch.Items.Count!=1)throw new Exception("Saved selection changed during startup.");
    apps.SetItemChecked(1,true);if(launch.Items.Count!=2)throw new Exception("Selection refresh before handle creation failed.");
    IntPtr handle=form.Handle;apps.SetItemChecked(1,false);Application.DoEvents();if(launch.Items.Count!=1)throw new Exception("Selection refresh after handle creation failed.");
   }
   Console.WriteLine("PASS: startup with saved selected apps, restored selection, and selection refresh before/after window handle creation. No tunnel activated.");return 0;
  }catch(InvalidOperationException ex){if(expectFailure&&ex.StackTrace.Contains("MarshaledInvoke")){Console.WriteLine("PASS: reproduced original startup crash before window handle creation.");return 0;}Console.Error.WriteLine(ex);return 1;}
  catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
 }
}

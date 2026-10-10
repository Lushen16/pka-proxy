using System;using System.Drawing;using System.Net;using System.Windows.Forms;
static class Program
{
 [STAThread]static int Main(string[] args)
 {
  ServicePointManager.SecurityProtocol=SecurityProtocolType.Tls12;
  if(args.Length>0&&(args[0]=="--engine-apply"||args[0]=="--engine-stop"||args[0]=="--guard-release"))return EngineController.Apply(args);
  Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
  if(args.Length>=2&&args[0]=="--preview"){
   using(var form=new MainForm()){if(args.Length>2)form.PreviewPage(args[2]);form.Show();form.Update();using(var bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));bitmap.Save(args[1]);}}return 0;
  }
  Application.Run(new MainForm());return 0;
 }
}

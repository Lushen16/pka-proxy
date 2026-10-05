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

static class Program {
 [STAThread] static int Main(string[] args){
  ServicePointManager.SecurityProtocol=SecurityProtocolType.Tls12;
  if(args.Length>0&&args[0]=="--apply-update")return UpdateInstaller.Apply(args);
  Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
  if(args.Length==2&&args[0]=="--preview"){
   using(var form=new PkaProxy()){form.Show();form.Update();using(var bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));bitmap.Save(args[1]);}}return 0;
  }
  if(args.Length==2&&args[0]=="--preview-routing"){using(var form=new RoutingDialog(new RoutingOptions{Global=true,Udp=true,Ipv6=true})){form.Show();form.Update();using(var bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));bitmap.Save(args[1]);}}return 0;}
  if(args.Length==2&&args[0]=="--preview-updates"){using(var form=new UpdateDialog(null)){form.Show();form.Update();using(var bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));bitmap.Save(args[1]);}}return 0;}
  Application.Run(new PkaProxy());return 0;
 }
}


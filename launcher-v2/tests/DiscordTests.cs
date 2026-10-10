using System;using System.IO;using System.Text;using System.Xml;
public static class EngineController {public static string Mode(){return "NONE";}public static string Status(){return "Motor desconectado";}public static string Command(string text,int timeout){throw new InvalidOperationException("No real engine in fixture");}}
public static class TunRuntime {public static int Run(string[] args){throw new InvalidOperationException("No real daemon in fixture");}}
public static class DiscordTests
{
 public static void Run(string root)
 {
  string exe=Path.Combine(root,"Discord.exe");File.WriteAllText(exe,"fixture, never executed");
  var p=new DiscordProfile{Host="192.0.2.10",Port=1080,User="fixture-user",Password="fixture-secret",DiscordPath=exe};DiscordProxy.Validate(p);
  var bytes=DiscordProxy.Encode(p);if(Encoding.UTF8.GetString(bytes).Contains("fixture-secret"))throw new Exception("Password stored unencrypted");var decoded=DiscordProxy.Decode(bytes);if(decoded.Host!=p.Host||decoded.Password!=p.Password||decoded.DiscordPath!=exe)throw new Exception("Discord profile roundtrip failed");
  bytes[0]^=255;bool failed=false;try{DiscordProxy.Decode(bytes);}catch{failed=true;}if(!failed)throw new Exception("Corrupt profile accepted");
  p.DiscordPath=Path.Combine(root,"fixture.exe");failed=false;try{DiscordProxy.Validate(p);}catch(ArgumentException){failed=true;}if(!failed)throw new Exception("Non-Discord executable accepted");
  string xml=DiscordProxy.TaskXml("S-1-5-21-1",@"C:\Protected & safe\Litfix.exe");var doc=new XmlDocument();doc.LoadXml(xml);var ns=new XmlNamespaceManager(doc.NameTable);ns.AddNamespace("t","http://schemas.microsoft.com/windows/2004/02/mit/task");
  string[,] expected={{"//t:LogonTrigger/t:UserId","S-1-5-21-1"},{"//t:Principal/t:UserId","S-1-5-21-1"},{"//t:RunLevel","HighestAvailable"},{"//t:LogonType","InteractiveToken"},{"//t:RestartOnFailure/t:Interval","PT1M"},{"//t:RestartOnFailure/t:Count","3"},{"//t:ExecutionTimeLimit","PT0S"},{"//t:MultipleInstancesPolicy","IgnoreNew"},{"//t:Command",@"C:\Protected & safe\Litfix.exe"},{"//t:Arguments","--discord-background"}};
  for(int i=0;i<expected.GetLength(0);i++)if(doc.SelectSingleNode(expected[i,0],ns).InnerText!=expected[i,1])throw new Exception("Invalid startup task: "+expected[i,0]);
  File.WriteAllText(Path.Combine(root,"discord-task.xml"),xml,Encoding.Unicode);
  string version=Path.Combine(root,"Discord","app-1.0");Directory.CreateDirectory(version);File.WriteAllText(Path.Combine(version,"Discord.exe"),"fixture");string version2=Path.Combine(root,"Discord","app-2.0");Directory.CreateDirectory(version2);File.WriteAllText(Path.Combine(version2,"Discord.exe"),"fixture");if(DiscordProxy.InstalledDiscordPaths(Path.Combine(version,"Discord.exe")).Length!=2)throw new Exception("Discord update discovery failed");
  Console.WriteLine("PASS: separate encrypted Discord profile, corruption rejection, executable validation, startup XML and installed version discovery. No tasks registered or network policies changed.");
 }
}

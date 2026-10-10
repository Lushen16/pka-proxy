using System;using System.IO;using System.Collections.Generic;using System.Reflection;using System.Runtime.InteropServices;
public static class AppDiscovery
{
 public static string[] DiscordAt(string root)
 {
  Version best=null;string found=null;
  if(Directory.Exists(root))foreach(string dir in Directory.GetDirectories(root,"app-*")){
   Version version;string path=Path.Combine(dir,"Discord.exe");
   if(Version.TryParse(Path.GetFileName(dir).Substring(4),out version)&&File.Exists(path)&&(best==null||version>best)){best=version;found=path;}
  }
  return found==null?new string[0]:new[]{Path.GetFullPath(found)};
 }
 public static string[] PkaAt(string root)
 {
  var paths=new List<string>();foreach(string name in new[]{"PokeAlliance_Launcher.exe","PokeAlliance_dx.exe","PokeAlliance_gl.exe"}){
   string path=Path.Combine(root,name);if(File.Exists(path))paths.Add(Path.GetFullPath(path));
  }return paths.ToArray();
 }
 public static string[] Find(bool discord)
 {
  string local=Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
  if(discord)return DiscordAt(Path.Combine(local,"Discord"));
  var paths=new List<string>();
  foreach(string root in new[]{Path.Combine(local,"PokeAlliance Games","PokeAlliance"),Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"PokeAlliance Games","PokeAlliance"),Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),"PokeAlliance Games","PokeAlliance")})
   foreach(string path in PkaAt(root))if(!paths.Contains(path))paths.Add(path);
  // Read installed shortcuts; never execute a shortcut or register an uninstaller.
  foreach(var folder in new[]{Environment.SpecialFolder.DesktopDirectory,Environment.SpecialFolder.CommonDesktopDirectory,Environment.SpecialFolder.Programs,Environment.SpecialFolder.CommonPrograms}){
   string root=Environment.GetFolderPath(folder);if(!Directory.Exists(root))continue;
   string[] links;try{links=Directory.GetFiles(root,"*PokeAlliance*.lnk",SearchOption.AllDirectories);}catch{continue;}
   foreach(string link in links){object shell=null,shortcut=null;try{
    var type=Type.GetTypeFromProgID("WScript.Shell");shell=Activator.CreateInstance(type);
    shortcut=type.InvokeMember("CreateShortcut",BindingFlags.InvokeMethod,null,shell,new object[]{link});
    string target=(string)shortcut.GetType().InvokeMember("TargetPath",BindingFlags.GetProperty,null,shortcut,null);
    if(!String.IsNullOrEmpty(target)&&File.Exists(target))foreach(string path in PkaAt(Path.GetDirectoryName(target)))if(!paths.Contains(path))paths.Add(path);
   }catch{}finally{if(shortcut!=null)Marshal.FinalReleaseComObject(shortcut);if(shell!=null)Marshal.FinalReleaseComObject(shell);}}
  }return paths.ToArray();
 }
}

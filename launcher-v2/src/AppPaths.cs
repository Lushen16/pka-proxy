using System;using System.IO;
public static class AppPaths
{
 public static string ProtectedRoot;
 public static string Root {get{if(ProtectedRoot!=null)return ProtectedRoot;return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"PKAproxyV2");}}
 public static string SettingsFile(string name){return Path.Combine(Root,name);}
}

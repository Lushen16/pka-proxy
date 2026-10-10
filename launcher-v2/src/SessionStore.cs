using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
public sealed class SavedSession
{
    public string Game
    { get; set; }
    public string Host
    { get; set; }
    public int Port
    { get; set; }
    public string User
    { get; set; }
    public string ProtectedPassword
    { get; set; }
    public RoutingOptions Route
    { get; set; }
    public string Password()
    {
        try
        {
            return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(ProtectedPassword??""),null,DataProtectionScope.CurrentUser));
        }
        catch
        {
            return "";
        }
    }
}
public static class SessionStore
{
    static string ConfigPath
    {
        get
        {
            return AppPaths.SettingsFile("session-v2.bin");
        }
    }
    public static SavedSession Load()
    {
        try
        {
            return new JavaScriptSerializer().Deserialize<SavedSession>(Encoding.UTF8.GetString(ProtectedData.Unprotect(File.ReadAllBytes(ConfigPath),null,DataProtectionScope.CurrentUser)));
        }
        catch
        {
            return null;
        }
    }
    public static void Save(string game,string host,int port,string user,string password,RoutingOptions route)
    {
        route.ClientPath=game;
        var session=new SavedSession
        {
            Game=game,Host=host,Port=port,User=user,Route=route,ProtectedPassword=Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(password),null,DataProtectionScope.CurrentUser))
        };
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
        File.WriteAllBytes(ConfigPath,ProtectedData.Protect(Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(session)),null,DataProtectionScope.CurrentUser));
    }
}

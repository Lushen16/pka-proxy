using System;using System.IO;
public static class AppPaths
{
    public static string Root { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PKAproxy"); } }
    public static string SettingsFile(string name)
    {
        string path = Path.Combine(Root, name);
        if (!File.Exists(path))
        {
            string previous = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PkaProxy", name);
            if (File.Exists(previous)) { Directory.CreateDirectory(Root); File.Copy(previous, path, false); }
        }
        return path;
    }
}

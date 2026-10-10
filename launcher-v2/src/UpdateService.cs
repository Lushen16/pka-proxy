using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
public sealed class UpdateManifest
{
    public string version
    { get; set; }
    public string sha256
    { get; set; }
    public string file
    { get; set; }
    public long size
    { get; set; }
}
public sealed class PendingUpdate
{
    public string Repository,Folder;
    public UpdateManifest Manifest;
}
public sealed class PublishedAsset { public string name {get;set;} }
public sealed class PublishedRelease
{
    public string tag_name {get;set;}
    public bool draft {get;set;}
    public bool prerelease {get;set;}
    public PublishedAsset[] assets {get;set;}
}
public sealed class UpdatePreferences
{
    public string Repository
    { get; set; }
    public bool CheckOnStartup
    { get; set; }
    static string ConfigPath
    {
        get
        {
            return AppPaths.SettingsFile("updates.json");
        }
    }
    public static UpdatePreferences Load()
    {
        try
        {
            return new JavaScriptSerializer().Deserialize<UpdatePreferences>(File.ReadAllText(ConfigPath))??Defaults();
        }
        catch
        {
            return Defaults();
        }
    }
    static UpdatePreferences Defaults()
    {
        return new UpdatePreferences
        {
            Repository="Lushen16/LIT-fix",CheckOnStartup=true
        };
    }
    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
        File.WriteAllText(ConfigPath,new JavaScriptSerializer().Serialize(this),new UTF8Encoding(false));
    }
}
public static class UpdateService
{
    public static Version Current
    {
        get
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }
    }
    public static string Repository(string value)
    {
        value=value.Trim().TrimEnd('/');
        if(value.StartsWith("https://github.com/",StringComparison.OrdinalIgnoreCase))value=value.Substring(19);
        if(!Regex.IsMatch(value,@"\A[A-Za-z0-9][A-Za-z0-9-]{0,38}/[A-Za-z0-9_.-]{1,100}\z")||value.EndsWith("/..")||value.EndsWith("/."))throw new ArgumentException("Informe https://github.com/usuario/repositorio ou usuario/repositorio.");
        return value;
    }
    public static UpdateManifest VerifyManifest(byte[] bytes,byte[] signature,string publicKey)
    {
        if(bytes.Length>16384||signature.Length>1024)throw new InvalidOperationException("Manifesto inválido.");
        using(var rsa=new RSACryptoServiceProvider())
        {
            rsa.PersistKeyInCsp=false;
            rsa.FromXmlString(publicKey);
            if(!rsa.VerifyData(bytes,CryptoConfig.MapNameToOID("SHA256"),signature))throw new InvalidOperationException("Assinatura da atualização inválida.");
        }
        var m=new JavaScriptSerializer().Deserialize<UpdateManifest>(Encoding.UTF8.GetString(bytes));
        Version version;
        if(m==null||!Version.TryParse(m.version,out version)||m.file!="PKA-Proxy.exe"||m.size<1||m.size>50*1024*1024||!Regex.IsMatch(m.sha256??"",@"\A[0-9a-fA-F]{64}\z"))throw new InvalidOperationException("Manifesto incompatível.");
        return m;
    }
    public static void VerifyFile(string path,UpdateManifest m)
    {
        if(new FileInfo(path).Length!=m.size)throw new InvalidOperationException("Tamanho do download incorreto.");
        using(var sha=SHA256.Create())using(var f=File.OpenRead(path))
        {
            string hash=BitConverter.ToString(sha.ComputeHash(f)).Replace("-","");
            if(!hash.Equals(m.sha256,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Download alterado ou incompleto.");
        }
        var actual=System.Reflection.AssemblyName.GetAssemblyName(path).Version;
        if(actual!=new Version(m.version))throw new InvalidOperationException("Versão do executável não corresponde ao manifesto.");
    }
    static byte[] Download(string url,int limit)
    {
        var req=(HttpWebRequest)WebRequest.Create(url);
        req.UserAgent="PKA-Proxy-Updater/1.1";
        req.Timeout=15000;
        req.ReadWriteTimeout=15000;
        req.AllowAutoRedirect=true;
        using(var res=(HttpWebResponse)req.GetResponse())
        {
            if(res.ResponseUri.Scheme!="https"||res.StatusCode!=HttpStatusCode.OK||res.ContentLength>limit)throw new InvalidOperationException("Download recusado.");
            using(var stream=res.GetResponseStream())using(var output=new MemoryStream())
            {
                var buffer=new byte[8192];
                int n;
                while((n=stream.Read(buffer,0,buffer.Length))>0)
                {
                    if(output.Length+n>limit)throw new InvalidOperationException("Download excede o limite.");
                    output.Write(buffer,0,n);
                }
                return output.ToArray();
            }
        }
    }
    public static string SelectReleaseTag(byte[] listing,bool includePrereleases)
    {
        var releases=new JavaScriptSerializer().Deserialize<PublishedRelease[]>(Encoding.UTF8.GetString(listing));
        Version best=Current;string tag=null;
        foreach(var release in releases??new PublishedRelease[0])
        {
            if(release==null||release.draft||(!includePrereleases&&release.prerelease))continue;
            if(!Regex.IsMatch(release.tag_name??"",@"\Av[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+\z"))continue;
            Version version;if(!Version.TryParse(release.tag_name.Substring(1),out version)||version<=best)continue;
            bool manifest=false,signature=false,binary=false;
            foreach(var asset in release.assets??new PublishedAsset[0])if(asset!=null)
            {manifest|=asset.name=="update.json";signature|=asset.name=="update.sig";binary|=asset.name=="PKA-Proxy.exe";}
            if(!manifest||!signature||!binary)continue;
            best=version;tag=release.tag_name;
        }
        return tag;
    }
    public static PendingUpdate Check(string repository)
    {
        repository=Repository(repository);
        // These candidate builds follow both stable and published test releases.
        // Metadata only locates the tag; the pinned signing key remains the trust boundary.
        string tag=SelectReleaseTag(Download("https://api.github.com/repos/"+repository+"/releases?per_page=100",1024*1024),true);
        if(tag==null)return null;
        string root="https://github.com/"+repository+"/releases/download/"+tag+"/";
        byte[] data=Download(root+"update.json",16384),signature=Download(root+"update.sig",1024);
        var m=VerifyManifest(data,signature,UpdateTrust.PublicKey);
        if(tag!="v"+m.version)throw new InvalidOperationException("Versão assinada não corresponde à release.");
        if(new Version(m.version)<=Current)return null;
        string folder=Path.Combine(AppPaths.Root,"cache",Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        File.WriteAllBytes(Path.Combine(folder,"update.json"),data);
        File.WriteAllBytes(Path.Combine(folder,"update.sig"),signature);
        return new PendingUpdate
        {
            Repository=repository,Folder=folder,Manifest=m
        };
    }
    public static void Fetch(PendingUpdate update)
    {
        // Signed version chooses an immutable tag URL, avoiding a latest-release race.
        string url="https://github.com/"+Repository(update.Repository)+"/releases/download/v"+update.Manifest.version+"/PKA-Proxy.exe";
        string path=Path.Combine(update.Folder,"PKA-Proxy.exe");
        File.WriteAllBytes(path,Download(url,(int)update.Manifest.size));
        VerifyFile(path,update.Manifest);
    }
}

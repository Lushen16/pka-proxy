using System;using System.IO;using System.Security.Cryptography;using System.Text;using System.Diagnostics;using System.Web.Script.Serialization;
[assembly:System.Reflection.AssemblyVersion("2.0.1.0")]
public static class AppPaths {public static string Root{get{return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);}}public static string SettingsFile(string name){return Path.Combine(Root,name);}}
class UpdaterTests
{
 static PublishedRelease Release(string tag,bool draft,bool prerelease,bool complete){return new PublishedRelease{tag_name=tag,draft=draft,prerelease=prerelease,assets=complete?new[]{new PublishedAsset{name="update.json"},new PublishedAsset{name="update.sig"},new PublishedAsset{name="PKA-Proxy.exe"}}:new PublishedAsset[0]};}
 static string Choose(bool candidates,params PublishedRelease[] releases){return UpdateService.SelectReleaseTag(Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(releases)),candidates);}
 static int count;static void Assert(bool ok,string name){if(!ok)throw new Exception(name);count++;}static void Reject(System.Action action,string name){bool failed=false;try{action();}catch(InvalidOperationException){failed=true;}Assert(failed,name);}
 static int Main(string[] args)
 {
  if(args.Length>0&&args[0]=="--apply-update")return UpdateInstaller.Apply(args);
  if(args.Length==2&&args[0]=="--parent"){
   string folder=Path.GetFullPath(args[1]);var manifest=UpdateService.VerifyManifest(File.ReadAllBytes(Path.Combine(folder,"update.json")),File.ReadAllBytes(Path.Combine(folder,"update.sig")),UpdateTrust.PublicKey);
   UpdateInstaller.Start(new PendingUpdate{Repository="Lushen16/LIT-fix",Folder=folder,Manifest=manifest});return 0;
  }
  string stage=Path.GetFullPath(args[0]);byte[] data=File.ReadAllBytes(Path.Combine(stage,"update.json")),signature=File.ReadAllBytes(Path.Combine(stage,"update.sig"));
  var m=UpdateService.VerifyManifest(data,signature,UpdateTrust.PublicKey);Assert(m.version=="2.0.2.0","valid signature accepted");UpdateService.VerifyFile(Path.Combine(stage,"PKA-Proxy.exe"),m);count++;
  byte[] changed=(byte[])data.Clone();changed[10]^=1;Reject(()=>UpdateService.VerifyManifest(changed,signature,UpdateTrust.PublicKey),"manifest tampering rejected");changed=(byte[])signature.Clone();changed[0]^=1;Reject(()=>UpdateService.VerifyManifest(data,changed,UpdateTrust.PublicKey),"signature tampering rejected");
  string source=Path.Combine(stage,"PKA-Proxy.exe"),bad=Path.Combine(stage,"bad.exe");byte[] binary=File.ReadAllBytes(source);binary[binary.Length-1]^=1;File.WriteAllBytes(bad,binary);Reject(()=>UpdateService.VerifyFile(bad,m),"same-size corrupted download rejected");File.WriteAllText(bad,"truncated");Reject(()=>UpdateService.VerifyFile(bad,m),"truncated download rejected");
  string oldVersion=m.version;m.version="2.0.3.0";Reject(()=>UpdateService.VerifyFile(source,m),"binary version mismatch rejected");m.version=oldVersion;
  string original=Path.Combine(stage,"old.bin"),next=Path.Combine(stage,"new.bin"),profile=Path.Combine(stage,"profile.bin");File.WriteAllText(original,"OLD");File.WriteAllText(next,"NEW");File.WriteAllText(profile,"PROFILE");string backup=UpdateInstaller.ReplaceBinary(next,original);Assert(File.ReadAllText(original)=="NEW","atomic replacement");Assert(File.ReadAllText(backup)=="OLD","previous version backed up");Assert(File.ReadAllText(profile)=="PROFILE","profile preserved");UpdateInstaller.ReplaceBinary(backup,original);Assert(File.ReadAllText(original)=="OLD","rollback replacement works");
  Assert(UpdateService.Repository("https://github.com/Lushen16/LIT-fix/")=="Lushen16/LIT-fix","repository normalized");bool invalid=false;try{UpdateService.Repository("../bad");}catch(ArgumentException){invalid=true;}Assert(invalid,"repository traversal refused");
  Assert(Choose(true,Release("v2.0.2.0",false,true,true))=="v2.0.2.0","published prerelease discovered");
  Assert(Choose(false,Release("v2.0.2.0",false,true,true))==null,"stable channel excludes prerelease");
  Assert(Choose(true,Release("v9.0.0.0",true,true,true))==null,"draft ignored");
  Assert(Choose(true,Release("v9.0.0.0",false,true,false))==null,"incomplete release ignored");
  Assert(Choose(true,Release("v2.0.1.0",false,false,true))==null,"current version ignored");
  Assert(Choose(true,Release("v2.0.3.0",false,true,true),Release("v2.0.10.0",false,false,true),Release("v2.0.2.0",false,false,true))=="v2.0.10.0","numeric highest version independent of release order");
  Assert(Choose(true,Release("v../../evil",false,true,true))==null,"untrusted tag traversal ignored");
  Console.WriteLine("PASS: "+count+" updater assertions (release discovery, signature, hash, version, atomic replacement, rollback and profile preservation).");return 0;
 }
}

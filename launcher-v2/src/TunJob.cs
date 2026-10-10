using System;using System.ComponentModel;using System.Diagnostics;using System.Runtime.InteropServices;
public sealed class TunJob:IDisposable
{
    IntPtr handle;
    [StructLayout(LayoutKind.Sequential)]struct Limits {public long PerProcess,PerJob;public uint Flags;public UIntPtr Minimum,Maximum;public uint Active;public UIntPtr Affinity;public uint Priority,Scheduling;}
    [StructLayout(LayoutKind.Sequential)]struct Counters {public ulong Read,Write,Other,ReadBytes,WriteBytes,OtherBytes;}
    [StructLayout(LayoutKind.Sequential)]struct Extended {public Limits Basic;public Counters Io;public UIntPtr ProcessMemory,JobMemory,PeakProcess,PeakJob;}
    [DllImport("kernel32.dll",SetLastError=true)]static extern IntPtr CreateJobObject(IntPtr attributes,string name);
    [DllImport("kernel32.dll",SetLastError=true)]static extern bool SetInformationJobObject(IntPtr job,int type,IntPtr data,uint size);
    [DllImport("kernel32.dll",SetLastError=true)]static extern bool AssignProcessToJobObject(IntPtr job,IntPtr process);
    [DllImport("kernel32.dll")]static extern bool CloseHandle(IntPtr handle);
    public TunJob()
    {
        handle=CreateJobObject(IntPtr.Zero,null);if(handle==IntPtr.Zero)throw new Win32Exception();
        var limits=new Extended();limits.Basic.Flags=0x2000;int size=Marshal.SizeOf(limits);IntPtr data=Marshal.AllocHGlobal(size);
        try {Marshal.StructureToPtr(limits,data,false);if(!SetInformationJobObject(handle,9,data,(uint)size))throw new Win32Exception();}catch{Dispose();throw;}finally{Marshal.FreeHGlobal(data);}
    }
    public void Assign(Process p){if(!AssignProcessToJobObject(handle,p.Handle))throw new Win32Exception();}
    public void Dispose(){if(handle!=IntPtr.Zero){CloseHandle(handle);handle=IntPtr.Zero;}}
}

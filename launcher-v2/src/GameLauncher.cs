using System;
using System.IO;
using System.Diagnostics;
public static class GameLauncher
{
    public static void Open(string client, string launcher)
    {
        string selected=String.IsNullOrWhiteSpace(launcher)?client:launcher;
        if(String.IsNullOrWhiteSpace(selected))throw new InvalidOperationException("Selecione o executável em Escolher ou informe o launcher em Roteamento.");
        string path=Path.GetFullPath(selected.Trim());
        if(!Path.GetExtension(path).Equals(".exe",StringComparison.OrdinalIgnoreCase)||!File.Exists(path))
            throw new InvalidOperationException("Executável não encontrado neste computador. Selecione novamente o arquivo .exe do notebook; o caminho da VM pode ser diferente.");
        Process.Start(new ProcessStartInfo(path){UseShellExecute=true,WorkingDirectory=Path.GetDirectoryName(path)});
    }
}

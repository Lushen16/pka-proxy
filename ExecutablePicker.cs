using System;
using System.IO;
using System.Windows.Forms;
public static class ExecutablePicker
{
    public static string Select(IWin32Window owner)
    {
        using(var d=new OpenFileDialog
        {
            Title="Selecione o cliente final .exe (ou seu atalho)",Filter="Executáveis e atalhos|*.exe;*.lnk|Todos os arquivos|*.*",DereferenceLinks=true,InitialDirectory=Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
        })
        {
            if(d.ShowDialog(owner)!=DialogResult.OK)return null;
            if(Path.GetExtension(d.FileName).Equals(".exe",StringComparison.OrdinalIgnoreCase))return d.FileName;
            MessageBox.Show(owner,"Selecione um .exe. No atalho do launcher, abra Propriedades > Destino para localizar sua pasta. Com o jogo aberto, use Gerenciador de Tarefas > Detalhes > processo do cliente > Abrir local do arquivo. A regra do launcher não é herdada pelos filhos.","Localizar cliente final");
            return null;
        }
    }
}

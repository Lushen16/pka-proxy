using System;using System.Collections.Generic;using System.Windows.Forms;using System.Drawing;
public sealed class ProxyPicker:Panel
{
 public RadioButton Manual=new ReadableRadioButton(),Automatic=new ReadableRadioButton();public ComboBox Options=new ComboBox();public Button Import=new ReadableButton();public event Action Changed;public event Action ImportRequested;bool loading;
 public ProxyPicker(Color background,Color foreground,Font font)
 {
  SetBounds(20,48,775,36);BackColor=background;ForeColor=foreground;Font=font;
  Manual.Text="Proxy manual";Manual.SetBounds(0,2,150,30);Manual.Checked=true;Automatic.Text="Proxy automática";Automatic.SetBounds(150,2,165,30);
  Options.SetBounds(315,2,320,30);Options.FlatStyle=FlatStyle.Flat;Options.DropDownStyle=ComboBoxStyle.DropDownList;Options.DrawMode=DrawMode.OwnerDrawFixed;Options.DrawItem+=(s,e)=>{if(e.Index<0)return;using(var brush=new SolidBrush(background))e.Graphics.FillRectangle(brush,e.Bounds);TextRenderer.DrawText(e.Graphics,Options.Items[e.Index].ToString(),Font,e.Bounds,foreground,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);if((e.State&DrawItemState.Focus)!=0)e.DrawFocusRectangle();};Options.BackColor=background;Options.ForeColor=foreground;
  Import.Text="Importar lista";Import.SetBounds(645,0,130,34);Import.FlatStyle=FlatStyle.Flat;Import.FlatAppearance.BorderColor=Color.FromArgb(37,119,255);Import.BackColor=background;Import.ForeColor=foreground;
  Controls.AddRange(new Control[]{Manual,Automatic,Options,Import});
  Manual.CheckedChanged+=(s,e)=>{if(Manual.Checked)Notify();};Automatic.CheckedChanged+=(s,e)=>{if(Automatic.Checked)Notify();};Options.SelectedIndexChanged+=(s,e)=>Notify();Import.Click+=(s,e)=>{if(ImportRequested!=null)ImportRequested();};
 }
 void Notify(){Options.Enabled=Enabled&&Automatic.Checked&&Options.Items.Count>0;if(!loading&&Changed!=null)Changed();}
 public void Fill(List<ProxyPreset> presets)
 {loading=true;try{int previous=Options.SelectedIndex;Options.Items.Clear();for(int i=0;i<presets.Count;i++)Options.Items.Add("Opção "+(i+1)+(String.IsNullOrEmpty(presets[i].Location)?"":" — "+presets[i].Location));Automatic.Enabled=presets.Count>0;if(presets.Count>0)Options.SelectedIndex=Math.Max(0,Math.Min(previous,presets.Count-1));else Manual.Checked=true;}finally{loading=false;}Notify();}
 public void Restore(bool automatic,int index){loading=true;try{if(Options.Items.Count>0)Options.SelectedIndex=Math.Max(0,Math.Min(index,Options.Items.Count-1));if(automatic&&Options.Items.Count>0)Automatic.Checked=true;else Manual.Checked=true;}finally{loading=false;}Notify();}
 public void SetAvailable(bool available){Enabled=available;Options.Enabled=available&&Automatic.Checked&&Options.Items.Count>0;Automatic.Enabled=available&&Options.Items.Count>0;}
}

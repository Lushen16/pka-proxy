using System.Drawing;
using System.Windows.Forms;
public sealed class ReadableButton:Button
{
 protected override void OnPaint(PaintEventArgs e)
 {
  base.OnPaint(e);
  if(!Enabled){using(var brush=new SolidBrush(BackColor))e.Graphics.FillRectangle(brush,ClientRectangle);using(var pen=new Pen(FlatAppearance.BorderColor))e.Graphics.DrawRectangle(pen,0,0,Width-1,Height-1);TextRenderer.DrawText(e.Graphics,Text,Font,ClientRectangle,Color.FromArgb(145,181,230),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);}
 }
}

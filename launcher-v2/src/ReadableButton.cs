using System.Drawing;
using System.Windows.Forms;
public sealed class ReadableButton:Button
{
 public Color DisabledTextColor {get;set;}
 public ReadableButton(){DisabledTextColor=Color.FromArgb(145,181,230);}
 protected override void OnPaint(PaintEventArgs e)
 {
  base.OnPaint(e);
  if(!Enabled){using(var brush=new SolidBrush(BackColor))e.Graphics.FillRectangle(brush,ClientRectangle);using(var pen=new Pen(FlatAppearance.BorderColor))e.Graphics.DrawRectangle(pen,0,0,Width-1,Height-1);TextRenderer.DrawText(e.Graphics,Text,Font,ClientRectangle,DisabledTextColor,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);}
 }
}
public sealed class ReadableCheckBox:CheckBox
{
 public Color DisabledTextColor {get;set;}
 public ReadableCheckBox(){DisabledTextColor=Color.FromArgb(119,207,255);}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);if(!Enabled){var area=new Rectangle(18,0,Width-18,Height);using(var brush=new SolidBrush(BackColor))e.Graphics.FillRectangle(brush,area);TextRenderer.DrawText(e.Graphics,Text,Font,area,DisabledTextColor,TextFormatFlags.Left|TextFormatFlags.VerticalCenter);}}
}
public sealed class ReadableRadioButton:RadioButton
{
 public Color DisabledTextColor {get;set;}
 public ReadableRadioButton(){DisabledTextColor=Color.FromArgb(119,207,255);}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);if(!Enabled){var area=new Rectangle(18,0,Width-18,Height);using(var brush=new SolidBrush(BackColor))e.Graphics.FillRectangle(brush,area);TextRenderer.DrawText(e.Graphics,Text,Font,area,DisabledTextColor,TextFormatFlags.Left|TextFormatFlags.VerticalCenter);}}
}

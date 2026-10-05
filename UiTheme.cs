using System.Drawing;using System.Windows.Forms;
public static class UiTheme {
 public static Button PrimaryButton(string text,int x,int y,int width){
  var b=new Button{Text=text,Left=x,Top=y,Width=width,Height=42,FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(29,78,216),ForeColor=Color.White,Font=new Font("Segoe UI",11,FontStyle.Bold),Cursor=Cursors.Hand,Padding=new Padding(10,4,10,4),Image=FolderIcon(),TextImageRelation=TextImageRelation.ImageBeforeText};
  b.FlatAppearance.BorderColor=Color.FromArgb(147,197,253);b.FlatAppearance.BorderSize=1;b.FlatAppearance.MouseOverBackColor=Color.FromArgb(37,99,235);b.FlatAppearance.MouseDownBackColor=Color.FromArgb(30,64,175);
  return b;
 }
 static Bitmap FolderIcon(){var image=new Bitmap(22,20);using(var g=Graphics.FromImage(image)){using(var pen=new Pen(Color.White,2)){g.DrawRectangle(pen,2,6,18,12);g.DrawLines(pen,new[]{new Point(2,6),new Point(2,3),new Point(9,3),new Point(12,6)});}}return image;}
}

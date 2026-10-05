using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
public static class UiTheme
{
    public static Button PrimaryButton(string text, int x, int y, int width)
    {
        var button = new Button
        {
            Text=text, Left=x, Top=y, Width=width, Height=42,
            FlatStyle=FlatStyle.Flat, BackColor=Color.FromArgb(250,190,55),
            ForeColor=Color.FromArgb(20,22,28), Font=new Font("Segoe UI",11,FontStyle.Bold),
            Cursor=Cursors.Hand, Padding=new Padding(10,4,10,4),
            Image=FolderIcon(), TextImageRelation=TextImageRelation.ImageBeforeText
        };
        button.FlatAppearance.BorderColor=Color.FromArgb(255,219,127);
        button.FlatAppearance.BorderSize=1;
        button.FlatAppearance.MouseOverBackColor=Color.FromArgb(255,210,95);
        button.FlatAppearance.MouseDownBackColor=Color.FromArgb(221,156,28);
        return button;
    }
    static Bitmap FolderIcon()
    {
        var image=new Bitmap(22,20);
        using(var graphics=Graphics.FromImage(image))
        using(var pen=new Pen(Color.FromArgb(20,22,28),2))
        {
            graphics.DrawRectangle(pen,2,6,18,12);
            graphics.DrawLines(pen,new[]{new Point(2,6),new Point(2,3),new Point(9,3),new Point(12,6)});
        }
        return image;
    }
    public static Bitmap BrandMark(int size)
    {
        var bitmap=new Bitmap(size,size);
        using(var graphics=Graphics.FromImage(bitmap))
        {
            graphics.SmoothingMode=SmoothingMode.AntiAlias;
            graphics.ScaleTransform(size/64f,size/64f);
            using(var background=new SolidBrush(Color.FromArgb(250,190,55)))
                graphics.FillEllipse(background,2,2,60,60);
            using(var ink=new SolidBrush(Color.FromArgb(15,17,23)))
                graphics.FillPolygon(ink,new[]{new Point(34,10),new Point(17,35),new Point(29,35),new Point(25,54),new Point(47,27),new Point(34,27)});
        }
        return bitmap;
    }
}

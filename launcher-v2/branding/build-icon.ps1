Add-Type -AssemblyName System.Drawing
$root=Split-Path $PSScriptRoot
$stream=New-Object IO.MemoryStream
$writer=New-Object IO.BinaryWriter($stream)
$sizes=@(16,24,32,48,64,128,256)
$writer.Write([uint16]0);$writer.Write([uint16]1);$writer.Write([uint16]$sizes.Count)
$images=@();$offset=6+16*$sizes.Count
foreach($size in $sizes){
$b=New-Object Drawing.Bitmap($size,$size);$g=[Drawing.Graphics]::FromImage($b);$g.SmoothingMode='AntiAlias';$g.Clear([Drawing.Color]::FromArgb(7,12,20));$g.ScaleTransform($size/256.0,$size/256.0)
$brush=New-Object Drawing.SolidBrush([Drawing.Color]::FromArgb(37,119,255));$g.FillEllipse($brush,16,16,224,224);$brush.Dispose()
$brush=New-Object Drawing.SolidBrush([Drawing.Color]::FromArgb(7,12,20));$g.FillEllipse($brush,28,28,200,200);$brush.Dispose()
$brush=New-Object Drawing.SolidBrush([Drawing.Color]::FromArgb(119,207,255));$points=[Drawing.PointF[]]@([Drawing.PointF]::new(78,58),[Drawing.PointF]::new(108,58),[Drawing.PointF]::new(108,162),[Drawing.PointF]::new(185,162),[Drawing.PointF]::new(169,194),[Drawing.PointF]::new(78,194));$g.FillPolygon($brush,$points);$brush.Dispose()
$brush=New-Object Drawing.SolidBrush([Drawing.Color]::White);$g.FillPolygon($brush,[Drawing.PointF[]]@([Drawing.PointF]::new(149,64),[Drawing.PointF]::new(187,64),[Drawing.PointF]::new(156,121),[Drawing.PointF]::new(132,121)));$brush.Dispose()
$m=New-Object IO.MemoryStream;$b.Save($m,[Drawing.Imaging.ImageFormat]::Png);$bytes=$m.ToArray();$images+=,$bytes
if($size -eq 256){$b.Save((Join-Path $root 'branding\Litfix.png'),[Drawing.Imaging.ImageFormat]::Png)}
$writer.Write([byte]($size%256));$writer.Write([byte]($size%256));$writer.Write([byte]0);$writer.Write([byte]0);$writer.Write([uint16]1);$writer.Write([uint16]32);$writer.Write([uint32]$bytes.Length);$writer.Write([uint32]$offset);$offset+=$bytes.Length
$m.Dispose();$g.Dispose();$b.Dispose()
}
foreach($bytes in $images){$writer.Write([byte[]]$bytes)}
[IO.File]::WriteAllBytes((Join-Path $root 'PKAproxy.ico'),$stream.ToArray());$writer.Dispose();$stream.Dispose()

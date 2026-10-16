using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Icon = System.Drawing.Icon;

namespace PetPing.Windows;

/// <summary>
/// Render the one approved Companion Beacon WPF vector; never load network artwork.
/// Windows supports a PNG-framed ICO for the tray icon (Windows Vista and later).
/// </summary>
internal static class BrandIconFactory
{
    public static BitmapSource RenderBitmap(int size)
    {
        var mark = new BrandMark { Width = size, Height = size };
        mark.Measure(new Size(size, size));
        mark.Arrange(new Rect(0, 0, size, size));
        mark.UpdateLayout();

        var target = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        target.Render(mark);
        target.Freeze();
        return target;
    }

    public static Icon CreateTrayIcon()
    {
        const int size = 64;
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(RenderBitmap(size)));
        using var pngBuffer = new MemoryStream();
        encoder.Save(pngBuffer);
        var png = pngBuffer.ToArray();

        using var icoBuffer = new MemoryStream();
        using (var writer = new BinaryWriter(icoBuffer, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write((ushort)0);  // reserved
            writer.Write((ushort)1);  // ICO type
            writer.Write((ushort)1);  // image count
            writer.Write((byte)size);
            writer.Write((byte)size);
            writer.Write((byte)0);    // no palette
            writer.Write((byte)0);    // reserved
            writer.Write((ushort)1);  // color planes
            writer.Write((ushort)32); // bits per pixel
            writer.Write(png.Length);
            writer.Write(22);         // ICO header + one directory entry
            writer.Write(png);
        }
        icoBuffer.Position = 0;
        using var icon = new Icon(icoBuffer);
        return (Icon)icon.Clone();
    }
}

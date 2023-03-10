using System;
using System.Collections.Immutable;

namespace Magician.UI;
/* Lets you style RichParagraph using string interpolation. */
public class TextFormatSetting
{
    public const char Delimiter = '§';
    public const char Prefix = '¶';

    public enum FormatSetting
    {
        COLOR = 'c',
        SIZE = 's',
        BACK = 'b'
    }

    bool closingTag = false;
    Color? col;
    int? size;
    public TextFormatSetting(Color? col = null, int? size = null, bool closingTag = false)
    {
        this.col = col;
        this.size = size;
        this.closingTag = closingTag;
    }

    /* The thing */
    public override string ToString()
    {
        string tfs = Delimiter.ToString();

        // Down one level
        if (closingTag)
        {
            tfs += Prefix.ToString() + $"{(char)FormatSetting.BACK}";
            return tfs + Delimiter.ToString();
        }
        // Color
        if (col != null)
        {
            byte[] rgb = new byte[] { (byte)col.R, (byte)col.G, (byte)col.B, (byte)col.A };
            tfs += Prefix.ToString() + $"{(char)FormatSetting.COLOR}{BitConverter.ToString(rgb).Replace("-", "")}";
        }
        // Size
        if (size != null)
        {
            tfs += Prefix.ToString() + $"{(char)FormatSetting.SIZE}{size.ToString()}";
        }
        return tfs + Delimiter.ToString();
    }
}

/* Preset TextFormatSettings so you don't have to keep typing "TextFormatSetting" */
public static class TFS
{
    public static TextFormatSetting Red = new TextFormatSetting(new RGBA(0xff0000ff));
    public static TextFormatSetting Green = new TextFormatSetting(new RGBA(0x00ff00ff));
    public static TextFormatSetting Blue = new TextFormatSetting(new RGBA(0x0000ffff));
    public static TextFormatSetting RGB(int r, int g, int b) => new TextFormatSetting(new RGBA(r, g, b, 255));

    public static TextFormatSetting Small = new TextFormatSetting(null, 12);
    public static TextFormatSetting Size(int s) => new TextFormatSetting(null, s);

    public static TextFormatSetting Back = new TextFormatSetting(closingTag: true);
}
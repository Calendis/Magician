using System;
using System.Collections.Immutable;

namespace Magician.UI
{
    /* Lets you style RichParagraph using string interpolation. Currently supports Color */
    public class TextFormatSetting
    {
        public const char Delimiter = '§';
        public const char Prefix = '¶';
        public static Dictionary<char, int> Encoding = new Dictionary<char, int>
        {
            {'c', 8},  // color, as 8-char hex string
            {'s', 3},  // size, up to 999
            {'r', 0}   // reset format settings
        };

        public enum FormatSetting
        {
            COLOR = 'c',
            SIZE = 's',
            RESET = 'r'
        }

        //public string Prefix {get => prefix.ToString();}
        //public string Suffix {get => suffix.ToString();}

        Color? col;
        int? size;
        public TextFormatSetting(Color? col=null, int? size=null)
        {
            this.col = col;
            this.size = size;
        }

        /* The thing */
        public override string ToString()
        {
            string tfs = Delimiter.ToString();

            // Color
            if (col != null)
            {
                byte[] rgb = new byte[] { (byte)col.R, (byte)col.G, (byte)col.B, (byte)col.A };
                tfs += Prefix.ToString() + $"c{BitConverter.ToString(rgb).Replace("-", "")}";
            }
            if (size != null)
            {
                tfs += Prefix.ToString();
            }

            return tfs + Delimiter.ToString();
        }
    }

    /* Preset TextFormatSettings so you don't have to keep typing "TextFormatSetting" */
    public static class TFS
    {
        public static TextFormatSetting Red = new TextFormatSetting(new RGBA(0xff0000ff));
        public static TextFormatSetting Blue = new TextFormatSetting(new RGBA(0x0000ffff));
    }
}

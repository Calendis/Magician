using Magician.Renderer;
using System.Linq;

namespace Magician.UI
{
    public class Paragraph : Multi
    {
        protected string[]? sentences;
        Paragraph(double x = 0, double y = 0, string fontPath = "", Color? c = null, int? size = null, params string[] ss) : base(x, y, c ?? Data.Col.UIDefault.FG, DrawMode.INVISIBLE)
        {
            sentences = new string[ss.Length];
            // If no path given, use the default
            fontPath = fontPath == "" ? Text.FallbackFontPath : fontPath;
            for (int i = 0; i < ss.Length; i++)
            {
                sentences[i] = ss[i];
                size = size ?? Data.Globals.fontSize;
                Text t = new Text(sentences[i], col, (int)size, fontPath);
                this[$"line{i}"] = new Multi(0, -i * (int)size)
                .Textured(t.Render())
                .DrawFlags(DrawMode.INVISIBLE)
                ;
                t.Dispose();
            }
        }
        public Paragraph(double x, double y, string s, Color c, int size, char delimiter = '\n', string fp = "") : this(x, y, fp, c, size, s.Split(delimiter)) { }
        public Paragraph(double x, double y, Color? c = null, int? size = null, params string[] ss) : this(x, y, "", c, size, ss) { }
        // Does not set text, only position and colour. Used by RichParagraph
        protected Paragraph(double x, double y, Color c) : base(x, y, c, DrawMode.INVISIBLE) { }

        /* TODO: finish these */
        public Multi LeftAligned()
        {
            throw new NotImplementedException("Doesn't work yet");
        }
        public Multi RightAligned()
        {
            throw new NotImplementedException("Doesn't work yet");
        }
        public Multi Justified()
        {
            // Determine maximum width of our lines
            throw new NotImplementedException("Doesn't work yet");
        }
    }

    public enum Justification
    {
        LEFT,
        CENTRE,
        RIGHT
    }

    /* Paragraph with in-line formatting */
    public class RichParagraph : Paragraph
    {
        int size;
        string[] inStr;
        Justification just;
        // Rich paragraph supports inline text formatting using TextFormatSetting and $"string interpolation"
        RichParagraph(double x = 0, double y = 0, string fontPath = "", Color? c = null, int? sz = null, Justification just = 0, params string[] inputStr) : base(x, y, c ?? Data.Col.UIDefault.FG)
        {
            // If no size given, use the default
            size = sz ?? Data.Globals.fontSize;
            inStr = inputStr;
            this.just = just;

            // If no path given, use the default
            fontPath = fontPath == "" ? Text.FallbackFontPath : fontPath;

            // Group the input into text and formatters
            List<string[]> groupedFormats = new List<string[]>();
            for (int i = 0; i < inputStr.Length; i++)
            {
                // Split each line along the format delimiter
                string[] formats = inputStr[i].Split(TextFormatSetting.Delimiter);

                // Filter empties
                formats = formats.Where(s => s != "").ToArray();

                // Flag the formatters as 1 and the text as 0
                bool[] flags = formats.Select(f => f.Substring(0, 1) == TextFormatSetting.Prefix.ToString()).ToArray();

                // Remove extraneous format settings
                bool lastFlag = false;
                int run = 0;
                List<string> filteredFormats = new List<string>();
                for (int j = 0; j < formats.Length; j++)
                {
                    if (lastFlag && flags[j] == lastFlag)
                    {
                        filteredFormats[j - ++run] = formats[j];
                        continue;
                    }
                    run = 0;
                    filteredFormats.Add(formats[j]);
                    lastFlag = flags[j];
                }
                groupedFormats.Add(filteredFormats.ToArray());
            }

            /* Now that filtering is done, assemble the Rich Paragraph */
            // These two stacks are to keep strack of nested in-line format settings
            Stack<Color> colStack = new Stack<Color>(); colStack.Push(col);
            Stack<int> sizStack = new Stack<int>(); sizStack.Push(size);
            int maxSize = sizStack.Peek();
            /* The bool[] represents wasColorChanged, wasSizeChanged */
            Stack<bool[]> deltas = new Stack<bool[]>();
            Multi phrases = new Multi().DrawFlags(DrawMode.INVISIBLE);
            // Used for justification
            int maxLineWidth = 0;
            for (int row = 0; row < groupedFormats.Count; row++)
            {
                Multi wordsInLine = new Multi().DrawFlags(DrawMode.INVISIBLE);
                int runningLength = 0;
                for (int column = 0; column < groupedFormats[row].Length; column++)
                {
                    char ch = groupedFormats[row][column][0];
                    /* The item is a formatter */
                    if (ch == TextFormatSetting.Prefix)
                    {
                        deltas.Push(new bool[] { false, false });
                        string formatSettings = groupedFormats[row][column].Substring(1);
                        // Extract the formatters
                        foreach (string formatSetting in formatSettings.Split(TextFormatSetting.Prefix))
                        {
                            // What kind of formatter is it?
                            char formatSettingIdentifer = formatSetting[0];
                            switch (formatSettingIdentifer)
                            {
                                case ((char)TextFormatSetting.FormatSetting.COLOR):
                                    byte[] rgba = Convert.FromHexString(formatSetting.Substring(1));
                                    colStack.Push(new RGBA(rgba[0], rgba[1], rgba[2], rgba[3]));
                                    deltas.Peek()[0] = true;
                                    break;
                                case ((char)TextFormatSetting.FormatSetting.SIZE):
                                    int s;
                                    int.TryParse(formatSetting.Substring(1), out s);
                                    sizStack.Push(s);
                                    maxSize = s > maxSize ? s : maxSize;
                                    deltas.Peek()[1] = true;
                                    break;
                                case ((char)TextFormatSetting.FormatSetting.BACK):
                                    if (deltas.Count < 1)
                                    {
                                        Scribe.Warn("Text format setting in RichParagraph fell back on nothing");
                                        break;
                                    }

                                    deltas.Pop();
                                    if (deltas.Peek()[0])
                                    {
                                        colStack.Pop();
                                        if (colStack.Count < 1)
                                        {
                                            colStack.Push(col);
                                        }
                                    }
                                    if (deltas.Peek()[1])
                                    {
                                        sizStack.Pop();
                                        if (sizStack.Count < 1)
                                        {
                                            sizStack.Push(size);
                                        }
                                    }
                                    break;
                                default:
                                    Scribe.Error($"Unsupported format setting identifer {formatSettingIdentifer}");
                                    break;
                            }
                        }
                    }
                    /* The item is a phrase */
                    else
                    {
                        // Add words
                        Text t = new Text(groupedFormats[row][column].ToString(), colStack.Peek(), sizStack.Peek());
                        Texture txr = t.Render();

                        // Determine position
                        double lineX = runningLength;
                        double lineY = -row * maxSize - (maxSize - sizStack.Peek());

                        phrases.Add(new Multi(lineX, lineY).Textured(txr));
                        runningLength += txr.Width;
                        maxLineWidth = runningLength > maxLineWidth ? runningLength : maxLineWidth;
                        t.Dispose();
                    }
                }
            }
            // Become the assembled Multi
            Become(phrases);

            // Calculate shifts
            int trail = 0;
            for (int i = 0; i < Count; i++)
            {
                Multi m = this[Count - i - 1];
                double dRightMargin = maxLineWidth - m.x.Evaluate() - m.Texture.Width;
                m.Written(dRightMargin - trail);
                trail += m.Texture.Width;
                if (m.x.Evaluate() == 0)
                {
                    trail = 0;
                }
            }

            // Apply justification rules
            switch (this.just)
            {
                // Default case, do nothing
                case (Justification.LEFT):
                    break;

                case (Justification.CENTRE):
                    Sub(m => m.Translated(m.Evaluate() / 2, 0));
                    break;

                case (Justification.RIGHT):
                    Sub(m => m.Translated(m.Evaluate(), 0));
                    break;
            }
        }
        public RichParagraph(double x, double y, string s, Color c, int size, Justification j = 0, char delimiter = '\n', string fp = "") : this(x, y, fp, c, size, j, s.Split(delimiter)) { }
        public RichParagraph(double x, double y, Color? c = null, int? size = null, Justification j = 0, params string[] ss) : this(x, y, "", c, size, j, ss) { }
    }
}
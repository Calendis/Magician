namespace Magician
{
    public class Color
    {
        // 32-bit int representing the colour in RGBA
        private uint col;
        
        // Lol, I store a uint for the colour value and three floats for precision!
        // Oh well, the bitwise stuff was fun to write
        private float[] floatingParts = new float[3];
        // TODO: actually implement Saturation and Lightness getters
        bool isHSL = false;
        public Color(byte r, byte g, byte b, byte a, float f0 = 0, float f1 = 0, float f2 = 0)
        {
            isHSL = false;
            (floatingParts[0], floatingParts[1], floatingParts[2]) = (f0, f1, f2);
            //int[] integerParts = handleFloatOverflow();
            //col = (uint)(r + integerParts[0] << 24) + (uint)(g + integerParts[1] << 16) + (uint)(b + integerParts[2]<< 8) + (uint)(a);
            col = (uint)(r << 24) + (uint)(g << 16) + (uint)(b << 8) + (uint)(a);
        }
        public Color(byte r, byte g, byte b, byte a, float[] fps) : this(r, g, b, a, fps[0], fps[1], fps[2]) {}

        // Yeah this is awful, but it makes the constructors not ambiguous
        // I could make a base class for RGBColor and HSLColor, but I don't want to
        public Color(float h, float s, float l, byte a, float f0 = 0, float f1 = 0, float f2 = 0, bool disambiguationBool = true)
        {
            isHSL = true;
            (floatingParts[0], floatingParts[1], floatingParts[2]) = (f0, f1, f2);
            int[] integerParts = handleFloatOverflow();
            col = HSLToRGBHex((h + integerParts[0]) % 360, s + integerParts[1], l + integerParts[2], a);
        }

        // Create an RGBA colour directly from hex value
        public Color(uint hex)
        {
            isHSL = false;
            col = hex;
        }

        private int[] handleFloatOverflow()
        {
            int[] integerParts = new int[3];
            for(int i = 0; i < floatingParts.Length; i++)
            {
                if (floatingParts[i] >= 1)
                {
                    integerParts[i] = (int)floatingParts[i];
                    floatingParts[i] -= integerParts[i];
                }
                else
                {
                    integerParts[i] = 0;
                }
            }

            return integerParts;
        }

        public uint HexCol
        {
            get => col;
            set => col = value;
        }
        public byte R
        {
            get => (byte)((col & 0xff000000) >> 24);
            set => col = (uint)((byte)value << 24) + (col & 0x00ffffff);

        } 
        public byte G
        {
            get => (byte)((col & 0x00ff0000) >> 16);
            set => col = (uint)((byte)value << 16) + (col & 0xff00ffff);
        } 
        public byte B
        {
            get => (byte)((col & 0x0000ff00) >> 8);
            set => col = (uint)((byte)value << 8) + (col & 0xffff00ff);
        } 
        public byte A
        {
            get => (byte)((col & 0x000000ff) >> 0);
            set => col = (uint)((byte)value) + (col & 0xffffff00);
        }
        public float Hue => HueFromRGB(R, G, B);
        public float Saturation => 1;
        public float Lightness => 1;

        public float FloatingPart0
        {
            get => floatingParts[0];
            set => floatingParts[0] = value;
        }
        public float FloatingPart1
        {
            get => floatingParts[1];
            set => floatingParts[1] = value;
        }
        public float FloatingPart2
        {
            get => floatingParts[2];
            set => floatingParts[2] = value;
        }

        public bool IsHSL
        {
            get => isHSL;
        }

        public Color Copy()
        {
            return new Color(R, G, B, A);
        }

        public Color ToHSL()
        {
            return new Color(Hue, Saturation, Lightness, A, disambiguationBool: true);
        }

        /*
            Begin static methods
        */

        public static uint HSLToRGBHex(float h, float s, float l, byte a)
        {
            float r, g, b;
            float c = l * s;
            float x = c * (1-Math.Abs((h/60f) % 2 - 1));
            float m = l - c;

            if (h < 60)
            {
                r = c;
                g = x;
                b = 0;
            }
            else if (h < 120)
            {
                r = x;
                g = c;
                b = 0;
            }
            else if (h < 180)
            {
                r = 0;
                g = c;
                b = x;
            }
            else if (h < 240)
            {
                r = 0;
                g = x;
                b = c;
            }
            else if (h < 300)
            {
                r = x;
                g = 0;
                b = c;
            }
            else if (h < 360)
            {
                r = c;
                g = 0;
                b = x;
            }
            else
            {
                throw new InvalidDataException($"Invalid phase {h}");
            }
            r = 255*(r+m);
            g = 255*(g+m);
            b = 255*(b+m);

            return (uint)((byte)r << 24) + (uint)((byte)g << 16) + (uint)((byte)b << 8) + (uint)(a);
        }
        private static float HueFromRGB(byte r, byte g, byte b)
        {
            float h;
            float rf = (float)r / 255f;
            float gf = (float)g / 255f;
            float bf = (float)b / 255f;
            float colMax = Math.Max(rf, Math.Max(gf, bf)); // TODO: get rid of this recursion
            float colMin = Math.Min(rf, Math.Min(gf, bf));

            if (colMax == rf)
            {
                h = (gf - bf) / (colMax - colMin);
            }
            else if (colMax == gf)
            {
                h = 2f + (bf - rf) / (colMax - colMin);
            }
            else if (colMax == bf)
            {
                h = 4f + (rf - gf) / (colMax - colMin);
            }
            else
            {
                throw new InvalidDataException($"Could not get hue from rgb: {r} {g} {b}");
            }
            h *= 60;
            if (h < 0)
            {
                h += 360;
            }
            return h;
        }

        public static Color RED
        {
            get => new Color(0xff0000ff);
        }

        public static Color GREEN
        {
            get => new Color(0x00ff00ff);
        }

        public static Color BLUE
        {
            get => new Color(0x0000ffff);
        }

        public static Color YELLOW
        {
            get => new Color(0xffff00ff);
        }

        public static Color CYAN
        {
            get => new Color(0x00ffffff);
        }

        public override string ToString()
        {
            return $"RGBA: {R}, {G}, {B}, {A}";
        }
    }
}
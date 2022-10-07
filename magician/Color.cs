/*
*  Class for storing and manipulating colour data, with RGBA and HSLA support
*/

namespace Magician
{
    public class Color
    {
        // 32-bit int representing the colour in RGBA
        private uint col;
        
        // Lol, I store a uint for the colour value and three additional floats for precision
        // It makes the bitwise integer math pretty pointless but it was fun to write
        private float[] floatingParts = new float[3];
        
        // Whether or not the colour is treated as HSL
        bool isHSL = false;
        
        // Create a colour from RGBA, and floating parts for more precise RGB
        public Color(byte r, byte g, byte b, byte a, float f0 = 0, float f1 = 0, float f2 = 0)
        {
            isHSL = false;
            (floatingParts[0], floatingParts[1], floatingParts[2]) = (f0, f1, f2);
            int[] integerParts = handleFloatOverflow();
            col = (uint)(r + integerParts[0] << 24) + (uint)(g + integerParts[1] << 16) + (uint)(b + integerParts[2]<< 8) + (uint)(a);
        }
        public Color(byte r, byte g, byte b, byte a, float[] fps) : this(r, g, b, a, fps[0], fps[1], fps[2]) {}

        // Create a colour from HSLA, and floating parts for more precise HSL
        // This gross constructor has a bool parameter so it's not ambiguous with the other constructor
        // The disambiguation bool's value is not used
        // I could make a base class for RGBColor and HSLColor, but I don't want to
        public Color(float h, float s, float l, byte a, float f0 = 0, float f1 = 0, float f2 = 0, bool disambiguationBool = true)
        {
            isHSL = true;
            (floatingParts[0], floatingParts[1], floatingParts[2]) = (f0, f1, f2);
            int[] integerParts = handleFloatOverflow();
            col = HSLToRGBHex((h + integerParts[0]), s + integerParts[1], l + integerParts[2], a);
        }

        // Create an RGBA colour directly from hex value
        public Color(uint hex)
        {
            isHSL = false;
            col = hex;
        }

        // If the floating parts of the colour are more than 1, return integer parts and
        // subtract from the floating part so that it is between 0 and 1 again
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

        // A bunch of properties
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
        // TODO: actually implement Saturation and Lightness getters
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

        // Given HSLA, return RGBA as a 32-bit uint
        public static uint HSLToRGBHex(float h, float s, float l, byte a)
        {
            float r, g, b;
            float c = l * s;
            float x = c * (1-Math.Abs((h/60f) % 2 - 1));
            float m = l - c;

            h %= 360;

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
        
        // Calculate a colour's hue angle from RGB values
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

        // Some pre-defined colours
        public static Color Red
        {
            get => new Color(0xff0000ff);
        }

        public static Color Green
        {
            get => new Color(0x00ff00ff);
        }

        public static Color Blue
        {
            get => new Color(0x0000ffff);
        }

        public static Color Yellow
        {
            get => new Color(0xffff00ff);
        }

        public static Color Cyan
        {
            get => new Color(0x00ffffff);
        }

        public override string ToString()
        {
            return $"RGBA: {R}, {G}, {B}, {A}";
        }
    }
}
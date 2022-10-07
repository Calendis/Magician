namespace Magician
{
    public class Color
    {
        // 32-bit int representing
        private uint col;
        public uint HexCol
        {
            get => col;
            set
            {
                col = value;
            }
        }
        public byte R
        {
            get => (byte)((col & 0xff000000) >> 24);
            set
            {
                col = (uint)((byte)value << 24) + (col & 0x00ffffff);
            }

        } 
        public byte G
        {
            get => (byte)((col & 0x00ff0000) >> 16);
            set
            {
                col = (uint)((byte)value << 16) + (col & 0xff00ffff);
            }
        } 
        public byte B
        {
            get => (byte)((col & 0x0000ff00) >> 8);
            set
            {
                col = (uint)((byte)value << 8) + (col & 0xffff00ff);
            }
        } 
        public byte A
        {
            get => (byte)((col & 0x000000ff) >> 0);
            set
            {
                col = (uint)((byte)value) + (col & 0xffffff00);
            }
        }
        public float Hue => HueFromRGB(R, G, B);
        bool hsv = false;
        public Color(byte r, byte g, byte b, byte a)
        {
            hsv = false;
            col = (uint)(r << 24) + (uint)(g << 16) + (uint)(b << 8);// + (uint)(a);
        }

        // Yeah this is awful, but it makes the constructors not ambiguous
        public Color(float h, float s, float v, byte a, bool _ = true)
        {
            hsv = true;
            col = HSVToRGB(h, s, v, a);
        }

        public Color(uint hex)
        {
            hsv = false;
            col = hex;
        }

        public bool HSV
        {
            get => hsv;
        }

        public Color Copy()
        {
            return new Color(R, G, B, A);
        }

        public Color ToHSV()
        {
            return new Color(R, G, B, A, true);
        }

        /*
            Begin static methods
        */

        public static uint HSVToRGB(float h, float s, float l, byte a)
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
            // Phase is from 0 to 60
            if (r == 255 && b == 0)
            {
                return 60 * (float)g/255f;
            }
            // Phase is from 60 to 120
            else if (g == 255 && b == 0)
            {
                return 60 + 60 * (255 - (float)r/255f);
            }
            // Phase is from 120 to 180
            else if (g == 255 && r == 0)
            {
                return 120 + 60 * (float)b/255f;
            }
            // Phase is from 180 to 240
            else if (b == 255 && r == 0)
            {
                return 180 + 60 * (255 - (float)g/255f);
            }
            // Phase is from 240 to 300
            else if (b == 255 && g == 0)
            {
                return 240 + 60 * (float)r/255f;
            }
            // Phase is from 300 to 360
            else if (r == 255 && g == 0)
            {
                return 300 + 60*(255 - (float)b/255f);
            }
            // Greyscale hue
            else if (r + g + b == 0)
            {
                Console.WriteLine("Warning: attempted to get hue from greyscale");
                return 0;
            }
            else
            {
                throw new InvalidDataException("Could not get Hue from RGB!");
            }
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
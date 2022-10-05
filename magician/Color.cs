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

        public static uint HSVToRGB(float h, float s, float v, byte a)
        {
            byte r = 0;
            byte g = 0;
            byte b = 0;
            float phase = h % 360;
            float invSat = 1 - s;
            
            if (phase < 60)
            {
                r = 255;
                g = HSVSlope(phase);
                b = 0;
            }
            else if (phase < 120)
            {
                r = (byte)(255 - HSVSlope(phase-60));
                g = 255;
                b = 0;
            }
            else if (phase < 180)
            {
                r = 0;
                g = 255;
                b = HSVSlope(phase - 120);
            }
            else if (phase < 240)
            {
                r = 0;
                g = (byte)(255 - HSVSlope(phase - 180));
                b = 255;
            }
            else if (phase < 300)
            {
                r = HSVSlope(phase - 240);
                g = 0;
                b = 255;
            }
            else if (phase < 360)
            {
                r = 255;
                g = 0;
                b = (byte)(255 - HSVSlope(phase - 300));
            }
            else
            {
                throw new InvalidDataException($"Invalid phase {phase}");
            }
            return (uint)(r << 24) + (uint)(g << 16) + (uint)(b << 8) + (uint)(a);
        }

        private static byte HSVSlope(float x)
        {
            return (byte)(x / 60f * 255);
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

        public override string ToString()
        {
            return $"RGBA: {R}, {G}, {B}, {A}";
        }
    }
}
namespace Magician
{
    public class Color
    {
        // 32-bit int representing
        private uint col;
        public uint HexCol => col;
        public byte R => (byte)((col & 0xff000000) >> 24);
        public byte G => (byte)((col & 0x00ff0000) >> 16);
        public byte B => (byte)((col & 0x0000ff00) >> 8);
        public byte A => (byte)((col & 0x000000ff) >> 0);
        public Color(byte r, byte g, byte b, int a, bool hsv=false)
        {
            // Convert to HSV before setting
            if (hsv)
            {
                int h;
                int s;
                int v;
            }

            col = (uint)(r << 24) + (uint)(g << 16) + (uint)(b << 8);
        }

        public Color(uint hex, bool hsv=false)
        {
            if (hsv)
            {
                // do hsv conversion
            }
            col = hex;
        }

        public Color Copy()
        {
            return new Color(R, G, B, A);
        }

        public static int RGBToHSV(byte r, byte g, byte b)
        {
            throw new NotImplementedException();
        }

        public static int RGBToHSV(int hex)
        {
            byte r = (byte)(hex & 0xff000000);
            byte g = (byte)(hex & 0x00ff0000);
            byte b = (byte)(hex & 0x0000ff00);
            return RGBToHSV(r, g, b);
        }

        public static int HSVToRGB(byte h, byte s, byte v)
        {
            throw new NotImplementedException();
        }

        public static int HSVToRGB(int hex)
        {
            byte h = (byte)(hex & 0xff000000);
            byte s = (byte)(hex & 0x00ff0000);
            byte v = (byte)(hex & 0x000000ff);
            return HSVToRGB(h, s, v);
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
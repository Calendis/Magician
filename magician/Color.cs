namespace Magician
{
    class Color
    {
        // 32-bit int representing
        private int col;
        public char R => (char)(col & 0xff000000);
        public Color(char r, char g, char b, int a, bool hsv=false)
        {
            // Convert to HSV before setting
            if (hsv)
            {
                int h;
                int s;
                int v;
            }

            char byte1 = (char)(r & 0xff000000);
            char byte2 = (char)((g & 0xff000000) >> 8);
            char byte3 = (char)((r & 0xff000000) >> 16);

            col = byte1 + byte2 + byte3;
        }

        public Color(int hex, bool hsv=false)
        {
            if (hsv)
            {
                // do hsv conversion
            }
        }

        public static int RGBToHSV(char r, char g, char b)
        {
            throw new NotImplementedException();
        }

        public static int RGBToHSV(int hex)
        {
            char r = (char)(hex & 0xff000000);
            char g = (char)(hex & 0x00ff0000);
            char b = (char)(hex & 0x0000ff00);
            return RGBToHSV(r, g, b);
        }

        public static int HSVToRGB(char h, char s, char v)
        {
            throw new NotImplementedException();
        }

        public static int HSVToRGB(int hex)
        {
            char h = (char)(hex & 0xff000000);
            char s = (char)(hex & 0x00ff0000);
            char v = (char)(hex & 0x000000ff);
            return HSVToRGB(h, s, v);
        }

    }
}
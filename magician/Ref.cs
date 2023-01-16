/*
*  Class for storing global variables like window and UI values
*/

namespace Magician
{
    public static class Ref
    {
        public static int winWidth = 1200;
        public static int winHeight = 800;
        public static int fontSize = 24;

        public static HSLA bgCol = new RGBA(0x001010ff).ToHSLA();
        public static HSLA fgCol = new RGBA(0x00e9f5ff).ToHSLA();
    }
}
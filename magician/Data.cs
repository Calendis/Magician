/*
*  Class for storing global variables like window and UI values
*/

namespace Magician.Data
{
    public static class Globals
    {
        public static int winWidth = 1200;
        public static int winHeight = 800;
        public static int fontSize = 24;

    }

    public static class Env
    {
        public static double Time { get; set; }
        public static Random RNG = new Random();
        public static double RandX => RNG.NextDouble() * Globals.winWidth - Globals.winWidth / 2;
        public static double RandY => RNG.NextDouble() * Globals.winHeight - Globals.winHeight / 2;
    }

    public static class Color
    {
        public static Palette UITurquoise = new Palette(
            new RGBA(0x000d0dff),
            new RGBA(0x002626ff),
            new RGBA(0x005151ff),
            new RGBA(0x007676ff),
            new RGBA(0xffffffff)
        );

        public static Palette UIBlue = new Palette(
            new RGBA(0x000000ff),
            new RGBA(0x000c27ff),
            new RGBA(0x00174bff),
            new RGBA(0x002986ff),
            new RGBA(0xffffffff)
        );

        public static Palette UIRed = new Palette(
            new RGBA(0x300e17ff),
            new RGBA(0x5e0018ff),
            new RGBA(0x861431ff),
            new RGBA(0xaa2b4bff),
            new RGBA(0xffffffff)
        );

        public static Palette UIGreen = new Palette(
            new RGBA(0x000c00ff),
            new RGBA(0x004d00ff),
            new RGBA(0x007100ff),
            new RGBA(0x00bf00ff),
            new RGBA(0xffffffff)
        );

        public static Palette UIDefault = UITurquoise;
    }
}
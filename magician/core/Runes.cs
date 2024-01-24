/*
*  Class for storing global variables like window and UI values
*/
namespace Magician.Runes;

public static class App
{
    const string name = "Magician";
    const string version = "Alpha 0.1";
    public static string Title
    {
        get => name + " " + version;
    }
}

public static class Globals
{
    public static double winWidth = 1200;
    public static double winHeight = 800;
    public static int fontSize = 24;
    public const double defaultTol = 1.4210854715202004E-14;

}

public static class Numbers
{
    const int howManyNums = 10;
    const int circleDivisions = 8;
    public readonly static Core.IVal e = new Core.Val(Math.E);
    public readonly static Core.IVal Pi = new Core.Val(Math.PI);
    public readonly static Core.IVal i = new Core.Val(0, 1);
    public readonly static Core.IVal iPi = new Core.Val(0, Math.PI);
    readonly static Core.IVal[] nums = new Core.Val[howManyNums];
    readonly static Core.IVal[] unitCircle = new Core.Val[circleDivisions];
    static Numbers()
    {
        for (int i = 0; i < howManyNums; i++)
        {
            nums[i] = new Core.Val(i);
        }
        for (int i = 0; i < circleDivisions; i++)
        {
            unitCircle[i] = new Core.Val(Alg.Numeric.Trig.Cos(2 * Math.PI / i), Alg.Numeric.Trig.Sin(2 * Math.PI / i));
        }
    }
    public static Core.IVal Get(int i)
    {
        return nums[i];
    }
    public static Core.IVal Circle8ths(int i)
    {
        return unitCircle[i];
    }
}

// TODO move these

public static class Col
{
    public readonly static Palette UITurquoise = new(
        new RGBA(0x000d0dff),
        new RGBA(0x002626ff),
        new RGBA(0x005151ff),
        new RGBA(0x007676ff),
        new RGBA(0xffffffff)
    );

    public readonly static Palette UIBlue = new(
        new RGBA(0x000000ff),
        new RGBA(0x000c27ff),
        new RGBA(0x00174bff),
        new RGBA(0x002986ff),
        new RGBA(0xffffffff)
    );

    public readonly static Palette UIRed = new(
        new RGBA(0x300e17ff),
        new RGBA(0x5e0018ff),
        new RGBA(0x861431ff),
        new RGBA(0xaa2b4bff),
        new RGBA(0xffffffff)
    );

    public readonly static Palette UIGreen = new(
        new RGBA(0x000c00ff),
        new RGBA(0x004d00ff),
        new RGBA(0x007100ff),
        new RGBA(0x00bf00ff),
        new RGBA(0xffffffff)
    );
    public static Palette UIDefault = UITurquoise;

    public static Palette PurpleJan21 = new(
        new RGBA(0xDC78E5FF),
        new RGBA(0xBA65C2FF),
        new RGBA(0x935099FF),
        new RGBA(0x6C3A70FF),
        new RGBA(0x452547FF)
    );
}
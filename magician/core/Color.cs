/*
*  Class for storing and manipulating colour data, with RGBA and HSLA support
*/
namespace Magician;
using Alg.Numeric;


public abstract class Color
{
    /*
    * Format:
    * r [0, 255]
    * g [0, 255]
    * b [0, 255]
    * h [0, 2Pi)
    * s [0, 1]
    * l [0, 1]
    * a [0, 255]
    */

    protected double a;
    public abstract double R { get; set; }
    public abstract double G { get; set; }
    public abstract double B { get; set; }
    public abstract double H { get; set; }
    public abstract double S { get; set; }
    public abstract double L { get; set; }
    public abstract Color Copy();
    public double A { get => a % 256; set => a = value; }
    public uint Hex()
    {
        return (uint)((byte)R << 24) + (uint)((byte)G << 16) + (uint)((byte)B << 8) + (uint)(A);
    }

    // Calculate a colour's hue angle from RGB values
    protected static double HueFromRGB(double r, double g, double b)
    {
        double h;
        r /= 255f;
        g /= 255f;
        b /= 255f;
        double colMax = Math.Max(r, Math.Max(g, b));
        double colMin = Math.Min(r, Math.Min(g, b));

        if (colMax == r)
        {
            h = (g - b) / (colMax - colMin);
        }
        else if (colMax == g)
        {
            h = 2f + (b - r) / (colMax - colMin);
        }
        else if (colMax == b)
        {
            h = 4f + (r - g) / (colMax - colMin);
        }
        else
        {
            throw new InvalidDataException($"Could not get hue from rgb: {r} {g} {b}");
        }

        // Convert from 0-1 to 0-2Pi
        h *= Math.PI / 3;
        if (h < 0)
        {
            h += 2 * Math.PI;
        }
        return h;
    }

    protected static double SaturationFromRGB(double r, double g, double b)
    {
        double l = LightnessFromRGB(r, g, b);
        double s;
        double rf = r / 255;
        double gf = g / 255;
        double bf = b / 255;
        double colMax = Math.Max(rf, Math.Max(gf, bf));
        double colMin = Math.Min(rf, Math.Min(gf, bf));

        if (l < 1)
        {
            s = (colMax - colMin) / (1 - Math.Abs(2 * l - 1));
        }
        else if (l == 1)
        {
            return 0;
        }
        else
        {
            throw new InvalidDataException($"ERROR: invalid lightness {l}");
        }

        return s;
    }

    protected static double LightnessFromRGB(double r, double g, double b)
    {
        float rf = (float)r / 255f;
        float gf = (float)g / 255f;
        float bf = (float)b / 255f;
        float colMax = Math.Max(rf, Math.Max(gf, bf));
        float colMin = Math.Min(rf, Math.Min(gf, bf));
        return 0.5f * (colMax + colMin);
    }

    public static double RedFromHSL(double h, double s, double l)
    {
        if (double.IsNaN(h))
        {
            Console.Write(0);
        }
        double hh = h % (2 * Math.PI);
        double r;
        double c = l * s;
        double x = c * (1 - Math.Abs((hh / (Math.PI / 3f)) % 2 - 1));
        double m = l - c;

        if (hh < Math.PI / 3)
        {
            r = c;
        }
        else if (hh < Math.PI - Math.PI / 3)
        {
            r = x;
        }
        else if (hh < Math.PI)
        {
            r = 0;
        }
        else if (hh < Math.PI + Math.PI / 3)
        {
            r = 0;
        }
        else if (hh < 2 * Math.PI - Math.PI / 3)
        {
            r = x;
        }
        else if (hh < 2 * Math.PI)
        {
            r = c;
        }
        else
        {
            throw new InvalidDataException($"Red: invalid phase {h}");
        }
        return r * 255;
    }

    public static double GreenFromHSL(double h, double s, double l)
    {
        double hh = h % (2 * Math.PI);
        double g;
        double c = l * s;
        double x = c * (1 - Math.Abs((hh / (Math.PI / 3f)) % 2 - 1));
        double m = l - c;

        if (hh < Math.PI / 3)
        {
            g = x;
        }
        else if (hh < Math.PI - Math.PI / 3)
        {
            g = c;
        }
        else if (hh < Math.PI)
        {
            g = c;
        }
        else if (hh < Math.PI + Math.PI / 3)
        {
            g = x;
        }
        else if (hh < 2 * Math.PI - Math.PI / 3)
        {
            g = 0;
        }
        else if (hh < 2 * Math.PI)
        {
            g = 0;
        }
        else
        {
            throw new InvalidDataException($"Green: invalid phase {h}");
        }
        return g * 255;
    }

    public static double BlueFromHSL(double h, double s, double l)
    {
        double hh = h % (2 * Math.PI);
        double b;
        double c = l * s;
        double x = c * (1 - Math.Abs((hh / (Math.PI / 3f)) % 2 - 1));
        double m = l - c;

        if (hh < Math.PI / 3)
        {
            b = 0;
        }
        else if (hh < Math.PI - Math.PI / 3)
        {
            b = 0;
        }
        else if (hh < Math.PI)
        {
            b = x;
        }
        else if (hh < Math.PI + Math.PI / 3)
        {
            b = c;
        }
        else if (hh < 2 * Math.PI - Math.PI / 3)
        {
            b = c;
        }
        else if (hh < 2 * Math.PI)
        {
            b = x;
        }
        else
        {
            throw new InvalidDataException($"Blue: invalid phase {h}");
        }
        return b * 255;
    }
}

public class RGBA : Color
{
    public RGBA(double r, double g, double b, double a)
    {
        R = Math.Abs(r) % 256;
        G = Math.Abs(g) % 256;
        B = Math.Abs(b) % 256;
        A = Math.Abs(a) % 256;
    }
    public RGBA(uint hex)
    {
        R = hex >> 24;
        G = (hex & 0x00ff0000) >> 16;
        B = (hex & 0x0000ff00) >> 8;
        A = (hex & 0x000000ff);
    }

    public override double R { get; set; }
    public override double G { get; set; }
    public override double B { get; set; }
    public override double H
    {
        get => HueFromRGB(R, G, B);
        set
        {
            HSLA converted = ToHSLA();
            converted.H = value;
            R = converted.R;
            G = converted.G;
            B = converted.B;
        }
    }
    public override double S
    {
        get => SaturationFromRGB(R, G, B);
        set
        {
            Color converted = ToHSLA();
            converted.S = value;
            R = converted.R;
            G = converted.G;
            B = converted.B;
        }
    }
    public override double L
    {
        get => LightnessFromRGB(R, G, B);
        set
        {
            Color converted = ToHSLA();
            converted.L = value;
            R = converted.R;
            G = converted.G;
            B = converted.B;
        }
    }
    public HSLA ToHSLA()
    {
        return new HSLA(H, S, L, A);
    }

    public override RGBA Copy()
    {
        return new RGBA(R, G, B, A);
    }

    public override string ToString()
    {
        return $"RGBA({R}, {G}, {B}, {A})";
    }
    public static RGBA Random(double? red = null, double? green = null, double? blue = null, double? alpha = null)
    {
        red ??= Rand.RNG.Next(256);
        green ??= Rand.RNG.Next(256);
        blue ??= Rand.RNG.Next(256);
        alpha ??= Rand.RNG.Next(256);
        return new RGBA((double)red, (double)green, (double)blue, (double)alpha);
    }
}

public class HSLA : Color
{
    public HSLA(double h, double s, double l, double a)
    {
        H = Math.Abs(h) % (2 * Math.PI);
        S = Math.Abs(s);
        L = Math.Abs(l);
        A = Math.Abs(a) % 256;
    }
    public override double H { get; set; }
    public override double S { get; set; }
    public override double L { get; set; }
    public override double R
    {
        get => RedFromHSL(H, S, L);
        set
        {
            Color converted = ToRGBA();
            converted.R = value;
            H = converted.H;
            S = converted.S;
            L = converted.L;
        }
    }
    public override double G
    {
        get => GreenFromHSL(H, S, L);
        set
        {
            Color converted = ToRGBA();
            converted.G = value;
            H = converted.H;
            S = converted.S;
            L = converted.L;
        }
    }
    public override double B
    {
        get => BlueFromHSL(H, S, L);
        set
        {
            Color converted = ToRGBA();
            converted.B = value;
            H = converted.H;
            S = converted.S;
            L = converted.L;
        }
    }

    public RGBA ToRGBA()
    {
        return new RGBA(R, G, B, A);
    }

    public override HSLA Copy()
    {
        return new HSLA(H, S, L, A);
    }

    public override string ToString()
    {
        return $"HSLA({H}, {S}, {L}, {A})";
    }

    public static HSLA Random(double? hue = null, double? saturation = null, double? lightness = null, double? alpha = null)
    {
        hue ??= Rand.RNG.NextDouble() * 2 * Math.PI;
        saturation ??= Rand.RNG.NextDouble();
        lightness ??= Rand.RNG.NextDouble();
        alpha ??= Rand.RNG.Next(256);
        return new HSLA((double)hue, (double)saturation, (double)lightness, (double)alpha);
    }
    public static HSLA RandomVisible()
    {
        return Random(saturation: 1, lightness: 1, alpha: 255);
    }
}

public class Palette
{
    int size;
    // Colours are ordered by darkness
    // The first colour is the background
    // The third colour is the foreground
    Color[] palette;

    public Palette(params Color[] colors)
    {
        size = colors.Length;
        palette = new Color[size];
        int i = 0;
        foreach (Color c in colors)
        {
            palette[i++] = c;
        }
    }

    public Color this[int i]
    {
        get => palette[i % size];
        set => palette[i % size] = value;
    }

    public void Rotate(double theta)
    {
        for (int i = 0; i < size; i++)
        {
            palette[i].H += theta;
        }
    }

    public Color FG { get => this[Math.Min(size, 2)]; }
    public Color BG { get => this[0]; }
}
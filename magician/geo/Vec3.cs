namespace Magician.Geo;
using Magician.Core;

using Silk.NET.Maths;

public class Vec3 : Vec
{
    public Vec3(double x, double y, double z) : base(x, y, z) { }

    /* Measured phase */
    public virtual double PhaseXY
    {
        get
        {
            double p = Math.Atan2(y.Get(), x.Get());
            p = p < 0 ? p + 2 * Math.PI : p;
            return p;
        }
        set
        {
            double mag = XYDist;
            x.Set(mag * Math.Cos(value));
            y.Set(mag * Math.Sin(value));
        }
    }
    public virtual double PhaseXZ
    {
        get
        {
            double p = Math.Atan2(z.Get(), x.Get());
            p = p < 0 ? p + 2 * Math.PI : p;
            return p;
        }
        set
        {
            double mag = XZDist;
            x.Set(mag * Math.Cos(value));
            z.Set(mag * Math.Sin(value));
        }
    }
    public virtual double PhaseYZ
    {
        get
        {
            double p = Math.Atan2(z.Get(), y.Get());
            p = p < 0 ? p + 2 * Math.PI : p;
            return p;
        }
        set
        {
            double mag = YZDist;
            y.Set(mag * Math.Cos(value));
            z.Set(mag * Math.Sin(value));
        }
    }

    public double XYDist
    {
        get => Math.Sqrt(x.Get() * x.Get() + y.Get() * y.Get());
    }
    public double YZDist
    {
        get => Math.Sqrt(z.Get() * z.Get() + y.Get() * y.Get());
    }
    public double XZDist
    {
        get => Math.Sqrt(x.Get() * x.Get() + z.Get() * z.Get());
    }

    public Node Render()
    {
        Node line = new(
            new Node(0, 0, 0),
            new Node(x.Get(), y.Get(), z.Get())
        );
        // TODO: draw the arrowhead
        Node arrowhead;
        return line;
    }
}
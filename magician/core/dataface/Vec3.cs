namespace Magician.Geo;
using Silk.NET.Maths;

public class Vec3 : Vec
{
    public Vec3(double x, double y, double z) : base(x, y, z) { }
    public Vec3(double[] xyz) : base(xyz)
    {
        if (xyz.Length != 3)
        {
            throw Scribe.Error($"Cannot store {xyz.Length} values in 3-vector");
        }
    }

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

    public Vec3 YawPitchRotated(double yaw, double pitch)
    {
        Matrix4X4<double> rotMat = Matrix4X4.CreateFromYawPitchRoll(yaw, pitch, 0);
        Vector3D<double> rotated = Vector3D.Transform(new Vector3D<double>(x.Get(), y.Get(), z.Get()), rotMat);
        return new(rotated.X, rotated.Y, rotated.Z);
    }

    public Multi Render()
    {
        Multi line = new(
            new Multi(0, 0, 0),
            new Multi(x.Get(), y.Get(), z.Get())
        );
        // TODO: draw the arrowhead
        Multi arrowhead;
        return line;
    }
}
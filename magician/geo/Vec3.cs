namespace Magician.Geo;
using Silk.NET.Maths;

public class Vec3 : Vec
{
    public Vec3(double x, double y, double z) : base(x, y, z) { }
    public Vec3(Quantity[] xyz) : base(xyz)
    {
        if (xyz.Length != 3)
        {
            throw Scribe.Error($"Cannot store {xyz.Length} values in 3-vector");
        }
    }
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
            double p = Math.Atan2(y.Evaluate(), x.Evaluate());
            p = p < 0 ? p + 2 * Math.PI : p;
            Scribe.Info(p);
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
            double p = Math.Atan2(z.Evaluate(), x.Evaluate());
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
            double p = Math.Atan2(z.Evaluate(), y.Evaluate());
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
        get => Math.Sqrt(x.Evaluate() * x.Evaluate() + y.Evaluate() * y.Evaluate());
    }
    public double YZDist
    {
        get => Math.Sqrt(z.Evaluate() * z.Evaluate() + y.Evaluate() * y.Evaluate());
    }
    public double XZDist
    {
        get => Math.Sqrt(x.Evaluate() * x.Evaluate() + z.Evaluate() * z.Evaluate());
    }

    public Vec3 YawPitchRotated(double yaw, double pitch)
    {
        Matrix4X4<double> rotMat = Matrix4X4.CreateFromYawPitchRoll(yaw, pitch, 0);
        //Matrix3X3<double> rot = Matrix3X3.C
        //Vector3D<double> v = Vector3D.Multiply<double>();
        Vector3D<double> rotated = Vector3D.Transform(new Vector3D<double>(x.Evaluate(), y.Evaluate(), z.Evaluate()), rotMat);
        return new(rotated.X, rotated.Y, rotated.Z);
    }

    public Multi Render()
    {
        Multi line = new(
            new Multi(0, 0, 0),
            new Multi(x.Evaluate(), y.Evaluate(), z.Evaluate())
        );
        // TODO: draw the arrowhead
        Multi arrowhead;
        return line;
    }

    // TODO: find a better pattern for these... if only we had int literal types
    public static Vec3 operator +(Vec3 v1, Vec3 v2)
    {
        return new(v1.vecArgs.Select((x, i) => x + v2.vecArgs[i]).ToArray());
    }
    public static Vec3 operator -(Vec3 v1, Vec3 v2)
    {
        return new(v1.vecArgs.Select((x, i) => x - v2.vecArgs[i]).ToArray());
    }
    // Scalar multiplication
    public static Vec3 operator *(Vec3 v1, double x)
    {
        return new(v1.vecArgs.Select(va => va.Evaluate() * x).ToArray());
    }
    public new Vec3 Normalized()
    {
        double m = Magnitude;
        List<double> news = new();
        foreach (Quantity q in vecArgs)
        {
            news.Add(q.Evaluate() / m);
        }
        return new(news.ToArray());
    }
}
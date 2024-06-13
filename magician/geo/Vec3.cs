namespace Magician.Geo;
using Magician.Core;
using Silk.NET.Maths;


public class Vec3 : Vec
{
    public Vec3(double x, double y, double z) : base(x, y, z) { }
    public Vec3((double x, double y, double z) xyz) : base(xyz.x, xyz.y, xyz.z) {}

    /* Measured phase */
    public virtual double PhaseXY
    {
        get
        {
            double p = Math.Atan2(y.Get(), x.Get());
            p = p < 0 ? p + 2 * Math.PI : p;
            return p;
        }
        //set
        //{
        //    double mag = XYDist;
        //    x.Set(mag * Math.Cos(value));
        //    y.Set(mag * Math.Sin(value));
        //}
    }
    public virtual double PhaseXZ
    {
        get
        {
            double p = Math.Atan2(z.Get(), x.Get());
            p = p < 0 ? p + 2 * Math.PI : p;
            return p;
        }
        //set
        //{
        //    double mag = XZDist;
        //    x.Set(mag * Math.Cos(value));
        //    z.Set(mag * Math.Sin(value));
        //}
    }
    public virtual double PhaseYZ
    {
        get
        {
            double p = Math.Atan2(z.Get(), y.Get());
            p = p < 0 ? p + 2 * Math.PI : p;
            return p;
        }
        //set
        //{
        //    double mag = YZDist;
        //    y.Set(mag * Math.Cos(value));
        //    z.Set(mag * Math.Sin(value));
        //}
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

    public Vec3 Cross(Vec3 a)
    {
        double cX, cY, cZ;
        cX = y.Get()*a.z.Get()-z.Get()*a.y.Get();
        cY = z.Get()*a.x.Get()-x.Get()*a.z.Get();
        cZ = x.Get()*a.y.Get()-y.Get()*a.x.Get();
        return new(cX, cY, cZ);
    }
    public void RotateX(double theta)
    {
        Quaternion<double> rotation = Quaternion<double>.CreateFromAxisAngle(new(1,0,0), theta);
        Vector3D<double> rotated = Vector3D.Transform<double>(new Vector3D<double>(x.Get(), y.Get(), z.Get()), rotation);
        x.Set(rotated.X);
        y.Set(rotated.Y);
        z.Set(rotated.Z);
    }
    public void RotateY(double theta)
    {
        Quaternion<double> rotation = Quaternion<double>.CreateFromAxisAngle(new(0,1,0), theta);
        Vector3D<double> rotated = Vector3D.Transform<double>(new Vector3D<double>(x.Get(), y.Get(), z.Get()), rotation);
        x.Set(rotated.X);
        y.Set(rotated.Y);
        z.Set(rotated.Z);
    }
    public void RotateZ(double theta)
    {
        Quaternion<double> rotation = Quaternion<double>.CreateFromAxisAngle(new(0,0,1), theta);
        Vector3D<double> rotated = Vector3D.Transform<double>(new Vector3D<double>(x.Get(), y.Get(), z.Get()), rotation);
        x.Set(rotated.X);
        y.Set(rotated.Y);
        z.Set(rotated.Z);
    }
    public static Vec3 operator +(Vec3 a, Vec3 b)
    {
        return new Vec3(IVal.Add(a.x, b.x).Get(), IVal.Add(a.y, b.y).Get(), IVal.Add(a.z, b.z).Get());
    }
    public static Vec3 operator -(Vec3 a, Vec3 b)
    {
        return new Vec3(IVal.Subtract(a.x, b.x).Get(), IVal.Subtract(a.y, b.y).Get(), IVal.Subtract(a.z, b.z).Get());
    }
    public static Vec3 operator *(Vec3 a, double b)
    {
        return new Vec3(IVal.Multiply(a.x, b).Get(), IVal.Multiply(a.y, b).Get(), IVal.Multiply(a.z, b).Get());
    }
    public static Vec3 operator /(Vec3 a, double b)
    {
        return new Vec3(IVal.Divide(a.x, b).Get(), IVal.Divide(a.y, b).Get(), IVal.Divide(a.z, b).Get());
    }
}
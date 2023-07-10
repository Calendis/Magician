using System;
// TODO: don't rely on these
using Silk.NET.Maths;

namespace Magician.Geo
{
    public class Vec
    {
        Quantity[] vecArgs;
        public int Dims => vecArgs.Length;
        public Vec(params double[] vals)
        {
            vecArgs = new Quantity[vals.Length];
            for (int i = 0; i < vals.Length; i++)
            {
                vecArgs[i] = new Quantity(vals[i]);
            }
        }
        public Vec(params Quantity[] qs)
        {
            vecArgs = qs;
        }

        public Quantity x
        {
            get => vecArgs[0];
        }
        public Quantity y
        {
            get => vecArgs[1];
        }
        public Quantity z
        {
            get => vecArgs[2];
        }

        /* Measured phase */
        public virtual double XYAngle
        {
            get
            {
                double p = Math.Atan2(y.Evaluate(), x.Evaluate());
                p = p < 0 ? p + 2 * Math.PI : p;
                return p;
            }
        }
        public virtual double XZAngle
        {
            get
            {
                double p = Math.Atan2(z.Evaluate(), x.Evaluate());
                p = p < 0 ? p + 2 * Math.PI : p;
                return p;
            }
        }
        public virtual double YZAngle
        {
            get
            {
                double p = Math.Atan2(z.Evaluate(), y.Evaluate());
                p = p < 0 ? p + 2 * Math.PI : p;
                return p;
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

        public static Vec operator +(Vec v1, Vec v2)
        {
            return new(v1.vecArgs.Select((x, i) => x + v2.vecArgs[i]).ToArray());
        }
        public static Vec operator -(Vec v1, Vec v2)
        {
            return new(v1.vecArgs.Select((x, i) => x - v2.vecArgs[i]).ToArray());
        }
        // Scalar multiplication
        public static Vec operator *(Vec v1, double x)
        {
            return new(v1.vecArgs.Select(va => va.Evaluate() * x).ToArray());
        }

        // TODO: how does this behave in n dimensions? (where n != 3)
        public Vec Cross(Vec v)
        {
            if (Dims != v.Dims)
            {
                throw Scribe.Error("Vectors must be of same length");
            }
            double[] results = new double[Dims];
            for (int i = 0; i < Dims; i++)
            {
                int j = (i + 1) % Dims;
                int k = (i + 2) % Dims;
                results[i] = vecArgs[j].Evaluate() * v.vecArgs[k].Evaluate() - vecArgs[k].Evaluate() * v.vecArgs[j].Evaluate();
            }
            return new Vec(results);
        }

        public double Magnitude()
        {
            double m = 0;
            for (int i = 0; i < Dims; i++)
            {
                m += Math.Pow(vecArgs[i].Evaluate(), 2);
            }
            return Math.Sqrt(m);
        }

        public Vec Normalized()
        {
            double m = Magnitude();
            List<double> news = new();
            foreach (Quantity q in vecArgs)
            {
                news.Add(q.Evaluate() / m);
            }
            return new(news.ToArray());
        }

        public double AxisDist(int axis0, int axis1)
        {
            double val0 = vecArgs[axis0].Evaluate();
            double val1 = vecArgs[axis1].Evaluate();
            return Math.Sqrt(val0 * val0 + val1 * val1);
        }

        public double AxisAngle(int axis0, int axis1)
        {
            double p = Math.Atan2(vecArgs[axis1].Evaluate(), vecArgs[axis0].Evaluate());
            p = p < 0 ? p + 2 * Math.PI : p;
            return p;
        }

        public Vec AxisRotated(int axis0, int axis1, double theta)
        {
            Vec n = new(vecArgs);
            double mag = AxisDist(axis0, axis1);
            double phase = AxisAngle(axis0, axis1);
            n.vecArgs[axis0].Set(mag * Math.Cos(theta + phase));
            n.vecArgs[axis1].Set(mag * Math.Sin(theta + phase));
            return n;
        }

        public Vec AxisRotatedTo(int axis0, int axis1, double theta)
        {
            Vec n = new(vecArgs);
            double mag = AxisDist(axis0, axis1);
            n.vecArgs[axis0].Set(mag * Math.Cos(theta));
            n.vecArgs[axis1].Set(mag * Math.Sin(theta));
            return n;
        }

        //public Vec YawPitchRotated(double yaw, double pitch)
        //{
        //    Vec yawed = AxisRotatedTo(0, 2, yaw);
        //    return yawed.AxisRotatedTo(1, 2, pitch);
        //}

        // TODO: remove pointless roll and rewrite this
        public Vec YawPitchRotated(double yaw, double pitch)
        {
            if (Dims != 3)
            {
                throw Scribe.Error("Not implemented");
            }
            Matrix4X4<double> rotMat = Matrix4X4.CreateFromYawPitchRoll(yaw, pitch, 0);
            //Matrix3X3<double> rot = Matrix3X3.C
            //Vector3D<double> v = Vector3D.Multiply<double>();
            Vector3D<double> rotated = Vector3D.Transform(new Vector3D<double>(x.Evaluate(), y.Evaluate(), z.Evaluate()), rotMat);
            return new(rotated.X, rotated.Y, rotated.Z);
        }

        public Multi Render()
        {
            if (Dims != 3)
            {
                throw Scribe.Error($"Tried to render {Dims}-vector, but only 3-vectors are renderable");
            }
            Multi line = new(
                new Multi(0, 0, 0),
                new Multi(x.Evaluate(), y.Evaluate(), z.Evaluate())
            );
            Multi arrowhead;
            return line;
        }

        public override string ToString()
        {
            string s = "(";
            foreach (Quantity q in vecArgs)
            {
                s += $"{q.Evaluate()}, ";
            }
            return s + ")";
        }

    }
}

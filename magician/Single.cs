/*
    A single is the basic "unit" math object in Magician
    from which more complex kinds of objects are derived
*/

namespace Magician
{
    public abstract class Single
    {
        protected double[] pos = new double[]{0,0};
        protected List<Driver> drivers = new List<Driver>();
        protected Multi? parent;

        public void SetX(double x)
        {
            double parentOffset = 0;
            if (parent is not null)
            {
                parentOffset = parent.XAbsolute(0);
            }
            pos[0] = x - parentOffset;
        }
        public void SetY(double x)
        {
            double parentOffset = 0;
            if (parent is not null)
            {
                parentOffset = parent.YAbsolute(0);
            }
            pos[1] = x - parentOffset;
        }
        public void IncrX(double x)
        {
            pos[0] += x;
        }
        public void IncrY(double x)
        {
            pos[1] += x;
        }
        public void SetPhase(double x)
        {
            double parentOffset = 0;
            if (parent is not null)
            {
                parentOffset = parent.Phase;
            }
            double m = Magnitude;
            pos[0] = m*Math.Cos(x - parentOffset);
            pos[1] = m*Math.Sin(x - parentOffset);
        }
        public void IncrPhase(double x)
        {
            SetPhase(Phase + x);
        }
        public void SetMagnitude(double x)
        {
            double parentOffset = 0;
            if (parent is not null)
            {
                parentOffset = parent.Phase;
            }
            double p = Phase;
            pos[0] = x*Math.Cos(p - parentOffset);
            pos[1] = x*Math.Sin(p - parentOffset);
        }
        public void IncrMagnitude(double x)
        {
            SetMagnitude(Magnitude + x);
        }

        public double Phase
        {
            get
            {
                double p = Math.Atan2(pos[1], pos[0]);
                p = p < 0 ? p + 2 * Math.PI : p;
                return p;
            }
        }
        public double Magnitude
        {
            get => Math.Sqrt(pos[0] * pos[0] + pos[1] * pos[1]);
        }
        public double XCartesian(double offset)
        {
            return Globals.winWidth / 2 + pos[0] + offset;
        }
        public double XAbsolute(double offset)
        {
            return pos[0] + offset;
        }
        public double YCartesian(double offset)
        {
            return Globals.winHeight / 2 - pos[1] - offset;
        }
        public double YAbsolute(double offset)
        {
            return pos[1] + offset;
        }
        public Point Point()
        {
            return new Point(pos[0], pos[1]);
        }

        // Raw screen coordinates
        public double WindowX
        {
            get => pos[0];
        }
        public double WindowY
        {
            get => pos[1];
        }

        public abstract void Draw(ref IntPtr renderer, double xOffset = 0, double yOffset = 0);

        public void AddDriver(Driver d)
        {
            drivers.Add(d);
        }

        public void Drive(params double[] x)
        {
            foreach (Driver d in drivers)
            {
                d.Drive(x);
            }
        }
    }
}
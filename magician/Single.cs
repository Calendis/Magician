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

        public void SetX(double x)
        {
            pos[0] = x;
        }
        public void SetY(double x)
        {
            pos[1] = x;
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
            double m = Magnitude;
            pos[0] = m*Math.Cos(x);
            pos[1] = m*Math.Sin(x);
        }
        public void IncrPhase(double x)
        {
            SetPhase(Phase + x);
        }
        public void SetMagnitude(double x)
        {
            double p = Phase;
            pos[0] = x*Math.Cos(p);
            pos[1] = x*Math.Sin(p);
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
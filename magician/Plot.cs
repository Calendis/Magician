/*
*  A Plot is used for drawing relations between two values. A plot is defined abstractly,
*  and is drawn by getting a Multi of Points interpolating the plot
*/

namespace Magician
{
    public class Plot : Multi, IMap, Drawable, Driveable
    {
        protected Driver toPlot;
        private double start;
        private double end;
        private double dx;
        
        // Create a plot with a defined position, driver, bounds, resolution, and colour
        public Plot(double x, double y, Driver d, double start, double end, double dx, Color c) : base(x, y, c, true, false, false)
        {
            //pos[0] = x;
            //pos[1] = y;
            toPlot = d;
            this.start = start;
            this.end = end;
            this.dx = dx;
            //this.col = c;
        }

        public void SetDx(double x)
        {
            dx = x;
        }
        public void IncrDx(double x)
        {
            dx += x;
        }

        // Get the value of a plot driver given inputs
        public new double Evaluate(params double[] x)
        {
            return toPlot.Evaluate(x);
        }
        
        // Linear interpolation of two values along a plot driver
        private Point[] interpolate(double x0, double x1)
        {            
            Point p0 = new Point(x0, Evaluate(x0));
            Point p1 = new Point(x1, Evaluate(x1));
            return new Point[] {p0, p1};
        }
        private Point[] interpolate(double x)
        {
            return interpolate(x, x + dx);
        }

        // Drawable render of a plot
        public Multi Interpolation()
        {
            //List<Point> points = new List<Point>();
            for (double x = start; x < end; x+=dx)
            {
                //Point[] ps = interpolate(x);
                //points.Add(ps[1]);
                Drawable d = interpolate(x)[0];
                d.Col = col;
                constituents.Add(d);
            }
            //Multi m = new Multi(pos[0], pos[1], col, true, false, false, points.ToArray());
            //return m;
            return this;
            // TODO: test this implementation to see if it actually works
        }

        public new void Draw(ref IntPtr renderer, double xOffset=0, double yOffset=0)
        {
            Interpolation().Draw(ref renderer, xOffset, yOffset);
        }

        public new Plot Driven(Func<double[], double> df, string s)
        {
            Driver d = new Driver(df);
            Action<double> output = Plot.StringMap(this, s);
            d.SetOutput(output);
            d.ActionString = s;
            drivers.Add(d);
            return this;
        }

        public static Action<double> StringMap(Plot p, String s)
        {
            Action<double> o;
            switch (s)
            {
                case "dx":
                    o = p.SetDx;
                    break;
                
                case "dx+":
                    o = p.IncrDx;
                    break;
                default:
                    o = Multi.StringMap(p, s);
                    break;
            }

            return o;
        }
    }
}
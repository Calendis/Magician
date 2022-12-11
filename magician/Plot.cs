/*
*  A Plot is used for drawing relations between two values. A plot is defined abstractly,
*  and is drawn by getting a Multi of Points interpolating the plot
*/

namespace Magician
{
    public class Plot : Multi, Drawable, Driveable
    {
        protected IMap toPlot;
        private double start;
        private double end;
        private double dx;
        
        // Create a plot with a defined position, driver, bounds, resolution, and colour
        public Plot(double x, double y, IMap im, double start, double end, double dx, Color c) : base(x, y, c, DrawMode.PLOT)
        {
            //pos[0] = x;
            //pos[1] = y;
            toPlot = im;
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
        private Multi[] interpolate(double x0, double x1)
        {            
            Multi p0 = Multi.Point(x0, Evaluate(x0), col);
            Multi p1 = Multi.Point(x1, Evaluate(x1), col);
            return new Multi[] {p0, p1};
        }
        private Multi[] interpolate(double x)
        {
            return interpolate(x, x + dx);
        }

        // Drawable render of a plot
        public Multi Interpolation()
        {
            List<Multi> points = new List<Multi>();
            for (double x = start; x < end; x+=dx)
            {
                Multi[] ps = interpolate(x);
                ps[0].Col = col;
                ps[1].Col = col;
                points.Add(ps[0]);

            }
            Multi m = new Multi(X.Evaluate(), Y.Evaluate(), col, DrawMode.PLOT, points.ToArray());
            return m;
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
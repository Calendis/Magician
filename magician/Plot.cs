/*
*  A Plot is used for drawing relations between two values. A plot is defined abstractly,
*  and is drawn by getting a Multi of Points interpolating the plot
*/

namespace Magician
{
    public class Plot : IMap
    {
        protected Driver toPlot;
        private double start;
        private double end;
        

        private double dx;
        private Color col;
        private double[] pos = new double[2];
        
        // Create a plot with a defined position, driver, bounds, resolution, and colour
        public Plot(double x, double y, Driver d, double start, double end, double dx, Color c)
        {
            pos[0] = x;
            pos[1] = y;
            toPlot = d;
            this.start = start;
            this.end = end;
            this.dx = dx;
            this.col = c;
        }

        // Get the value of a plot driver given inputs
        public double Evaluate(params double[] x)
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
            List<Point> points = new List<Point>();
            for (double x = start; x < end; x+=dx)
            {
                Point[] ps = interpolate(x);
                points.Add(ps[1]);
            }
            Multi m = new Multi(points.ToArray());
            m.Lined = true;
            return m;
        }
    }
}
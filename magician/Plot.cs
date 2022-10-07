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

        public double Evaluate(params double[] x)
        {
            return toPlot.Evaluate(x);
        }
        
        private Point[] interpolate(double x)
        {
            Point p0 = new Point(x, Evaluate(x));
            Point p1 = new Point(x + dx, Evaluate(x + dx));
            //Line l = new Line(p0, p1, col);
            return new Point[] {p0, p1};
        }
        private Point[] interpolate(double x0, double x1)
        {
            // TODO: Make plots point-based intead of line-based
            
            Point p0 = new Point(x0, Evaluate(x0));
            Point p1 = new Point(x1, Evaluate(x1));
            //Line l = new Line(p0, p1, col);
            return new Point[] {p0, p1};
        }

        public Multi Interpolation()
        {
            List<Point> points = new List<Point>();
            for (double x = start; x < end; x+=dx)
            {
                Point[] ps = interpolate(x);
                //points.AddRange(ps);
                //points.Add(ps[0]);
                points.Add(ps[1]);
            }
            Multi m = new Multi(points.ToArray());
            m.Lined = true;
            return m;
        }
    }
}
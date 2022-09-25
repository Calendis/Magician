namespace Magician
{
    public class Plot : Multi, IMap
    {
        protected Driver toPlot;
        private double start;
        private double end;
        protected Color col;
        public Color Col
        {
            get => col;
            set
            {
                col = value;
            }
        }

        private double dx;
        public Plot(double x, double y, Driver d, double start, double end, double dx, Color c)
        {
            pos[0] = x;
            pos[1] = y;
            toPlot = d;
            this.start = start;
            this.end = end;
            this.dx = dx;
            this.col = c;
            constituents.AddRange(Interpolation().Constituents);
        }

        // Default plot of nothing
        public Plot()
        {
            toPlot = null;
            start = 0;
            end = 0;
            col = new Color();
        }

        public double Evaluate(params double[] x)
        {
            return toPlot.Evaluate(x);
        }
        
        private Line interpolate(double x)
        {
            Point p0 = new Point(x, Evaluate(x));
            Point p1 = new Point(x + dx, Evaluate(x + dx));
            Line l = new Line(p0, p1);
            //l.SetX(pos[0]);
            //l.SetY(pos[1]);
            return l;
        }
        private Line interpolate(double x0, double x1)
        {
            Point p0 = new Point(x0, Evaluate(x0));
            Point p1 = new Point(x1, Evaluate(x1));
            Line l = new Line(p0, p1);
            //l.SetX(pos[0]);
            //l.SetY(pos[1]);
            return l;
        }

        public Multi Interpolation()
        {
            List<Line> lines = new List<Line>();
            for (double x = start; x < end; x+=dx)
            {
                lines.Add(interpolate(x));
            }
            return new Multi(lines.ToArray());
        }

        
        public override void Draw(ref IntPtr renderer, double xOffset = 0, double yOffset = 0)
        {
            base.Draw(ref renderer, pos[0], pos[1]);
        }
        
    }
}
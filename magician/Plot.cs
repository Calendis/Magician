namespace Magician
{
    public class Plot : Multi, IMap
    {
        protected Driver toPlot;
        private double start;
        private double end;
        protected Color col;

        public Plot(Driver d, double start, double end, Color c)
        {
            toPlot = d;
            this.start = start;
            this.end = end;
            this.col = c;
        }

        // Default plot of nothing
        public Plot()
        {
            toPlot = new Driver();
            start = 0;
            end = 0;
            col = new Color(0x00e9f5ff);
        }

        public double Evaluate(params double[] x)
        {
            return toPlot.Evaluate(x);
        }
        public Line interpolate(double x, double dx)
        {
            throw new NotImplementedException();
        }
    }

}
namespace Magician
{
    public class Plot : Multi, Map
    {
        protected Driver toPlot;
        private double start;
        private double end;

        public Plot(Driver d, double start, double end)
        {
            toPlot = d;
            this.start = start;
            this.end = end;
        }

        // Default plot of nothing
        public Plot()
        {
            toPlot = new Driver();
            start = 0;
            end = 0;
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
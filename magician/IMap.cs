namespace Magician
{
    public interface IMap
    {
        public abstract double Evaluate(params double[] x);
        public Multi MultisAlong(double lb, double ub, double dx, Multi tmp, double xOffset=0, double yOffset=0, Func<double, double>? truth=null, double threshold=0)
        {
            if (truth is null)
            {
                truth = x => 1;
            }
            Multi m = new Multi(xOffset, yOffset);
            for (double i = lb; i < ub; i+=dx)
            {
                if (truth.Invoke(i) >= threshold)
                {
                    tmp.parent = m;
                    m.Add(tmp.Copy().Positioned(i+tmp.X.Evaluate(), Evaluate(i)+tmp.Y.Evaluate()));
                }
            }
            m.parent = Multi.Origin;
            return m.DrawFlags(DrawMode.PLOT);
        }
    }
}
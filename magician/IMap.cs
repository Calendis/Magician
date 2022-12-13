namespace Magician
{
    public interface IMap
    {
        public abstract double Evaluate(params double[] x);
        public Multi MultisAlong(double lb, double ub, double dx, IMap truth, double threshold, Multi tmp)
        {
            Multi m = new Multi(0, 0);
            for (double i = lb; i < ub; i+=dx)
            {
                if (truth.Evaluate(i) >= threshold)
                {
                    tmp.parent = m;
                    m.Add(tmp.Copy().Positioned(i, Evaluate(i)));
                }
            }
            m.parent = Multi.Origin;
            return m.DrawFlags(DrawMode.PLOT);
        }
    }
}
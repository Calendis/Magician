namespace Magician
{
    public interface IMap
    {
        public abstract double Evaluate(params double[] x);
        public Multi MultisAlong(double lb, double ub, double dx, IMap f, IMap truth, double threshold, Multi tmp)
        {
            Multi m = new Multi(0, 0);
            for (double i = lb; i < ub; i+=dx)
            {
                if (truth.Evaluate(i) >= threshold)
                {
                    m.Add(tmp.Copy().Positioned(i, f.Evaluate(i)));
                }
            }
            return m;
        }
    }
}
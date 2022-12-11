namespace Magician
{
    public interface IMap
    {
        public abstract double Evaluate(params double[] x);
        //public abstract Multi MultisAlong(double lb, double ub, IMap truth, Multi tmp);
    }
}
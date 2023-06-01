namespace Magician.Algo;

internal class NDCounter
{

    double[] mins;
    double[] maxs;
    double counterMax = 1;
    double current = 0;
    double[] vals;
    public double Get(int n) => vals[n];
    double res;
    public bool done = false;
    public bool Done => done;
    public NDCounter(double res, params Tuple<double, double>[] ranges)
    {
        this.res = res;
        int l = ranges.Length;
        mins = new double[l]; maxs = new double[l]; vals = new double[l];

        int i = 0;
        foreach (Tuple<double, double> t in ranges)
        {
            mins[i] = t.Item1;
            vals[i] = mins[i];
            maxs[i] = t.Item2;
            counterMax *= (maxs[i] - mins[i]) / res;
            i++;
        }
    }

    public void Increment()
    {
        int pos = 0;
        bool incremented = false;
        while (!incremented)
        {
            vals[pos] += res;
            if (vals[pos] >= maxs[pos])
            {
                vals[pos] = mins[pos];
                pos++;
            }
            else
            {
                incremented = true;
            }
        }
        
        current += res;
        if (current >= counterMax)
        {
            done = true;
        }
    }
}